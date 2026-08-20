namespace myMotionApi.DTOs
{
    // ── REQUEST DTOs ─────────────────────────────────────────────────────────

    public class CreateAttachmentRequest
    {
        /// <summary>ID của lời nhắn mà attachment thuộc về</summary>
        public Guid MessageId { get; set; }

        /// <summary>URL file đã upload (ví dụ: Supabase Storage URL)</summary>
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>"image" hoặc "video"</summary>
        public string FileType { get; set; } = "image";

        public string? OriginalFileName { get; set; }
        public long? FileSizeBytes { get; set; }
    }

    public class UpdateAttachmentRequest
    {
        public string? FileUrl { get; set; }
        public string? FileType { get; set; }
        public string? OriginalFileName { get; set; }
    }
}
