namespace StudentEnrolmentSystem.Models;

public class CurriculumCourse
{
    public int CurCrsId { get; set; }
    public int CurId { get; set; }
    public int CrsId { get; set; }

    public Curriculum Curriculum { get; set; }
    public Course Course { get; set; }
}