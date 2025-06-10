using Microsoft.AspNetCore.Mvc;
using Npgsql;
using StudentEnrolmentSystem.Models;
using StudentEnrolmentSystem.Models.Dto;

namespace StudentEnrolmentSystem.Controllers;

[ApiController]
public class ScheduleApiController(
    IConfiguration config, ILogger<ScheduleApiController> logger,
    CourseApiController courseApi
    ) : ControllerBase
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;

    [NonAction]
    public async Task<List<CourseSchedule>> GetSchedules()
    {
        var list = new List<CourseSchedule>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM course_schedule
            ORDER BY sched_id";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var sched = new CourseSchedule
            {
                SchedId = reader.GetInt32(reader.GetOrdinal("sched_id")),
                SchedCode = reader.GetString(reader.GetOrdinal("sched_code")),
                CrsId = reader.GetInt32(reader.GetOrdinal("crs_id")),
                TchrId = reader.GetInt32(reader.GetOrdinal("tchr_id")),
                RoomId = reader["room_id"] as int?,
                SchedCapacity = reader.GetInt32(reader.GetOrdinal("sched_capacity")),
                SchedDescription = reader.GetString(reader.GetOrdinal("sched_description")),
                SchedIsActive = reader.GetBoolean(reader.GetOrdinal("sched_is_active")),
            };
        
            list.Add(sched);
        }
    
        return list;
    }
    
    [NonAction]
    public async Task<List<ScheduleSession>> GetSessions()
    {
        var list = new List<ScheduleSession>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT *
            FROM schedule_session
            ORDER BY sess_id";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var session = new ScheduleSession
            {
                SessId = reader.GetInt32(reader.GetOrdinal("sess_id")),
                StartSlotId = reader.GetInt32(reader.GetOrdinal("start_slot_id")),
                EndSlotId = reader.GetInt32(reader.GetOrdinal("end_slot_id")),
                SchedId = reader.GetInt32(reader.GetOrdinal("sched_id")),
            };
        
            list.Add(session);
        }
    
        return list;
    }
    
    [NonAction]
    public async Task<List<TimeSlot>> GetTimeSlots()
    {
        var list = new List<TimeSlot>();
    
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
    
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT
                slot_id,
                slot_day::INT,
                slot_time_start::TIME,
                slot_time_end::TIME
            FROM timeslot";
    
        await using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            var slot = new TimeSlot
            {
                SlotId = reader.GetInt32(reader.GetOrdinal("slot_id")),
                SlotDay = reader.GetInt32(reader.GetOrdinal("slot_day")),
                SlotTimeStart = reader.GetTimeSpan(reader.GetOrdinal("slot_time_start")),
                SlotTimeEnd = reader.GetTimeSpan(reader.GetOrdinal("slot_time_end")),
            };
        
            list.Add(slot);
        }
    
        return list;
    }
    
    [HttpPost("Schedules/Add", Name = "Schedules.Add")]
    public async Task<IActionResult> AddSchedule([FromForm] ScheduleCreateDto form)
    {
        var course = courseApi.GetCourses().Result.FirstOrDefault(c => c.CrsId == form.CrsId);
        var hours = course!.CrsHrsLec + course.CrsHrsLab;
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (form.SlotIds.Count != hours)
            return BadRequest(new { success = false, message = "Total slots doesn't match course hours." });
        
        var sessions = SlotIdsToSessions(form.SlotIds, form.SchedId);
        if (sessions == null)
            return BadRequest(new { success = false, message = "A section must not have more than one session in the same day." });

        var description = GetScheduleDescription(sessions);
        
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var tx = await conn.BeginTransactionAsync();
            
            await using var insertCmd = conn.CreateCommand();
            insertCmd.Transaction = tx;
            insertCmd.CommandText = @"
                INSERT INTO course_schedule (
                    crs_id, sched_code, room_id,
                    sched_capacity, tchr_id, sched_description
                ) VALUES (
                    @crsId, @schedCode, @roomId,
                    @schedCapacity, @tchrId, @schedDescription
                ) RETURNING sched_id";

            insertCmd.Parameters.AddWithValue("crsId", form.CrsId);
            insertCmd.Parameters.AddWithValue("schedCode", form.SchedCode);
            if (form.RoomId.HasValue)
                insertCmd.Parameters.AddWithValue("roomId", form.RoomId);
            else
                insertCmd.Parameters.AddWithValue("roomId", DBNull.Value);
            insertCmd.Parameters.AddWithValue("schedCapacity", form.SchedCapacity);
            insertCmd.Parameters.AddWithValue("tchrId", form.TchrId);
            insertCmd.Parameters.AddWithValue("schedDescription", description);
            
            var newId = (int)(await insertCmd.ExecuteScalarAsync())!;
            
            sessions = SlotIdsToSessions(form.SlotIds, newId);
            if (sessions!.Count > 0)
            {
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO schedule_session (
                        start_slot_id, end_slot_id, sched_id
                    ) VALUES (
                        @startSlotId, @endSlotId, @schedId
                    )";

                cmd.Parameters.AddWithValue("schedId", newId);
                cmd.Parameters.Add("startSlotId", NpgsqlTypes.NpgsqlDbType.Integer);
                cmd.Parameters.Add("endSlotId", NpgsqlTypes.NpgsqlDbType.Integer);

                foreach (var session in sessions)
                {
                    cmd.Parameters["startSlotId"].Value = session.StartSlotId;
                    cmd.Parameters["endSlotId"].Value = session.EndSlotId;
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
    
    [HttpPost("Schedules/Update", Name = "Schedules.Update")]
    public async Task<IActionResult> UpdateSchedule([FromForm] ScheduleCreateDto form)
    {
        var course = courseApi.GetCourses().Result.FirstOrDefault(c => c.CrsId == form.CrsId);
        var hours = course!.CrsHrsLec + course.CrsHrsLab;
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (form.SlotIds.Count != hours)
            return BadRequest(new { success = false, message = "Total slots doesn't match course hours." });
        
        var sessions = SlotIdsToSessions(form.SlotIds, form.SchedId);
        if (sessions == null)
            return BadRequest(new { success = false, message = "A section must not have more than one session in the same day." });

        var description = GetScheduleDescription(sessions);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var tx = await conn.BeginTransactionAsync();
            
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                UPDATE course_schedule
                SET 
                    sched_code = @schedCode,
                    tchr_id = @tchrId,
                    room_id = @roomId,
                    sched_capacity = @schedCapacity,
                    sched_description = @schedDescription
                WHERE sched_id = @schedId";

            cmd.Parameters.AddWithValue("schedCode", form.SchedCode);
            cmd.Parameters.AddWithValue("tchrId", form.TchrId);
            if (form.RoomId.HasValue)
                cmd.Parameters.AddWithValue("roomId", form.RoomId);
            else
                cmd.Parameters.AddWithValue("roomId", DBNull.Value);
            cmd.Parameters.AddWithValue("schedCapacity", form.SchedCapacity);
            cmd.Parameters.AddWithValue("schedDescription", description);
            cmd.Parameters.AddWithValue("schedId", form.SchedId!);
            
            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
            {
                return NotFound(new { success = false, message = "Schedule not found" });
            }
            
            await using var cleanCmd = conn.CreateCommand();
            cleanCmd.Transaction = tx;
            cleanCmd.CommandText = @"
                    DELETE FROM schedule_session
                    WHERE sched_id = @schedId";

            cleanCmd.Parameters.AddWithValue("schedId", form.SchedId!);
            await cleanCmd.ExecuteNonQueryAsync();
            
            if (sessions.Count > 0)
            {
                await using var newCmd = conn.CreateCommand();
                newCmd.Transaction = tx;
                newCmd.CommandText = @"
                    INSERT INTO schedule_session (
                        start_slot_id, end_slot_id, sched_id
                    ) VALUES (
                        @startSlotId, @endSlotId, @schedId
                    )";

                newCmd.Parameters.AddWithValue("schedId", form.SchedId);
                newCmd.Parameters.Add("startSlotId", NpgsqlTypes.NpgsqlDbType.Integer);
                newCmd.Parameters.Add("endSlotId", NpgsqlTypes.NpgsqlDbType.Integer);

                foreach (var session in sessions)
                {
                    newCmd.Parameters["startSlotId"].Value = session.StartSlotId;
                    newCmd.Parameters["endSlotId"].Value = session.EndSlotId;
                    await newCmd.ExecuteNonQueryAsync();
                }
            }
            
            await tx.CommitAsync();
            
            return Ok(new
            {
                success = true,
                data = new { Id = form.SchedId }
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
    
    [HttpPost("Schedules/Delete", Name = "Schedules.Delete")]
    public async Task<IActionResult> DeleteSchedule([FromForm] IdDto form)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            await using var tx = await conn.BeginTransactionAsync();
            
            await using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.Transaction = tx;
                checkCmd.CommandText = @"
                    SELECT 1 FROM enrolment
                    WHERE sched_id = @schedId";
                
                checkCmd.Parameters.AddWithValue("schedId", form.Id);

                var exists = await checkCmd.ExecuteScalarAsync();
                
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                
                if (exists is not null)
                {
                    cmd.CommandText = @"
                        UPDATE course_schedule
                        SET 
                            sched_is_active = false
                        WHERE sched_id = @schedId";
                }
                else
                {
                    cmd.CommandText = @"
                        DELETE FROM course_schedule
                        WHERE sched_id = @schedId";
                }
                cmd.Parameters.AddWithValue("schedId", form.Id);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    return NotFound(new { success = false, message = "Schedule not found" });
                }
            }
            
            await tx.CommitAsync();
            
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
    
    [NonAction]
    private List<ScheduleSession>? SlotIdsToSessions(List<int> slotIds, int? schedId)
    {
        var slots = GetTimeSlots().Result
            .Where(ts => slotIds.Contains(ts.SlotId))
            .OrderBy(ts => ts.SlotDay)
            .ThenBy(ts => ts.SlotId)
            .ToList();

        var sessions = new List<ScheduleSession>();
        var currentGroup = new List<TimeSlot>();
        
        var slotIdToDay = slots.ToDictionary(ts => ts.SlotId, ts => ts.SlotDay);

        foreach (var slot in slots)
        {
            if (currentGroup.Count > 0 &&
                slot.SlotDay == currentGroup[0].SlotDay &&
                slot.SlotId == currentGroup.Last().SlotId + 1)
            {
                currentGroup.Add(slot);
            }
            else
            {
                if (currentGroup.Count > 0)
                {
                    var day = currentGroup[0].SlotDay;
                    if (sessions.Count != 0)
                    {
                        var lastSession = sessions.Last();
                        var lastDay = slotIdToDay[lastSession.StartSlotId];
                        if (lastDay == day)
                            return null;
                    }
                    
                    sessions.Add(new ScheduleSession
                    {
                        StartSlotId = currentGroup.First().SlotId,
                        EndSlotId = currentGroup.Last().SlotId,
                        SchedId = schedId ?? 0,
                    });
                }
                currentGroup = new List<TimeSlot> { slot };
            }
        }

        if (currentGroup.Count > 0)
        {
            var day = currentGroup[0].SlotDay;
            if (sessions.Count != 0)
            {
                var lastSession = sessions.Last();
                var lastDay = slotIdToDay[lastSession.StartSlotId];
                if (lastDay == day)
                    return null;
            }
            
            sessions.Add(new ScheduleSession
            {
                StartSlotId = currentGroup.First().SlotId,
                EndSlotId = currentGroup.Last().SlotId,
                SchedId = schedId ?? 0,
            });
        }
        
        return sessions;
    }
    
    [NonAction]
    public List<int> SessionsToSlotIds(List<ScheduleSession> sessions)
    {
        var slotIds = new List<int>();

        foreach (var sess in sessions)
        {
            for (var id = sess.StartSlotId; id <= sess.EndSlotId; id++)
            {
                slotIds.Add(id);
            }
        }
        
        return slotIds.OrderBy(x => x).ToList();
    }
    
    [NonAction]
    private string GetScheduleDescription(List<ScheduleSession> sessions)
    {
        var slotLookup = GetTimeSlots().Result.ToDictionary(ts => ts.SlotId);
        var abbrevDay = new Dictionary<int,string> {
            [1] = "M", [2] = "T", [3] = "W", [4] = "Th", [5] = "F", [6] = "S"
        };
        var shortDay = new Dictionary<int,string> {
            [1] = "Mon", [2] = "Tues", [3] = "Wed", [4] = "Thurs", [5] = "Fri", [6] = "Sat"
        };

        // group sessions by start/end
        var summaryParts = sessions
            .GroupBy(sess => (
                StartIndex : sess.StartSlotId % 11,
                EndIndex   : sess.EndSlotId   % 11
            ))
            .Select(group =>
            {
                var example = group.First();
                var startTs  = slotLookup[example.StartSlotId].SlotTimeStart;
                var endTs    = slotLookup[example.EndSlotId]  .SlotTimeEnd;

                var fmtStart = (new DateTime(1,1,1) + startTs).ToString("h ").Trim();
                var fmtEnd = (new DateTime(1,1,1) + endTs).ToString("h tt");

                var days = group
                    .Select(sess => slotLookup[sess.StartSlotId].SlotDay)
                    .Distinct()
                    .ToList();

                var dayString = days.Count > 1
                    ? string.Concat(days.Select(d => abbrevDay[d]))
                    : shortDay[days[0]];

                return $"{dayString} {fmtStart}-{fmtEnd}";
            });

        return string.Join(", ", summaryParts);
    }
}