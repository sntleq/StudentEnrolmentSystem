using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

[ApiController]
public class CurriculumApiController(
    IConfiguration config, ILogger<CurriculumApiController> logger,
    CourseApiController courseApi
    ) : ControllerBase
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
            FROM curriculum cur
            LEFT JOIN academic_year ay
            ON cur.ay_id = ay.ay_id
            ORDER BY ay.ay_start_yr DESC, cur.prog_id";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var cur = new Curriculum
            {
                CurId = reader.GetInt32(reader.GetOrdinal("cur_id")),
                ProgId = reader.GetInt32(reader.GetOrdinal("prog_id")),
                AyId = reader.GetInt32(reader.GetOrdinal("ay_id")),
                CurGeeUnits = reader["cur_gee_units"] as int?,
                CurPelecUnits = reader["cur_pelec_units"] as int?,
                CurStatus = reader.GetString(reader.GetOrdinal("cur_status")),
                CurRejectReason = reader["cur_reject_reason"] as string
            };
        
            list.Add(cur);
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
        var crsIds = GetCurriculumCourses().Result.Where(c => c.CurId == form.CurId).Select(c => c.CrsId).ToList();
        var maxGeeUnits = courseApi.GetCourses().Result.Where(c => c.CatgId == 2 && crsIds.Contains(c.CrsId)).Sum(c => c.CrsUnits);
        var maxPelecUnits = courseApi.GetCourses().Result.Where(c => c.CatgId == 6 && crsIds.Contains(c.CrsId)).Sum(c => c.CrsUnits);
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (form.CurGeeUnits > maxGeeUnits)
            return BadRequest(new { success = false, message = $"GEE requirement must be at most {maxGeeUnits} units." });
        
        if (form.CurPelecUnits > maxPelecUnits)
            return BadRequest(new { success = false, message = $"PELEC requirement must be at most {maxPelecUnits} units." });
        
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var tx = await conn.BeginTransactionAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                UPDATE curriculum
                SET 
                    cur_gee_units = @curGeeUnits,
                    cur_pelec_units = @curPelecUnits,
                    cur_status = 'Pending'
                WHERE cur_id = @curId";

            cmd.Parameters.AddWithValue("curGeeUnits", form.CurGeeUnits!);
            cmd.Parameters.AddWithValue("curPelecUnits", form.CurPelecUnits!);
            cmd.Parameters.AddWithValue("curId", form.CurId);
            
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Curriculum not found" });
            }
            
            await using var cleanCmd = conn.CreateCommand();
            cleanCmd.Transaction = tx;
            cleanCmd.CommandText = @"
                    DELETE FROM curriculum_course
                    WHERE cur_id = @curId";

            cleanCmd.Parameters.AddWithValue("curId", form.CurId);
            await cleanCmd.ExecuteNonQueryAsync();
            
            if (form.CrsIds.Count > 0)
            {
                await using var crsCmd = conn.CreateCommand();
                crsCmd.Transaction = tx;
                crsCmd.CommandText = @"
                    INSERT INTO curriculum_course (
                        cur_id, crs_id
                    ) VALUES (
                        @curId, @crsId
                    )";

                crsCmd.Parameters.AddWithValue("curId", form.CurId);
                crsCmd.Parameters.Add("crsId", NpgsqlTypes.NpgsqlDbType.Integer);
                foreach (var crsId in form.CrsIds)
                {
                    crsCmd.Parameters["crsId"].Value = crsId;
                    await crsCmd.ExecuteNonQueryAsync();
                }
            }
            
            await tx.CommitAsync();
            
            return Ok(new
            {
                success = true,
                data = new { Id = form.CurId }
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
    
    [HttpPost("Curricula/Approve", Name = "Curricula.Approve")]
    public async Task<IActionResult> ApproveCurriculum([FromForm] IdDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE curriculum
                SET cur_status = 'Approved'
                WHERE cur_id = @curId";

            cmd.Parameters.AddWithValue("curId", form.Id);
            
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Curriculum not found" });
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
    
    [HttpPost("Curricula/Reject", Name = "Curricula.Reject")]
    public async Task<IActionResult> RejectCurriculum([FromForm] RejectDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE curriculum
                SET
                    cur_status = 'Rejected',
                    cur_reject_reason = @reason
                WHERE cur_id = @curId";

            cmd.Parameters.AddWithValue("curId", form.Id);
            cmd.Parameters.AddWithValue("reason", form.Reason!);
            
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Curriculum not found" });
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