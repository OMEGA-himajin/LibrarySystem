namespace LibrarySystem.Models;

public class Book
{
    public long BookId { get; set; }
    public string? ISBN { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public short? PublishedYear { get; set; }
    public string? Genre { get; set; }
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public int Status { get; set; }
    public long? CurrentLendingId { get; set; }
    public DateTime? CurrentDueDate { get; set; }
}
