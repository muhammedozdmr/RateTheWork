using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Infrastructure.Services;

public class PasswordHashingService : IPasswordHashingService
{
    private readonly IConfiguration _configuration;
    private readonly int _workFactor;

    public PasswordHashingService(IConfiguration configuration)
    {
        _configuration = configuration;
        _workFactor = configuration.GetValue<int>("Security:BCrypt:WorkFactor", 12);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }

    public string GenerateStrongPassword(int length = 16)
    {
        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        const string allChars = upperCase + lowerCase + digits + specialChars;

        var random = new Random();
        var password = new StringBuilder();

        // Ensure at least one character from each category
        password.Append(upperCase[random.Next(upperCase.Length)]);
        password.Append(lowerCase[random.Next(lowerCase.Length)]);
        password.Append(digits[random.Next(digits.Length)]);
        password.Append(specialChars[random.Next(specialChars.Length)]);

        // Fill the rest randomly
        for (int i = 4; i < length; i++)
        {
            password.Append(allChars[random.Next(allChars.Length)]);
        }

        // Shuffle the password
        return new string(password.ToString().OrderBy(x => random.Next()).ToArray());
    }

    public bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        bool hasUpperCase = password.Any(char.IsUpper);
        bool hasLowerCase = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]");

        int criteriaCount = new[] { hasUpperCase, hasLowerCase, hasDigit, hasSpecialChar }.Count(x => x);

        return criteriaCount >= 3;
    }
}
