using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class TimeMachineController(IConfiguration config, ILogger<TimeMachineController> logger) : Controller
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    public IActionResult Index()
    {
        ViewBag.AcademicYears = GetAcademicYears().Result;
        ViewBag.Semesters = GetSemesters().Result;
        return View("~/Views/TimeMachine.cshtml");
    }

    [HttpPost("TimeMachine", Name = "Time.Set")]
    public IActionResult TimeSet([FromForm] TimeMachineDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        HttpContext.Session.SetInt32("AyId", form.AyId);
        HttpContext.Session.SetInt32("SemId", form.SemId);
        
        return Ok(new
        {
            success = true,
            data = new { }
        });
    }
    
    public async Task<List<AcademicYear>> GetAcademicYears()
    {
        var list = new List<AcademicYear>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM academic_year";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var year = new AcademicYear
            {
                AyId = reader.GetInt32(reader.GetOrdinal("ay_id")),
                AyStartYr = reader.GetInt32(reader.GetOrdinal("ay_start_yr")),
                AyEndYr = reader.GetInt32(reader.GetOrdinal("ay_end_yr"))
            };
        
            list.Add(year);
        }
    
        return list;
    }

    public async Task<List<Semester>> GetSemesters()
    {
        var list = new List<Semester>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM semester";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var sem = new Semester
            {
                SemId = reader.GetInt32(reader.GetOrdinal("sem_id")),
                SemName = reader.GetString(reader.GetOrdinal("sem_name")),
            };
        
            list.Add(sem);
        }
    
        return list;
    }
}