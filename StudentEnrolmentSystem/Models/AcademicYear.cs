namespace StudentEnrolmentSystem.Models;

public class AcademicYear
{
    public int AyId { get; set; }
    public int AyStartYr { get; set; }
    public int AyEndYr { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; }
    public ICollection<Curriculum> Curricula { get; set; }
}