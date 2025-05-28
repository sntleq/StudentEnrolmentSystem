using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class DeleteDto
{
    [Required(ErrorMessage = "ID is required")]
    public required int Id { get; set; }
}