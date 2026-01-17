namespace JournalManagementSystem.Models;

/// <summary>
/// Model for storing user security and authentication information
/// </summary>
public class UserSecurity
{
    public int Id { get; set; }

    // Password Authentication
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    // PIN Authentication
    public bool IsPinEnabled { get; set; }
    public string? PinHash { get; set; }
    public string? PinSalt { get; set; }

    // Account Security
    public bool IsLockedOut { get; set; }
    public DateTime? LockoutExpiresAt { get; set; }
    public int FailedLoginAttempts { get; set; }

    // Activity Tracking
    public DateTime? LastLoginTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
