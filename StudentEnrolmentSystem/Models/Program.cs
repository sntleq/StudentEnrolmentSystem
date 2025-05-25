namespace StudentEnrolmentSystem.Models;

public class Program
{
    public int ProgId { get; set; }
    public required string ProgTitle { get; set; }

    public ICollection<Student> Students { get; set; }
    public ICollection<Curriculum> Curricula { get; set; }
    public ICollection<Room> Rooms { get; set; }
}