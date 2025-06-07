using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models.Dto;

public class IdDto
{
    [Required(ErrorMessage = "ID is required")]
    public required int Id { get; set; }
    public int? TypeId { get; set; }
}