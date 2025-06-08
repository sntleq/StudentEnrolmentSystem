using Microsoft.AspNetCore.Mvc;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class RoomController(
    RoomApiController roomApi, ProgramApiController programApi
) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Rooms", "Admin");
    }

    public IActionResult Add()
    {
        ViewBag.Programs = programApi.GetPrograms().Result;
        return View("~/Views/Room/AddRoom.cshtml");
    }
    
    public IActionResult Update(int roomId)
    {
        var room = roomApi.GetRooms().Result.FirstOrDefault(x => x.RoomId == roomId);
        ViewBag.Programs = programApi.GetPrograms().Result;
        return View("~/Views/Room/EditRoom.cshtml", room);
    }
    
    public IActionResult Delete(int roomId)
    {
        var dto = new IdDto { Id = roomId };
        return View("~/Views/Room/DeleteRoom.cshtml", dto);
    }
}