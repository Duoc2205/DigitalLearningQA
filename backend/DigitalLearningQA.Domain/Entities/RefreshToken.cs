namespace DigitalLearningQA.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    // Refresh Token
    public string Token { get; set; } = string.Empty;

    // Thời điểm hết hạn
    public DateTime ExpiredAt { get; set; }

    // Đã bị thu hồi hay chưa
    public bool IsRevoked { get; set; }

    // Thời điểm tạo
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}