using LibrarySystem.Common;
using LibrarySystem.Models;
using LibrarySystem.Repositories;

namespace LibrarySystem.Services;

public class LibraryService
{
    private readonly BookRepository _bookRepository;
    private readonly UserRepository _userRepository;
    private readonly LendingRepository _lendingRepository;

    private const int MaxLendingCount = 5;
    private const int LendingDays = 14;

    public LibraryService(
        BookRepository bookRepository,
        UserRepository userRepository,
        LendingRepository lendingRepository)
    {
        _bookRepository = bookRepository;
        _userRepository = userRepository;
        _lendingRepository = lendingRepository;
    }

    public List<Book> SearchBooks(string keyword, int? status, int page, int pageSize)
    {
        return _bookRepository.Search(keyword, status, page, pageSize);
    }

    public Book? GetBookForDisplay(long bookId)
    {
        return _bookRepository.FindById(bookId);
    }

    public void AddBook(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title))
        {
            throw new ArgumentException("書名は必須です。");
        }

        if (string.IsNullOrWhiteSpace(book.Author))
        {
            throw new ArgumentException("著者は必須です。");
        }

        _bookRepository.Add(book);
    }

    public User? GetUserByCode(string userCode)
    {
        return _userRepository.FindByCode(userCode);
    }

    public User? FindUserByCode(string userCode)
    {
        return _userRepository.FindByCode(userCode);
    }

    public void SaveUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.UserCode))
        {
            throw new ArgumentException("利用者コードは必須です。");
        }

        if (string.IsNullOrWhiteSpace(user.FullName))
        {
            throw new ArgumentException("氏名は必須です。");
        }

        if (user.UserId == 0)
        {
            _userRepository.Add(user);
            return;
        }

        _userRepository.Update(user);
    }

    public int GetUserActiveLendingCount(int userId)
    {
        return _lendingRepository.CountActiveLendingsByUserId(userId);
    }

    public bool HasOverdue(int userId)
    {
        return _lendingRepository.ExistsOverdueByUserId(userId);
    }

    public void LendBook(long bookId, string userCode, int librarianId)
    {
        var book = _bookRepository.FindById(bookId) ?? throw new InvalidOperationException("対象の蔵書が存在しません。");
        var user = _userRepository.FindByCode(userCode) ?? throw new InvalidOperationException("対象の利用者が存在しません。");

        if (GetUserActiveLendingCount(user.UserId) >= MaxLendingCount)
        {
            throw new InvalidOperationException($"同時貸出上限は {MaxLendingCount} 冊です。");
        }

        if (book.Status == 1)
        {
            throw new InvalidOperationException("この本は貸出中です。");
        }

        var otherUserReservation = _lendingRepository.GetActiveReservationByBookIdExcludingUser(bookId, user.UserId);
        if (otherUserReservation is not null)
        {
            throw new InvalidOperationException("他利用者の有効予約があるため貸出できません。");
        }

        using var conn = Db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            var lending = new Lending
            {
                BookId = bookId,
                UserId = user.UserId,
                LibrarianId = librarianId,
                LentAt = DateTime.Now,
                DueDate = DateTime.Now.AddDays(LendingDays)
            };

            var lendingId = _lendingRepository.AddLending(lending, conn, tx);
            _bookRepository.MarkLent(bookId, lendingId, conn, tx);

            var reservation = _lendingRepository.GetActiveReservationByBookId(bookId, conn, tx);
            if (reservation is not null && reservation.UserId == user.UserId)
            {
                _lendingRepository.CompleteReservation(reservation.ReservationId, conn, tx);
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public (Book Book, User? Borrower) GetReturnTarget(long bookId)
    {
        var book = _bookRepository.FindById(bookId) ?? throw new InvalidOperationException("対象の蔵書が存在しません。");
        var borrower = _lendingRepository.GetBorrowerByBookId(bookId);
        return (book, borrower);
    }

    public string? ReturnBook(long bookId, int librarianId)
    {
        var lending = _lendingRepository.GetActiveLendingByBookId(bookId);
        if (lending is null)
        {
            throw new InvalidOperationException("この本は現在貸出中ではありません。");
        }

        using var conn = Db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            _lendingRepository.ReturnBook(bookId, librarianId, conn, tx);
            _bookRepository.MarkAvailable(bookId, conn, tx);

            string? notice = null;
            var reservation = _lendingRepository.GetActiveReservationByBookId(bookId, conn, tx);
            if (reservation is not null)
            {
                _lendingRepository.SetReservationHold(reservation.ReservationId, DateTime.Now.AddDays(3), conn, tx);
                _bookRepository.MarkReserved(bookId, conn, tx);

                var reservedUser = _userRepository.FindById(reservation.UserId, conn, tx);
                notice = reservedUser is null
                    ? "予約者への通知対象があります。"
                    : $"予約者 {reservedUser.FullName}（{reservedUser.UserCode}）へ連絡してください。";
            }

            tx.Commit();
            return notice;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public void ReserveBook(long bookId, string userCode, int librarianId)
    {
        var book = _bookRepository.FindById(bookId) ?? throw new InvalidOperationException("対象の蔵書が存在しません。");
        var user = _userRepository.FindByCode(userCode) ?? throw new InvalidOperationException("対象の利用者が存在しません。");

        if (_lendingRepository.ExistsActiveReservationByBookIdAndUserId(book.BookId, user.UserId))
        {
            throw new InvalidOperationException("同一利用者の重複予約はできません。");
        }

        var reservation = new Reservation
        {
            BookId = book.BookId,
            UserId = user.UserId,
            LibrarianId = librarianId,
            ReservedAt = DateTime.Now,
            Status = 0
        };

        _lendingRepository.AddReservation(reservation);
    }
}
