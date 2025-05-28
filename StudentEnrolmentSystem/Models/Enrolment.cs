namespace StudentEnrolmentSystem.Models;

public class Enrolment
{
    public int EnrlId { get; set; }
    public int StudId { get; set; }
    public int ProgId { get; set; }
    public int SchedId { get; set; }
    public int AyId { get; set; }
    public int SemId { get; set; }
    public bool EnrlIsCompleted { get; set; }
}