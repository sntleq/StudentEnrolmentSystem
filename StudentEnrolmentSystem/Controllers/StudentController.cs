using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class StudentController(
    IConfiguration config, ILogger<AdminController> logger,
    StudentApiController studentApi, ProgramApiController programApi
    ) : Controller
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    public IActionResult Index()
    {
        return RedirectToAction("Schedule");
    }
    
    public IActionResult Schedule()
    {
        ViewBag.StudId = HttpContext.Session.GetInt32("StudId");
        ViewBag.StudFirstName = HttpContext.Session.GetString("StudFirstName");
        
        ViewBag.ActiveEnrolments = studentApi.GetActiveEnrolments(
            (int)ViewBag.StudId, 
            (int)HttpContext.Session.GetInt32("AyId")!, 
            (int)HttpContext.Session.GetInt32("SemId")!
        ).Result;
        
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
        {
            return RedirectToAction("Student", "Auth");
        }
        return View("~/Views/Student/Profile.cshtml");
    }
}