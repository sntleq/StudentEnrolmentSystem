using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class AdminController(
    IConfiguration config, ILogger<AdminController> logger,
    CourseApiController courseApi, ProgramApiController programApi 
    ) : Controller
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    public IActionResult Index()
    {
        return RedirectToAction("Courses");
    }

    public IActionResult Courses()
    {
        ViewBag.Courses = courseApi.GetCourses().Result;
        ViewBag.Categories = courseApi.GetCategories().Result;
        return View("~/Views/Admin/Courses.cshtml");
    }

    public IActionResult Programs()
    {
        ViewBag.Programs = programApi.GetPrograms().Result;
        return View("~/Views/Admin/Programs.cshtml");
    }

    public IActionResult Curricula()
    {
        return View("~/Views/Admin/Curricula.cshtml");
    }
}