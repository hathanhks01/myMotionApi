namespace myMotionApi.Models
{
    /// <summary>
    /// Tệp đính kèm (ảnh / video) của một lời nhắn tỏ tình.
    /// </summary>
    public class MessageAttachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK về LoveMessage</summary>
        public Guid MessageId { get; set; }

        /// <summary>URL của file ảnh / video đã upload</summary>
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>Loại file: "image" hoặc "video"</summary>
        public string FileType { get; set; } = "image";

        /// <summary>Tên gốc của file khi upload</summary>
        public string? OriginalFileName { get; set; }

        /// <summary>Dung lượng file tính bằng bytes</summary>
        public long? FileSizeBytes { get; set; }

        /// <summary>Thời điểm upload (UTC)</summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public LoveMessage Message { get; set; } = null!;
    }
}
