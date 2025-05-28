using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class StudentController : Controller
{
    private readonly string _connectionString;
    private readonly ILogger<StudentController> _logger;
    
    public StudentController(IConfiguration config, ILogger<StudentController> logger)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        _logger = logger;
    }
    
    public IActionResult Index()
    {
        return RedirectToAction("Schedule");
    }
    
    public IActionResult Schedule()
    {
        ViewBag.StudId = HttpContext.Session.GetInt32("StudId");
        ViewBag.StudFirstName = HttpContext.Session.GetString("StudFirstName");
        
        ViewBag.ActiveEnrolments = GetActiveEnrolments(
            (int)ViewBag.StudId, 
            (int)HttpContext.Session.GetInt32("AyId")!, 
            (int)HttpContext.Session.GetInt32("SemId")!
        ).Result;
        
        return View("~/Views/Student/Schedule.cshtml");
    }
    
    public IActionResult History()
    {
        ViewBag.ActiveEnrolments = GetEnrolments(HttpContext.Session.GetInt32("StudId") ?? 0).Result;
        return View("~/Views/Student/History.cshtml");
    }
    
    public IActionResult Profile()
    {
        ViewBag.Student = GetStudent(HttpContext.Session.GetInt32("StudId") ?? 0).Result;
        ViewBag.Programs = GetPrograms().Result;
        
        if (ViewBag.Student == null)
        {
            return RedirectToAction("Student", "Auth");
        }
        return View("~/Views/Student/Profile.cshtml");
    }
    
    public async Task<Student> GetStudent(int studentId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT *
        FROM student
        WHERE stud_id = @studentId";
    
        cmd.Parameters.AddWithValue("studentId", studentId);
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        if (await reader.ReadAsync())
        {
            return new Student
            {
                StudId = reader.GetInt32(reader.GetOrdinal("stud_id")),
                StudCode = reader.GetString(reader.GetOrdinal("stud_code")),
                StudFirstName = reader.GetString(reader.GetOrdinal("stud_first_name")),
                StudLastName = reader.GetString(reader.GetOrdinal("stud_last_name")),
                StudContactNum = reader["stud_contact_num"] as string,
                StudEmail = reader.GetString(reader.GetOrdinal("stud_email")),
                StudPassword = reader.GetString(reader.GetOrdinal("stud_password")),
                StudDob = reader.GetDateTime(reader.GetOrdinal("stud_dob")),
                StudAddress = reader["stud_address"] as string,
                StudIsFirstGen = reader["stud_is_first_gen"] as bool?,
                ProgId = reader["prog_id"] as int?,
                StudStatus = reader["stud_status"] as string,
            };
        }
        
        return null!;
    }
    
    public async Task<List<Enrolment>> GetActiveEnrolments(int studentId, int ayId, int semId)
    {
        var list = new List<Enrolment>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT enrl_id, stud_id, prog_id, sched_id, ay_id, sem_id, enrl_is_completed
        FROM enrolment
        WHERE stud_id = @studentId
          AND ay_id = @ayId
          AND sem_id = @semId";
    
        cmd.Parameters.AddWithValue("studentId", studentId);
        cmd.Parameters.AddWithValue("ayId", ayId);
        cmd.Parameters.AddWithValue("semId", semId);
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var enrolment = new Enrolment
            {
                EnrlId = reader.GetInt32(reader.GetOrdinal("enrl_id")),
                StudId = reader.GetInt32(reader.GetOrdinal("stud_id")),
                ProgId = reader.GetInt32(reader.GetOrdinal("prog_id")),
                SchedId = reader.GetInt32(reader.GetOrdinal("sched_id")),
                AyId = reader.GetInt32(reader.GetOrdinal("ay_id")),
                SemId = reader.GetInt32(reader.GetOrdinal("sem_id")),
                EnrlIsCompleted = reader.GetBoolean(reader.GetOrdinal("enrl_is_completed"))
            };
        
            list.Add(enrolment);
        }
    
        return list;
    }
    
    public async Task<List<Enrolment>> GetEnrolments(int studentId)
    {
        var list = new List<Enrolment>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT enrl_id, stud_id, prog_id, sched_id, ay_id, sem_id, enrl_is_completed
        FROM enrolment
        WHERE stud_id = @studentId";
    
        cmd.Parameters.AddWithValue("studentId", studentId);
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var enrolment = new Enrolment
            {
                EnrlId = reader.GetInt32(reader.GetOrdinal("enrl_id")),
                StudId = reader.GetInt32(reader.GetOrdinal("stud_id")),
                ProgId = reader.GetInt32(reader.GetOrdinal("prog_id")),
                SchedId = reader.GetInt32(reader.GetOrdinal("sched_id")),
                AyId = reader.GetInt32(reader.GetOrdinal("ay_id")),
                SemId = reader.GetInt32(reader.GetOrdinal("sem_id")),
                EnrlIsCompleted = reader.GetBoolean(reader.GetOrdinal("enrl_is_completed"))
            };
        
            list.Add(enrolment);
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