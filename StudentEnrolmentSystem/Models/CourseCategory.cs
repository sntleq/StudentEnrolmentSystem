namespace StudentEnrolmentSystem.Models;

public class CourseCategory
{
    public int CatgId { get; set; }
    public required string CatgName { get; set; }

    public ICollection<Course> Courses { get; set; }
}