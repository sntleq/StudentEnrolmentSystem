namespace StudentEnrolmentSystem.Models;

public class Room
{
    public int RoomId { get; set; }
    public required string RoomCode { get; set; }
    public int ProgId { get; set; }

    public Program Program { get; set; }
    public ICollection<CourseSchedule> CourseSchedules { get; set; }
}