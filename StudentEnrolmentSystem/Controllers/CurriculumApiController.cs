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
                ProgTitle = reader.GetString(reader.GetOrdinal("prog_title")),
                ProgCode = reader.GetString(reader.GetOrdinal("prog_code")),
            };
        
            list.Add(program);
        }
    
        return list;
    }

    [HttpPost("Programs/Add", Name = "Programs.Add")]
    public async Task<IActionResult> AddProgram([FromForm] Models.Program form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO program (
                    prog_title, prog_code
                ) VALUES (
                    @title, @code
                ) RETURNING prog_id";

            insertCmd.Parameters.AddWithValue("title", form.ProgTitle.Trim());
            insertCmd.Parameters.AddWithValue("code", form.ProgCode.Trim());

            var newId = (int)(await insertCmd.ExecuteScalarAsync())!;

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO curriculum (
                    prog_id, ay_id
                ) VALUES (
                    @progId, @ayId
                ) RETURNING cur_id";

            cmd.Parameters.AddWithValue("progId", newId);
            cmd.Parameters.AddWithValue("ayId", HttpContext.Session.GetInt32("AyId") ?? 2);

            await cmd.ExecuteScalarAsync();
            
            return Ok(new
            {
                success = true,
                data = new { Id = newId }
            });
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during sign-up.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }
    
    [HttpPost("Programs/Update", Name = "Programs.Update")]
    public async Task<IActionResult> UpdateProgram([FromForm] Models.Program form)
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
            cmd.Parameters.AddWithValue("title", form.ProgTitle.Trim());
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
            logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during sign-up.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }
    
    [HttpPost("Programs/Delete", Name = "Programs.Delete")]
    public async Task<IActionResult> DeleteProgram([FromForm] DeleteDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM program
                WHERE prog_id = @progId";

            cmd.Parameters.AddWithValue("progId", form.Id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Program not found" });
            }
            
            return Ok(new
            {
                success = true,
                data = new { Id = form.Id }
            });
        }
        catch (NpgsqlException ex)
        {
            logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during sign-up.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }
}