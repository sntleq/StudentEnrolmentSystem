using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

public class AdminApiController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ILogger<AdminApiController> _logger;
    
    public AdminApiController(IConfiguration config, ILogger<AdminApiController> logger)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        _logger = logger;
    }
    
    [HttpPost("Courses/Add", Name = "Courses.Add")]
    public async Task<IActionResult> AddCourse([FromForm] Course form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO course (
                    crs_code, crs_title, crs_units,
                    crs_hrs_lec, crs_hrs_lab, catg_id
                ) VALUES (
                    @code, @title, @units,
                    @hrsLec, @hrsLab, @catgId
                ) RETURNING crs_id";

            insertCmd.Parameters.AddWithValue("code", form.CrsCode.Trim());
            insertCmd.Parameters.AddWithValue("title", form.CrsTitle.Trim());
            insertCmd.Parameters.AddWithValue("units", form.CrsUnits);
            insertCmd.Parameters.AddWithValue("hrsLec", form.CrsHrsLec);
            insertCmd.Parameters.AddWithValue("hrsLab", form.CrsHrsLab);
            insertCmd.Parameters.AddWithValue("catgId", form.CatgId);

            var newId = (int)(await insertCmd.ExecuteScalarAsync())!;

            return Ok(new
            {
                success = true,
                data = new { Id = newId }
            });
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-up.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }
    
    [HttpPost("Courses/Update", Name = "Courses.Update")]
    public async Task<IActionResult> UpdateCourse([FromForm] Course form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE course
                SET 
                    crs_code = @code,
                    crs_title = @title,
                    crs_units = @units,
                    crs_hrs_lec = @hrsLec,
                    crs_hrs_lab = @hrsLab,
                    catg_id = @catgId
                WHERE crs_id = @crsId";

            cmd.Parameters.AddWithValue("code", form.CrsCode.Trim());
            cmd.Parameters.AddWithValue("title", form.CrsTitle.Trim());
            cmd.Parameters.AddWithValue("units", form.CrsUnits);
            cmd.Parameters.AddWithValue("hrsLec", form.CrsHrsLec);
            cmd.Parameters.AddWithValue("hrsLab", form.CrsHrsLab);
            cmd.Parameters.AddWithValue("catgId", form.CatgId);
            cmd.Parameters.AddWithValue("crsId", form.CrsId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Course not found" });
            }
            
            return Ok(new
            {
                success = true,
                data = new { Id = form.CrsId }
            });
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-up.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
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
                    prog_title
                ) VALUES (
                    @title
                ) RETURNING prog_id";

            insertCmd.Parameters.AddWithValue("title", form.ProgTitle.Trim());

            var newId = (int)(await insertCmd.ExecuteScalarAsync())!;

            return Ok(new
            {
                success = true,
                data = new { Id = newId }
            });
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-up.");
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
            _logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-up.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }
    
    [HttpPost("Courses/Delete", Name = "Courses.Delete")]
    public async Task<IActionResult> DeleteCourse([FromForm] DeleteDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE FROM course
                WHERE crs_id = @crsId";

            cmd.Parameters.AddWithValue("crsId", form.Id);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Course not found" });
            }
            
            return Ok(new
            {
                success = true,
                data = new { Id = form.Id }
            });
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-up.");
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
            _logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign-up.");
            return StatusCode(500, new { success = false, message = "Unexpected error", error = ex.Message });
        }
    }
}