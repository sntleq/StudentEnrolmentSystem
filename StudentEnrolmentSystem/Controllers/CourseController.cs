using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class CourseController(
    IConfiguration config, ILogger<AdminController> logger,
    CourseApiController courseApi
    ) : Controller
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    public IActionResult Index()
    {
        return RedirectToAction("Courses", "Admin");
    }

    public IActionResult Add()
    {
        ViewBag.Categories = courseApi.GetCategories().Result;
        return View("~/Views/Course/AddCourse.cshtml");
    }
    
    public IActionResult Update(int crsId)
    {
        var course = courseApi.GetCourses().Result.FirstOrDefault(x => x.CrsId == crsId);
        ViewBag.Categories = courseApi.GetCategories().Result;
        return View("~/Views/Course/EditCourse.cshtml", course);
    }
    
    public IActionResult Delete(int crsId)
    {
        var dto = new DeleteDto { Id = crsId };
        return View("~/Views/Course/DeleteCourse.cshtml", dto);
    }
}