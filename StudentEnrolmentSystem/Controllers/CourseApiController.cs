using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

[ApiController]
public class CourseApiController(IConfiguration config, ILogger<CourseApiController> logger) : ControllerBase
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    [NonAction]
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
    
    [NonAction]
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
            logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during sign-up.");
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
            logger.LogError(ex, "Database error during sign-up.");
            return StatusCode(500, new { success = false, message = "Database error", error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during sign-up.");
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