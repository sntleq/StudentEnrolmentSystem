namespace StudentEnrolmentSystem.Models;

public class Course
{
    public int CrsId { get; set; }
    public required string CrsCode { get; set; }
    public required string CrsTitle { get; set; }
    public int CrsUnits { get; set; }
    public int CrsHraLec { get; set; }
    public int CrsHrsLab { get; set; }
    public int TchrId { get; set; }
    public int CatgId { get; set; }
}