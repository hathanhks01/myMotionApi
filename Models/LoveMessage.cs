namespace myMotionApi.Models
{
    /// <summary>
    /// Lời nhắn tỏ tình hằng ngày từ người gửi đến người nhận.
    /// SenderId và ReceiverId được hardcode từ biến môi trường (không cần đăng nhập).
    /// </summary>
    public class LoveMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>GUID cố định của người gửi (lấy từ .env SENDER_ID)</summary>
        public Guid SenderId { get; set; }

        /// <summary>GUID cố định của người nhận (lấy từ .env RECEIVER_ID)</summary>
        public Guid ReceiverId { get; set; }

        /// <summary>Nội dung lời nhắn / lời tỏ tình</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>Thời điểm gửi (UTC)</summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>Người nhận đã xem chưa</summary>
        public bool IsRead { get; set; } = false;

        /// <summary>Thời điểm người nhận xem lần đầu (UTC)</summary>
        public DateTime? ReadAt { get; set; }

        // Navigation property
        public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
    }
}
