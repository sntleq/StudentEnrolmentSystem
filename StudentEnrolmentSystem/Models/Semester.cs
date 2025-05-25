namespace StudentEnrolmentSystem.Models;

public class Semester
{
    public int SemId { get; set; }
    public required string SemName { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; }
}