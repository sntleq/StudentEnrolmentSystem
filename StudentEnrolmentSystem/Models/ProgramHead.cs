namespace StudentEnrolmentSystem.Models;

public class ProgramHead
{
    public int HeadId { get; set; }
    public required string HeadFirstName { get; set; }
    public required string HeadLastName { get; set; }
    public required string HeadEmail { get; set; }
    public required string HeadPassword { get; set; }
    public bool HeadIsActive { get; set; }
    public int? ProgId { get; set; }
}