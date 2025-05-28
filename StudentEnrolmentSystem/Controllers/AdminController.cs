using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class AdminController : Controller
{
    private readonly string _connectionString;
    private readonly ILogger<AdminController> _logger;
    
    public AdminController(IConfiguration config, ILogger<AdminController> logger)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        _logger = logger;
    }
    
    public IActionResult Index()
    {
        return RedirectToAction("Courses");
    }

    public IActionResult Courses()
    {
        ViewBag.Courses = GetCourses().Result;
        ViewBag.Categories = GetCategories().Result;
        return View("~/Views/Admin/Courses.cshtml");
    }

    public IActionResult Programs()
    {
        ViewBag.Programs = GetPrograms().Result;
        return View("~/Views/Admin/Programs.cshtml");
    }
    
    public IActionResult AddCourse()
    {
        ViewBag.Categories = GetCategories().Result;
        return View("~/Views/Admin/AddCourse.cshtml");
    }
    
    public IActionResult EditCourse(int crsId)
    {
        var course = GetCourses().Result.FirstOrDefault(x => x.CrsId == crsId);
        ViewBag.Categories = GetCategories().Result;
        return View("~/Views/Admin/EditCourse.cshtml", course);
    }
    
    public IActionResult DeleteCourse(int crsId)
    {
        var dto = new DeleteDto { Id = crsId };
        return View("~/Views/Admin/DeleteCourse.cshtml", dto);
    }
    
    public IActionResult AddProgram()
    {
        return View("~/Views/Admin/AddProgram.cshtml");
    }
    
    
    public IActionResult EditProgram(int progId)
    {
        var program = GetPrograms().Result.FirstOrDefault(x => x.ProgId == progId);
        return View("~/Views/Admin/EditProgram.cshtml", program);
    }
    
    public IActionResult DeleteProgram(int progId)
    {
        var dto = new DeleteDto { Id = progId };
        return View("~/Views/Admin/DeleteProgram.cshtml", dto);
    }
    
    public async Task<List<Course>> GetCourses()
    {
        var list = new List<Course>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT *
        FROM course";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var course = new Course
            {
                CrsId = reader.GetInt32(reader.GetOrdinal("crs_id")),
                CrsCode = reader.GetString(reader.GetOrdinal("crs_code")).Trim(),
                CrsTitle = reader.GetString(reader.GetOrdinal("crs_title")).Trim(),
                CrsUnits = reader.GetInt32(reader.GetOrdinal("crs_units")),
                CrsHrsLec = reader.GetInt32(reader.GetOrdinal("crs_hrs_lec")),
                CrsHrsLab = reader.GetInt32(reader.GetOrdinal("crs_hrs_lab")),
                CatgId = reader.GetInt32(reader.GetOrdinal("catg_id")),
            };
        
            list.Add(course);
        }
    
        return list;
    }
    
    public async Task<List<CourseCategory>> GetCategories()
    {
        var list = new List<CourseCategory>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT *
        FROM course_category";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var category = new CourseCategory
            {
                CatgId = reader.GetInt32(reader.GetOrdinal("catg_id")),
                CatgName = reader.GetString(reader.GetOrdinal("catg_name")).Trim(),
                CatgCode = reader.GetString(reader.GetOrdinal("catg_code")).Trim(),
            };
        
            list.Add(category);
        }
    
        return list;
    }
    
    public async Task<List<Models.Program>> GetPrograms()
    {
        var list = new List<Models.Program>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT *
        FROM program";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var program = new Models.Program
            {
                ProgId = reader.GetInt32(reader.GetOrdinal("prog_id")),
                ProgTitle = reader.GetString(reader.GetOrdinal("prog_title"))
            };
        
            list.Add(program);
        }
    
        return list;
    }
}