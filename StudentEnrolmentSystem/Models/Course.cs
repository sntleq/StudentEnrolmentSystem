using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models;

public class Course
{
    public int CrsId { get; set; }
    
    [Required(ErrorMessage = "Course code is required.")]
    public required string CrsCode { get; set; }
    
    [Required(ErrorMessage = "Course title is required.")]
    public required string CrsTitle { get; set; }
    
    [Required(ErrorMessage = "Number of units is required.")]
    public int CrsUnits { get; set; }
    
    [Required(ErrorMessage = "This is required.")]
    public int CrsHrsLec { get; set; }
    
    [Required(ErrorMessage = "This is required.")]
    public int CrsHrsLab { get; set; }
    
    [Required(ErrorMessage = "Course category is required.")]
    public int CatgId { get; set; }
}