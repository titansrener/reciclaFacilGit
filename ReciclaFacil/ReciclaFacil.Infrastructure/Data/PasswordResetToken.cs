namespace ReciclaFacil.Infrastructure.Data;

public sealed class PasswordResetToken
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public required string TokenHash { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }
    public LegacyUser? User { get; set; }
}
