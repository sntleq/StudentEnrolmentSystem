namespace StudentEnrolmentSystem.Models.Dto;

public class ChooseSchedulesDto
{
    public int CurId { get; set; }
    public List<int> SchedIds { get; set; } = [];
    public int? CrsCount { get; set; }
    public int StudId { get; set; }
    public int? SemId { get; set; }
}