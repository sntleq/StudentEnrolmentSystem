namespace StudentEnrolmentSystem.Models;

public class CourseDependency
{
    public int DepId { get; set; }
    public int CrsId { get; set; }
    public int CrsPreqId { get; set; }
}