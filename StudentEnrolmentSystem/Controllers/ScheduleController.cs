using Microsoft.AspNetCore.Mvc;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class ScheduleController(
    CourseApiController courseApi, FacultyApiController facultyApi,
    RoomApiController roomApi, ScheduleApiController scheduleApi
) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Schedule", "Admin");
    }

    public IActionResult Add(int crsId)
    {
        ViewBag.Course = courseApi.GetCourses().Result.FirstOrDefault(c => c.CrsId == crsId);
        ViewBag.Teachers = facultyApi.GetTeachers().Result;
        ViewBag.Rooms = roomApi.GetRooms().Result;
        ViewBag.TimeSlots = scheduleApi.GetTimeSlots().Result;
        ViewBag.Schedules = scheduleApi.GetSchedules().Result;
        ViewBag.Sessions = scheduleApi.GetSessions().Result;
        return View("~/Views/Schedule/AddSchedule.cshtml");
    }
    
    public IActionResult Update(int schedId)
    {
        var schedule = scheduleApi.GetSchedules().Result.FirstOrDefault(s => s.SchedId == schedId);
        ViewBag.Course = courseApi.GetCourses().Result.FirstOrDefault(c => c.CrsId == schedule!.CrsId);
        
        ViewBag.Teachers = facultyApi.GetTeachers().Result;
        ViewBag.Rooms = roomApi.GetRooms().Result;
        ViewBag.TimeSlots = scheduleApi.GetTimeSlots().Result;

        var dto = new ScheduleCreateDto
        {
            SchedId = schedule!.SchedId,
            SchedCode = schedule.SchedCode,
            CrsId = schedule.CrsId,
            TchrId = schedule.TchrId,
            RoomId = schedule.RoomId,
            SchedCapacity = schedule.SchedCapacity,
            SlotIds = [],
        };
        return View("~/Views/Schedule/EditSchedule.cshtml", dto);
    }
    
    public IActionResult Delete(int schedId)
    {
        var dto = new IdDto { Id = schedId };
        return View("~/Views/Schedule/DeleteSchedule.cshtml", dto);
    }
}