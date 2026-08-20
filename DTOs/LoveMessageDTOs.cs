namespace myMotionApi.DTOs
{
    // ── REQUEST DTOs ─────────────────────────────────────────────────────────

    public class CreateLoveMessageRequest
    {
        /// <summary>Nội dung lời nhắn (bắt buộc)</summary>
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateLoveMessageRequest
    {
        /// <summary>Nội dung mới (nếu muốn sửa)</summary>
        public string? Content { get; set; }

        /// <summary>Đánh dấu đã đọc</summary>
        public bool? IsRead { get; set; }
    }

    // ── RESPONSE DTOs ────────────────────────────────────────────────────────

    public class AttachmentResponse
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? OriginalFileName { get; set; }
        public long? FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class LoveMessageResponse
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public List<AttachmentResponse> Attachments { get; set; } = new();
    }
}
