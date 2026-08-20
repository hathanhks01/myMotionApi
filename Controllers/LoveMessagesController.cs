using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myMotionApi.Data;
using myMotionApi.DTOs;
using myMotionApi.Models;

namespace myMotionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoveMessagesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly Guid _senderId;
        private readonly Guid _receiverId;

        public LoveMessagesController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _senderId = Guid.Parse(config["AppSettings:SenderId"]
                ?? "11111111-1111-1111-1111-111111111111");
            _receiverId = Guid.Parse(config["AppSettings:ReceiverId"]
                ?? "22222222-2222-2222-2222-222222222222");
        }

        // ── GET /api/lovemessages ─────────────────────────────────────────────
        /// <summary>Lấy tất cả lời nhắn, hỗ trợ lọc theo ngày</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoveMessageResponse>>> GetAll(
            [FromQuery] DateOnly? date,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _db.LoveMessages
                .Include(m => m.Attachments)
                .AsQueryable();

            // Lọc theo ngày (theo giờ UTC)
            if (date.HasValue)
            {
                var start = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                var end = start.AddDays(1);
                query = query.Where(m => m.SentAt >= start && m.SentAt < end);
            }

            var messages = await query
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(messages.Select(ToResponse));
        }

        // ── GET /api/lovemessages/{id} ────────────────────────────────────────
        /// <summary>Lấy chi tiết 1 lời nhắn kèm attachment</summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<LoveMessageResponse>> GetById(Guid id)
        {
            var message = await _db.LoveMessages
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message is null) return NotFound(new { message = "Không tìm thấy lời nhắn." });

            return Ok(ToResponse(message));
        }

        // ── POST /api/lovemessages ────────────────────────────────────────────
        /// <summary>Gửi lời nhắn tỏ tình mới</summary>
        [HttpPost]
        public async Task<ActionResult<LoveMessageResponse>> Create(
            [FromBody] CreateLoveMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Nội dung không được để trống." });

            var message = new LoveMessage
            {
                SenderId = _senderId,
                ReceiverId = _receiverId,
                Content = request.Content.Trim(),
                SentAt = DateTime.UtcNow
            };

            _db.LoveMessages.Add(message);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = message.Id },
                ToResponse(message));
        }

        // ── PUT /api/lovemessages/{id} ────────────────────────────────────────
        /// <summary>Cập nhật lời nhắn (sửa nội dung hoặc đánh dấu đã đọc)</summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<LoveMessageResponse>> Update(
            Guid id,
            [FromBody] UpdateLoveMessageRequest request)
        {
            var message = await _db.LoveMessages
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message is null) return NotFound(new { message = "Không tìm thấy lời nhắn." });

            if (request.Content is not null)
                message.Content = request.Content.Trim();

            if (request.IsRead.HasValue && request.IsRead.Value && !message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return Ok(ToResponse(message));
        }

        // ── PATCH /api/lovemessages/{id}/read ────────────────────────────────
        /// <summary>Đánh dấu lời nhắn đã được đọc (shortcut)</summary>
        [HttpPatch("{id:guid}/read")]
        public async Task<ActionResult<LoveMessageResponse>> MarkAsRead(Guid id)
        {
            var message = await _db.LoveMessages
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (message is null) return NotFound(new { message = "Không tìm thấy lời nhắn." });

            if (!message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return Ok(ToResponse(message));
        }

        // ── DELETE /api/lovemessages/{id} ─────────────────────────────────────
        /// <summary>Xóa lời nhắn (kèm tất cả attachment)</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var message = await _db.LoveMessages.FindAsync(id);
            if (message is null) return NotFound(new { message = "Không tìm thấy lời nhắn." });

            _db.LoveMessages.Remove(message);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // ── Helper ───────────────────────────────────────────────────────────
        private static LoveMessageResponse ToResponse(LoveMessage m) => new()
        {
            Id = m.Id,
            SenderId = m.SenderId,
            ReceiverId = m.ReceiverId,
            Content = m.Content,
            SentAt = m.SentAt,
            IsRead = m.IsRead,
            ReadAt = m.ReadAt,
            Attachments = m.Attachments.Select(a => new AttachmentResponse
            {
                Id = a.Id,
                FileUrl = a.FileUrl,
                FileType = a.FileType,
                OriginalFileName = a.OriginalFileName,
                FileSizeBytes = a.FileSizeBytes,
                UploadedAt = a.UploadedAt
            }).ToList()
        };
    }
}
