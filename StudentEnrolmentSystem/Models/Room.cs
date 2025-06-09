using System.ComponentModel.DataAnnotations;

namespace StudentEnrolmentSystem.Models;

public class Room
{
    public int RoomId { get; set; }
    
    [Required(ErrorMessage = "Room code is required")]
    public required string RoomCode { get; set; }
    
    public int? ProgId { get; set; }
    
    public bool RoomIsActive { get; set; }
}