using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class TimeMachineDto
{
    [Required(ErrorMessage = "Academic year is required")]
    public int AyId { get; set; }
    
    [Required(ErrorMessage = "Semester is required")]
    public int SemId { get; set; }
}