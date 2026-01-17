using JournalManagementSystem.Models;

namespace JournalManagementSystem.Services;

/// <summary>
/// Interface for security and authentication operations
/// </summary>
public interface ISecurityService
{
    // Setup & Configuration
    Task<bool> IsSecuritySetupCompleteAsync();
    Task<bool> SetupSecurityAsync(string password);

    // Authentication
    Task<bool> AuthenticateWithPasswordAsync(string password);
    Task<bool> AuthenticateWithPinAsync(string pin);
    Task<bool> VerifyCurrentSessionAsync();

    // PIN Management
    Task<bool> SetupPinAsync(string pin);
    Task<bool> RemovePinAsync();
    Task<bool> ChangePinAsync(string oldPin, string newPin);

    // Password Management
    Task<bool> ChangePasswordAsync(string oldPassword, string newPassword);
    Task<bool> ResetPasswordAsync(string newPassword);

    // Session Management
    Task<AuthenticationState> GetCurrentAuthStateAsync();
    Task LogoutAsync();
    Task<int> GetSessionTimeoutMinutesAsync();

    // Security Info
    Task<UserSecurity?> GetSecurityInfoAsync();
}

public class AuthenticationState
{
    public bool IsAuthenticated { get; set; }
    public DateTime? LastAuthenticationTime { get; set; }
    public string? AuthenticationMethod { get; set; }
    public DateTime? SessionExpirationTime { get; set; }
}
