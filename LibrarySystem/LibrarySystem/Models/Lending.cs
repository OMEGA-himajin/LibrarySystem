namespace LibrarySystem.Models;

public class Lending
{
    public long LendingId { get; set; }
    public long BookId { get; set; }
    public int UserId { get; set; }
    public int LibrarianId { get; set; }
    public DateTime LentAt { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int? ReturnLibrarianId { get; set; }
}
