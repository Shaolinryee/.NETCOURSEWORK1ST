using System.Security.Cryptography;
using System.Text;
using JournalManagementSystem.Data;
using JournalManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace JournalManagementSystem.Services;

public class SecurityService : ISecurityService
{
    private readonly AppDbContext _context;
    private static AuthenticationState? _currentAuthState;
    private static DateTime _lastActivityTime = DateTime.UtcNow;
    private const int SESSION_TIMEOUT_MINUTES = 30;
    private const int MAX_FAILED_ATTEMPTS = 5;
    private const int LOCKOUT_DURATION_MINUTES = 15;

    public SecurityService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Checks if security has been set up
    /// </summary>
    public async Task<bool> IsSecuritySetupCompleteAsync()
    {
        var security = await _context.UserSecurities.FirstOrDefaultAsync();
        return security != null && !string.IsNullOrEmpty(security.PasswordHash);
    }

    /// <summary>
    /// Initial security setup with password
    /// </summary>
    public async Task<bool> SetupSecurityAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return false;

        try
        {
            var existingRecord = await _context.UserSecurities.FirstOrDefaultAsync();

            var (hash, salt) = HashPassword(password);

            if (existingRecord != null)
            {
                existingRecord.PasswordHash = hash;
                existingRecord.PasswordSalt = salt;
                existingRecord.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var security = new UserSecurity
                {
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _context.UserSecurities.AddAsync(security);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Authenticate using password
    /// </summary>
    public async Task<bool> AuthenticateWithPasswordAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        try
        {
            var security = await _context.UserSecurities.FirstOrDefaultAsync();
            if (security == null)
                return false;

            // Check if account is locked
            if (security.IsLockedOut && security.LockoutExpiresAt > DateTime.UtcNow)
                return false;

            // Reset lockout if expired
            if (security.LockoutExpiresAt <= DateTime.UtcNow)
            {
                security.IsLockedOut = false;
                security.FailedLoginAttempts = 0;
                security.LockoutExpiresAt = null;
            }

            // Verify password
            bool isValid = VerifyPassword(password, security.PasswordHash, security.PasswordSalt);

            if (isValid)
            {
                security.FailedLoginAttempts = 0;
                security.IsLockedOut = false;
                security.LockoutExpiresAt = null;
                security.LastLoginTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _currentAuthState = new AuthenticationState
                {
                    IsAuthenticated = true,
                    LastAuthenticationTime = DateTime.UtcNow,
                    AuthenticationMethod = "Password",
                    SessionExpirationTime = DateTime.UtcNow.AddMinutes(SESSION_TIMEOUT_MINUTES)
                };
                _lastActivityTime = DateTime.UtcNow;

                return true;
            }
            else
            {
                security.FailedLoginAttempts++;

                if (security.FailedLoginAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    security.IsLockedOut = true;
                    security.LockoutExpiresAt = DateTime.UtcNow.AddMinutes(LOCKOUT_DURATION_MINUTES);
                }

                await _context.SaveChangesAsync();
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Authenticate using PIN
    /// </summary>
    public async Task<bool> AuthenticateWithPinAsync(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4 || pin.Length > 6)
            return false;

        try
        {
            var security = await _context.UserSecurities.FirstOrDefaultAsync();
            if (security == null || !security.IsPinEnabled || string.IsNullOrEmpty(security.PinHash))
                return false;

            // Check if account is locked
            if (security.IsLockedOut && security.LockoutExpiresAt > DateTime.UtcNow)
                return false;

            bool isValid = VerifyPassword(pin, security.PinHash, security.PinSalt ?? "");

            if (isValid)
            {
                security.FailedLoginAttempts = 0;
                security.IsLockedOut = false;
                security.LockoutExpiresAt = null;
                security.LastLoginTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _currentAuthState = new AuthenticationState
                {
                    IsAuthenticated = true,
                    LastAuthenticationTime = DateTime.UtcNow,
                    AuthenticationMethod = "PIN",
                    SessionExpirationTime = DateTime.UtcNow.AddMinutes(SESSION_TIMEOUT_MINUTES)
                };
                _lastActivityTime = DateTime.UtcNow;

                return true;
            }
            else
            {
                security.FailedLoginAttempts++;
                if (security.FailedLoginAttempts >= MAX_FAILED_ATTEMPTS)
                {
                    security.IsLockedOut = true;
                    security.LockoutExpiresAt = DateTime.UtcNow.AddMinutes(LOCKOUT_DURATION_MINUTES);
                }
                await _context.SaveChangesAsync();
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verify current session is still valid
    /// </summary>
    public async Task<bool> VerifyCurrentSessionAsync()
    {
        if (_currentAuthState == null || !_currentAuthState.IsAuthenticated)
            return false;

        // Check session expiration
        if (_currentAuthState.SessionExpirationTime < DateTime.UtcNow)
        {
            _currentAuthState.IsAuthenticated = false;
            return false;
        }

        // Update session expiration on activity
        _lastActivityTime = DateTime.UtcNow;
        _currentAuthState.SessionExpirationTime = DateTime.UtcNow.AddMinutes(SESSION_TIMEOUT_MINUTES);

        return true;
    }

    /// <summary>
    /// Setup PIN for quick access
    /// </summary>
    public async Task<bool> SetupPinAsync(string pin)
    {
        if (!await VerifyCurrentSessionAsync())
            return false;

        if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4 || pin.Length > 6 || !pin.All(char.IsDigit))
            return false;

        try
        {
            var security = await _context.UserSecurities.FirstOrDefaultAsync();
            if (security == null)
                return false;

            var (hash, salt) = HashPassword(pin);
            security.PinHash = hash;
            security.PinSalt = salt;
            security.IsPinEnabled = true;
            security.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Remove PIN protection
    /// </summary>
    public async Task<bool> RemovePinAsync()
    {
        if (!await VerifyCurrentSessionAsync())
            return false;

        try
        {
            var security = await _context.UserSecurities.FirstOrDefaultAsync();
            if (security == null)
                return false;

            security.PinHash = null;
            security.PinSalt = null;
            security.IsPinEnabled = false;
            security.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Change PIN
    /// </summary>
    public async Task<bool> ChangePinAsync(string oldPin, string newPin)
    {
        if (!await AuthenticateWithPinAsync(oldPin))
            return false;

        return await SetupPinAsync(newPin);
    }

    /// <summary>
    /// Change password
    /// </summary>
    public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        if (!await AuthenticateWithPasswordAsync(oldPassword))
            return false;

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return false;

        return await SetupSecurityAsync(newPassword);
    }

    /// <summary>
    /// Reset password (admin/recovery)
    /// </summary>
    public async Task<bool> ResetPasswordAsync(string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return false;

        return await SetupSecurityAsync(newPassword);
    }

    /// <summary>
    /// Get current authentication state
    /// </summary>
    public async Task<AuthenticationState> GetCurrentAuthStateAsync()
    {
        await VerifyCurrentSessionAsync();
        return _currentAuthState ?? new AuthenticationState { IsAuthenticated = false };
    }

    /// <summary>
    /// Logout current session
    /// </summary>
    public Task LogoutAsync()
    {
        _currentAuthState = new AuthenticationState { IsAuthenticated = false };
        return Task.CompletedTask;
    }

    /// <summary>
    /// Get session timeout in minutes
    /// </summary>
    public async Task<int> GetSessionTimeoutMinutesAsync()
    {
        await VerifyCurrentSessionAsync();
        return SESSION_TIMEOUT_MINUTES;
    }

    /// <summary>
    /// Get security information
    /// </summary>
    public async Task<UserSecurity?> GetSecurityInfoAsync()
    {
        if (!await VerifyCurrentSessionAsync())
            return null;

        return await _context.UserSecurities.FirstOrDefaultAsync();
    }

    // Private helper methods

    /// <summary>
    /// Hash password using PBKDF2
    /// </summary>
    private (string hash, string salt) HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] saltBytes = new byte[32];
        rng.GetBytes(saltBytes);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);

        string saltString = Convert.ToBase64String(saltBytes);
        string hashString = Convert.ToBase64String(hash);

        return (hashString, saltString);
    }

    /// <summary>
    /// Verify password hash
    /// </summary>
    private bool VerifyPassword(string password, string hash, string salt)
    {
        try
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(32);
            byte[] hashBytes = Convert.FromBase64String(hash);

            return computedHash.SequenceEqual(hashBytes);
        }
        catch
        {
            return false;
        }
    }
}
