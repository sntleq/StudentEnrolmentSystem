using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

[ApiController]
public class CurriculumApiController(IConfiguration config, ILogger<CurriculumApiController> logger) : ControllerBase
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    [NonAction]
    public async Task<List<Curriculum>> GetCurricula()
    {
        var list = new List<Curriculum>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM curriculum";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var curr = new Curriculum
            {
                CurId = reader.GetInt32(reader.GetOrdinal("cur_id")),
                ProgId = reader.GetInt32(reader.GetOrdinal("prog_id")),
                AyId = reader.GetInt32(reader.GetOrdinal("ay_id")),
                CurIsApproved = reader.GetBoolean(reader.GetOrdinal("cur_is_approved")),
            };
        
            list.Add(curr);
        }
    
        return list;
    }
    
    [NonAction]
    public async Task<List<CurriculumCourse>> GetCurriculumCourses()
    {
        var list = new List<CurriculumCourse>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM curriculum_course";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var course = new CurriculumCourse
            {
                CurCrsId = reader.GetInt32(reader.GetOrdinal("cur_crs_id")),
                CurId = reader.GetInt32(reader.GetOrdinal("cur_id")),
                CrsId = reader.GetInt32(reader.GetOrdinal("crs_id")),
            };
        
            list.Add(course);
        }
    
        return list;
    }
    
    [HttpPost("Curricula/Update", Name = "Curricula.Update")]
    public async Task<IActionResult> UpdateCurriculum([FromForm] CurriculumUpdateDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE program
                SET 
                    prog_id = @id,
                    prog_title = @title
                WHERE prog_id = @progId";

            cmd.Parameters.AddWithValue("id", form.ProgId);
            cmd.Parameters.AddWithValue("title", form.AyId);
            cmd.Parameters.AddWithValue("progId", form.ProgId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Program not found" });
            }
            
            return Ok(new
            {
                success = true,
                data = new { Id = form.ProgId }
            });
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }
}