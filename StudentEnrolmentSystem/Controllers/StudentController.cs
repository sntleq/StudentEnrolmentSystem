using Microsoft.AspNetCore.Mvc;
namespace StudentEnrolmentSystem.Controllers;

public class StudentController(
    StudentApiController studentApi, ProgramApiController programApi,
    CourseApiController courseApi,ScheduleApiController scheduleApi
    ) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Schedule");
    }
    
    public IActionResult Schedule()
    {
        ViewBag.StudId = HttpContext.Session.GetInt32("StudId");
        ViewBag.StudFirstName = HttpContext.Session.GetString("StudFirstName");
        
        ViewBag.ActiveEnrolments = studentApi.GetActiveEnrolments(
            ViewBag.StudId, 
            HttpContext.Session.GetInt32("AyId") ?? 2, 
            HttpContext.Session.GetInt32("SemId") ?? 1
        ).Result;
        
        ViewBag.Courses = courseApi.GetCourses().Result;
        ViewBag.Schedules = scheduleApi.GetSchedules().Result;
        ViewBag.Sessions = scheduleApi.GetSessions().Result;
        ViewBag.TimeSlots = scheduleApi.GetTimeSlots().Result;
        return View("~/Views/Student/Schedule.cshtml");
    }
    
    public IActionResult History()
    {
        ViewBag.ActiveEnrolments = studentApi.GetEnrolments(HttpContext.Session.GetInt32("StudId") ?? 0).Result;
        return View("~/Views/Student/History.cshtml");
    }
    
    public IActionResult Profile()
    {
        ViewBag.Student = studentApi.GetStudent(HttpContext.Session.GetInt32("StudId") ?? 0).Result;
        ViewBag.Programs = programApi.GetPrograms().Result;
        
        if (ViewBag.Student == null)
            return RedirectToAction("Student", "Auth");
        
        return View("~/Views/Student/Profile.cshtml");
    }
}