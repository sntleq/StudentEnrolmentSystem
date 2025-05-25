namespace StudentEnrolmentSystem.Models;

public class Student
{
    public int StudId { get; set; }
    public required string StudCode { get; set; }
    public required string StudFirstName { get; set; }
    public required string StudLastName { get; set; }
    public required string StudContactNum { get; set; }
    public required string StudEmail { get; set; }
    public required string StudPassword { get; set; }
    public DateTime StudDob { get; set; }
    public required string StudAddress { get; set; }
    public bool StudIsFirstGen { get; set; }
    public int ProgId { get; set; }
    public required string StudYrLvl { get; set; }
    public required string StudStatus { get; set; }

    public Program Program { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; }
}