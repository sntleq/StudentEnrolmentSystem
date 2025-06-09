namespace StudentEnrolmentSystem.Models;

public class CourseSchedule
{
    public int SchedId { get; set; }
    public int CrsId { get; set; }
    public required string SchedCode { get; set; }
    public int? RoomId { get; set; }
    public int SchedCapacity { get; set; }
    public int TchrId { get; set; }
    public required string SchedDescription { get; set; }
    
    public bool SchedIsActive { get; set; }
}