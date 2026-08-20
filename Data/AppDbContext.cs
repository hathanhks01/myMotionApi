using Microsoft.EntityFrameworkCore;
using myMotionApi.Models;

namespace myMotionApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LoveMessage> LoveMessages { get; set; }
        public DbSet<MessageAttachment> MessageAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── LoveMessage ──────────────────────────────────────────────
            modelBuilder.Entity<LoveMessage>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Content)
                      .IsRequired()
                      .HasMaxLength(5000);

                entity.Property(m => m.SentAt)
                      .IsRequired();

                entity.Property(m => m.SenderId)
                      .IsRequired();

                entity.Property(m => m.ReceiverId)
                      .IsRequired();

                // Index để query theo ngày nhanh hơn
                entity.HasIndex(m => m.SentAt)
                      .HasDatabaseName("IX_LoveMessages_SentAt");

                // Index để lọc theo người nhận + ngày (dùng cho timeline)
                entity.HasIndex(m => new { m.ReceiverId, m.SentAt })
                      .HasDatabaseName("IX_LoveMessages_ReceiverId_SentAt");
            });

            // ── MessageAttachment ─────────────────────────────────────────
            modelBuilder.Entity<MessageAttachment>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.FileUrl)
                      .IsRequired()
                      .HasMaxLength(2048);

                entity.Property(a => a.FileType)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(a => a.OriginalFileName)
                      .HasMaxLength(255);

                // Quan hệ: LoveMessage (1) ──< MessageAttachment (N)
                entity.HasOne(a => a.Message)
                      .WithMany(m => m.Attachments)
                      .HasForeignKey(a => a.MessageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
