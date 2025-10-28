// RealEstate.Domain/Entities/User.cs
using RealEstate.Domain.Enums;

namespace RealEstate.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PasswordHash { get; set; } // Null for OAuth users
    public string? PhoneNumber { get; set; }
    public string? Agency { get; set; } // For Closers
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool IsOAuthUser { get; set; }
    public string? OAuthProvider { get; set; } // "Google", etc.
    public string? OAuthSubject { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    
    // Encrypted PII fields
    public string? EncryptedPhoneNumber { get; set; }
    public string? EncryptedAgency { get; set; }
}