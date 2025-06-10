namespace StudentEnrolmentSystem.Models.Dto;

public class ChooseCoursesDto
{
    public int CurId { get; set; }
    public List<int> CrsIds { get; set; } = [];
    public int StudId { get; set; }
}