namespace StudentEnrolmentSystem.Models;

public class Curriculum
{
    public int CurId { get; set; }
    public int ProgId { get; set; }
    public int AyId { get; set; }

    public Program Program { get; set; }
    public AcademicYear AcademicYear { get; set; }
    public ICollection<CurriculumCourse> CurriculumCourses { get; set; }
}