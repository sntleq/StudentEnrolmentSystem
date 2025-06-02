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
            FROM course
            ORDER BY catg_id, crs_id";
    
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
                LvlId = reader["lvl_id"] as int?
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

    [NonAction]
    public async Task<List<int>> GetPrerequisites(int crsId)
    {
        var list = new List<int>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT crs_preq_id
            FROM course_dependency
            WHERE crs_id = @crsId
            ORDER BY crs_preq_id";
    
        cmd.Parameters.AddWithValue("crsId", crsId);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            list.Add(reader.GetInt32(reader.GetOrdinal("crs_preq_id")));
        }
    
        return list;
    }

    
    [HttpPost("Courses/Add", Name = "Courses.Add")]
    public async Task<IActionResult> AddCourse([FromForm] CourseCreateDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var tx = await conn.BeginTransactionAsync();
            
            await using var insertCmd = conn.CreateCommand();
            insertCmd.Transaction = tx;
            insertCmd.CommandText = @"
                INSERT INTO course (
                    crs_code, crs_title, crs_units,
                    crs_hrs_lec, crs_hrs_lab, catg_id, lvl_id
                ) VALUES (
                    @code, @title, @units,
                    @hrsLec, @hrsLab, @catgId, @lvlId
                ) RETURNING crs_id";

            insertCmd.Parameters.AddWithValue("code", form.CrsCode.Trim());
            insertCmd.Parameters.AddWithValue("title", form.CrsTitle.Trim());
            insertCmd.Parameters.AddWithValue("units", form.CrsUnits);
            insertCmd.Parameters.AddWithValue("hrsLec", form.CrsHrsLec);
            insertCmd.Parameters.AddWithValue("hrsLab", form.CrsHrsLab);
            insertCmd.Parameters.AddWithValue("catgId", form.CatgId);
            
            if (form.LvlId.HasValue)
                insertCmd.Parameters.AddWithValue("lvlId", form.LvlId);
            else
                insertCmd.Parameters.AddWithValue("lvlId", DBNull.Value);

            var newId = (int)(await insertCmd.ExecuteScalarAsync())!;

            if (form.CrsPreqIds.Count > 0)
            {
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO course_dependency (
                        crs_id, crs_preq_id
                    ) VALUES (
                        @crsId, @crsPreqId
                    )";

                cmd.Parameters.AddWithValue("crsId", newId);
                cmd.Parameters.Add("crsPreqId", NpgsqlTypes.NpgsqlDbType.Integer);
                foreach (var preqId in form.CrsPreqIds)
                {
                    cmd.Parameters["crsPreqId"].Value = preqId;
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            
            await tx.CommitAsync();
            
            return Ok(new
            {
                success = true,
                data = new { Id = newId }
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
    
    [HttpPost("Courses/Update", Name = "Courses.Update")]
    public async Task<IActionResult> UpdateCourse([FromForm] CourseCreateDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var tx = await conn.BeginTransactionAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                UPDATE course
                SET 
                    crs_code = @code,
                    crs_title = @title,
                    crs_units = @units,
                    crs_hrs_lec = @hrsLec,
                    crs_hrs_lab = @hrsLab,
                    catg_id = @catgId,
                    lvl_id = @lvlId
                WHERE crs_id = @crsId";

            cmd.Parameters.AddWithValue("code", form.CrsCode.Trim());
            cmd.Parameters.AddWithValue("title", form.CrsTitle.Trim());
            cmd.Parameters.AddWithValue("units", form.CrsUnits);
            cmd.Parameters.AddWithValue("hrsLec", form.CrsHrsLec);
            cmd.Parameters.AddWithValue("hrsLab", form.CrsHrsLab);
            cmd.Parameters.AddWithValue("catgId", form.CatgId);
            if (form.LvlId.HasValue)
                cmd.Parameters.AddWithValue("lvlId", form.LvlId);
            else
                cmd.Parameters.AddWithValue("lvlId", DBNull.Value);
            cmd.Parameters.AddWithValue("crsId", form.CrsId);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Course not found" });
            }
            
            await using var cleanCmd = conn.CreateCommand();
            cleanCmd.Transaction = tx;
            cleanCmd.CommandText = @"
                    DELETE FROM course_dependency
                    WHERE crs_id = @crsId";

            cleanCmd.Parameters.AddWithValue("crsId", form.CrsId);
            await cleanCmd.ExecuteNonQueryAsync();
            
            if (form.CrsPreqIds.Count > 0)
            {
                await using var preqCmd = conn.CreateCommand();
                preqCmd.Transaction = tx;
                preqCmd.CommandText = @"
                    INSERT INTO course_dependency (
                        crs_id, crs_preq_id
                    ) VALUES (
                        @crsId, @crsPreqId
                    )";

                preqCmd.Parameters.AddWithValue("crsId", form.CrsId);
                preqCmd.Parameters.Add("crsPreqId", NpgsqlTypes.NpgsqlDbType.Integer);
                foreach (var preqId in form.CrsPreqIds)
                {
                    preqCmd.Parameters["crsPreqId"].Value = preqId;
                    await preqCmd.ExecuteNonQueryAsync();
                }
            }
            
            await tx.CommitAsync();
            
            return Ok(new
            {
                success = true,
                data = new { Id = form.CrsId }
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
                data = new { form.Id }
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