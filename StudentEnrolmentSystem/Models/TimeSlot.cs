namespace StudentEnrolmentSystem.Models;

public class TimeSlot
{
    public int SlotId { get; set; }
    public int SlotDay { get; set; }
    public TimeSpan SlotTimeStart { get; set; }
    public TimeSpan SlotTimeEnd { get; set; }

    public ICollection<CourseSchedule> CourseSchedules { get; set; } = new List<CourseSchedule>();
}
