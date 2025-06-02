using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class ProgramHeadAssignDto
{
    [Required(ErrorMessage = "Program is required")]
    public required int ProgId { get; set; }
    
    public int? HeadId { get; set; }
    
    public string? HeadFirstName { get; set; }
    public string? HeadLastName { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? HeadEmail { get; set; }
    
    public string? HeadPassword { get; set; }
}