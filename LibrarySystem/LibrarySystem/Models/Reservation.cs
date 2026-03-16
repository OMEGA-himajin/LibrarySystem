namespace LibrarySystem.Models;

public class Reservation
{
    public long ReservationId { get; set; }
    public long BookId { get; set; }
    public int UserId { get; set; }
    public int LibrarianId { get; set; }
    public DateTime ReservedAt { get; set; }
    public byte Status { get; set; }
    public DateTime? NotifiedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
