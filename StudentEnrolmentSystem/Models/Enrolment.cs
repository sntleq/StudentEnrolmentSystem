namespace StudentEnrolmentSystem.Models;

public class Enrolment
{
    public int EnrlId { get; set; }
    public int StudId { get; set; }
    public required string EnrlYrLvl { get; set; }
    public int ProgId { get; set; }
    public int SchedId { get; set; }
    public int AyId { get; set; }
    public int SemId { get; set; }
    public bool EnrlIsCompleted { get; set; }

    public Student Student { get; set; }
    public Program Program { get; set; }
    public CourseSchedule CourseSchedule { get; set; }
    public AcademicYear AcademicYear { get; set; }
    public Semester Semester { get; set; }
}