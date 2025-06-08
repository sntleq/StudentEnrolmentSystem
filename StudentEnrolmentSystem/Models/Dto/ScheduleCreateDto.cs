using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class ScheduleCreateDto
{
    public int SchedId { get; set; }
    public int CrsId { get; set; }
    
    [Required(ErrorMessage = "Section code is required")]
    public required string SchedCode { get; set; }
    
    [Required(ErrorMessage = "Teacher is required")]
    public int TchrId { get; set; }

    public int? RoomId { get; set; }
    
    [Required(ErrorMessage = "Class capacity is required")]
    public int SchedCapacity { get; set; }

    public List<int> SlotIds { get; set; } = [];
}