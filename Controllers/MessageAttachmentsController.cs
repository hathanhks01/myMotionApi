using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myMotionApi.Data;
using myMotionApi.DTOs;
using myMotionApi.Models;

namespace myMotionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageAttachmentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MessageAttachmentsController(AppDbContext db)
        {
            _db = db;
        }

        // ── GET /api/messageattachments?messageId={id} ────────────────────────
        /// <summary>Lấy tất cả attachment của 1 lời nhắn</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttachmentResponse>>> GetByMessage(
            [FromQuery] Guid messageId)
        {
            var exists = await _db.LoveMessages.AnyAsync(m => m.Id == messageId);
            if (!exists) return NotFound(new { message = "Không tìm thấy lời nhắn." });

            var attachments = await _db.MessageAttachments
                .Where(a => a.MessageId == messageId)
                .OrderBy(a => a.UploadedAt)
                .ToListAsync();

            return Ok(attachments.Select(ToResponse));
        }

        // ── GET /api/messageattachments/{id} ──────────────────────────────────
        /// <summary>Lấy chi tiết 1 attachment</summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AttachmentResponse>> GetById(Guid id)
        {
            var attachment = await _db.MessageAttachments.FindAsync(id);
            if (attachment is null) return NotFound(new { message = "Không tìm thấy attachment." });

            return Ok(ToResponse(attachment));
        }

        // ── POST /api/messageattachments ──────────────────────────────────────
        /// <summary>Thêm attachment vào lời nhắn</summary>
        [HttpPost]
        public async Task<ActionResult<AttachmentResponse>> Create(
            [FromBody] CreateAttachmentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FileUrl))
                return BadRequest(new { message = "FileUrl không được để trống." });

            var allowedTypes = new[] { "image", "video" };
            if (!allowedTypes.Contains(request.FileType.ToLower()))
                return BadRequest(new { message = "FileType chỉ được là 'image' hoặc 'video'." });

            var messageExists = await _db.LoveMessages.AnyAsync(m => m.Id == request.MessageId);
            if (!messageExists) return NotFound(new { message = "Không tìm thấy lời nhắn." });

            var attachment = new MessageAttachment
            {
                MessageId = request.MessageId,
                FileUrl = request.FileUrl.Trim(),
                FileType = request.FileType.ToLower(),
                OriginalFileName = request.OriginalFileName,
                FileSizeBytes = request.FileSizeBytes,
                UploadedAt = DateTime.UtcNow
            };

            _db.MessageAttachments.Add(attachment);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = attachment.Id },
                ToResponse(attachment));
        }

        // ── PUT /api/messageattachments/{id} ──────────────────────────────────
        /// <summary>Cập nhật thông tin attachment</summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<AttachmentResponse>> Update(
            Guid id,
            [FromBody] UpdateAttachmentRequest request)
        {
            var attachment = await _db.MessageAttachments.FindAsync(id);
            if (attachment is null) return NotFound(new { message = "Không tìm thấy attachment." });

            if (request.FileUrl is not null)
                attachment.FileUrl = request.FileUrl.Trim();

            if (request.FileType is not null)
            {
                var allowedTypes = new[] { "image", "video" };
                if (!allowedTypes.Contains(request.FileType.ToLower()))
                    return BadRequest(new { message = "FileType chỉ được là 'image' hoặc 'video'." });
                attachment.FileType = request.FileType.ToLower();
            }

            if (request.OriginalFileName is not null)
                attachment.OriginalFileName = request.OriginalFileName;

            await _db.SaveChangesAsync();
            return Ok(ToResponse(attachment));
        }

        // ── DELETE /api/messageattachments/{id} ───────────────────────────────
        /// <summary>Xóa attachment</summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var attachment = await _db.MessageAttachments.FindAsync(id);
            if (attachment is null) return NotFound(new { message = "Không tìm thấy attachment." });

            _db.MessageAttachments.Remove(attachment);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // ── DELETE /api/messageattachments/message/{messageId} ────────────────
        /// <summary>Xóa toàn bộ attachment của 1 lời nhắn</summary>
        [HttpDelete("message/{messageId:guid}")]
        public async Task<IActionResult> DeleteAllByMessage(Guid messageId)
        {
            var attachments = await _db.MessageAttachments
                .Where(a => a.MessageId == messageId)
                .ToListAsync();

            if (!attachments.Any())
                return NotFound(new { message = "Không có attachment nào cho lời nhắn này." });

            _db.MessageAttachments.RemoveRange(attachments);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // ── Helper ───────────────────────────────────────────────────────────
        private static AttachmentResponse ToResponse(MessageAttachment a) => new()
        {
            Id = a.Id,
            FileUrl = a.FileUrl,
            FileType = a.FileType,
            OriginalFileName = a.OriginalFileName,
            FileSizeBytes = a.FileSizeBytes,
            UploadedAt = a.UploadedAt
        };
    }
}
