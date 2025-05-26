namespace StudentEnrolmentSystem.Models;

public class Teacher
{
    public int TchrId { get; set; }
    public required string TchrFirstName { get; set; }
    public required string TchrLastName { get; set; }
    public required string TchrEmail { get; set; }
    public required string TchrPassword { get; set; }
    public bool TchrIsActive { get; set; }
}