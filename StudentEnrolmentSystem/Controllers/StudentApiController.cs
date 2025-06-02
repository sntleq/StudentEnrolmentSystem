using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class StudentApiController(IConfiguration config, ILogger<StudentApiController> logger) : ControllerBase
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    [NonAction]
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
                LvlId = reader["lvl_id"] as int?
            };
        }
        
        return null!;
    }
    
    [NonAction]
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
    
    [NonAction]
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
    
    public async Task<List<YearLevel>> GetYearLevels()
    {
        var list = new List<YearLevel>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM year_level";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var lvl = new YearLevel
            {
                LvlId = reader.GetInt32(reader.GetOrdinal("lvl_id")),
                LvlName = reader.GetString(reader.GetOrdinal("lvl_name")).Trim(),
            };
        
            list.Add(lvl);
        }
    
        return list;
    }
}