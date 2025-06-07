using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class ProgramHeadAssignDto
{
    [Required(ErrorMessage = "Program is required")]
    public required int ProgId { get; set; }
    
    [Required(ErrorMessage = "Program Head is required")]
    public required int HeadId { get; set; }
}