using Microsoft.AspNetCore.Mvc;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class RoomController(
    RoomApiController roomApi
) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Rooms", "Admin");
    }

    public IActionResult Add()
    {
        return View("~/Views/Program/AddProgram.cshtml");
    }
    
    
    public IActionResult Update(int roomId)
    {
        var room = roomApi.GetRooms().Result.FirstOrDefault(x => x.RoomId == roomId);
        return View("~/Views/Program/EditProgram.cshtml");
    }
    
    public IActionResult Delete(int progId)
    {
        var dto = new IdDto { Id = progId };
        return View("~/Views/Program/DeleteProgram.cshtml", dto);
    }
}