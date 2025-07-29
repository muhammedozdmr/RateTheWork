using FluentAssertions;
using Microsoft.Playwright;
using RateTheWork.E2E.Tests.Common;

namespace RateTheWork.E2E.Tests.Scenarios;

/// <summary>
/// End-to-end tests for user registration flow.
/// Tests the complete user journey from landing page to successful registration and email verification.
/// </summary>
public class UserRegistrationE2ETests : E2ETestBase
{
    [Fact]
    public async Task CompleteUserRegistration_Should_CreateAccountAndVerifyEmail()
    {
        // Arrange
        var email = $"e2e.test.{Guid.NewGuid()}@example.com";
        var password = "Test123!@#";

        // Act & Assert - Navigate to registration page
        await Page.GotoAsync("/");
        await Page.ClickAsync("text=Sign Up");
        await Page.WaitForURLAsync("**/register");

        // Fill registration form
        await Page.FillAsync("input[name='firstName']", "E2E");
        await Page.FillAsync("input[name='lastName']", "Test");
        await Page.FillAsync("input[name='email']", email);
        await Page.FillAsync("input[name='password']", password);
        await Page.FillAsync("input[name='confirmPassword']", password);
        await Page.FillAsync("input[name='phoneNumber']", "+905551234567");

        // Accept terms if present
        var termsCheckbox = await Page.QuerySelectorAsync("input[type='checkbox'][name='acceptTerms']");
        if (termsCheckbox != null)
        {
            await termsCheckbox.ClickAsync();
        }

        // Submit form
        await Page.ClickAsync("button[type='submit']");

        // Wait for success message or redirect
        var successMessage = await Page.WaitForSelectorAsync("text=Registration successful", new PageWaitForSelectorOptions
        {
            Timeout = 10000,
            State = WaitForSelectorState.Visible
        });

        successMessage.Should().NotBeNull();

        // Verify user in database
        var user = await ExecuteDbContextAsync(async db =>
            await db.Users.FirstOrDefaultAsync(u => u.Email == email));

        user.Should().NotBeNull();
        user!.IsEmailVerified.Should().BeFalse();
        user.EmailVerificationToken.Should().NotBeNullOrEmpty();

        // Take screenshot of success state
        await TakeScreenshotAsync("registration_success");
    }

    [Fact]
    public async Task Registration_Should_ShowValidationErrors_WhenInvalidData()
    {
        // Act - Navigate to registration page
        await Page.GotoAsync("/register");

        // Submit empty form
        await Page.ClickAsync("button[type='submit']");

        // Assert - Check for validation errors
        var emailError = await Page.WaitForSelectorAsync("text=Email is required");
        var passwordError = await Page.WaitForSelectorAsync("text=Password is required");

        emailError.Should().NotBeNull();
        passwordError.Should().NotBeNull();

        // Fill with invalid email
        await Page.FillAsync("input[name='email']", "invalid-email");
        await Page.ClickAsync("button[type='submit']");

        var invalidEmailError = await Page.WaitForSelectorAsync("text=Invalid email format");
        invalidEmailError.Should().NotBeNull();

        // Take screenshot of validation errors
        await TakeScreenshotAsync("registration_validation_errors");
    }

    [Fact]
    public async Task Registration_Should_PreventDuplicateEmail()
    {
        // Arrange
        var email = "existing@example.com";
        var password = "Test123!@#";

        // Create existing user
        await ExecuteDbContextAsync(async db =>
        {
            var existingUser = Domain.Entities.User.CreateForTesting(email, "existinguser");
            db.Users.Add(existingUser);
            await db.SaveChangesAsync();
        });

        // Act - Try to register with same email
        await Page.GotoAsync("/register");
        
        await Page.FillAsync("input[name='firstName']", "Duplicate");
        await Page.FillAsync("input[name='lastName']", "Test");
        await Page.FillAsync("input[name='email']", email);
        await Page.FillAsync("input[name='password']", password);
        await Page.FillAsync("input[name='confirmPassword']", password);
        await Page.FillAsync("input[name='phoneNumber']", "+905559999999");

        await Page.ClickAsync("button[type='submit']");

        // Assert - Should show error
        var errorMessage = await Page.WaitForSelectorAsync("text=Email already exists");
        errorMessage.Should().NotBeNull();

        await TakeScreenshotAsync("registration_duplicate_email");
    }

    [Fact]
    public async Task PasswordStrengthIndicator_Should_UpdateAsUserTypes()
    {
        // Act - Navigate to registration
        await Page.GotoAsync("/register");

        var passwordInput = await Page.QuerySelectorAsync("input[name='password']");
        passwordInput.Should().NotBeNull();

        // Type weak password
        await passwordInput!.FillAsync("123");
        var weakIndicator = await Page.QuerySelectorAsync(".password-strength-weak");
        weakIndicator.Should().NotBeNull();

        // Type medium password
        await passwordInput.FillAsync("Test123");
        var mediumIndicator = await Page.QuerySelectorAsync(".password-strength-medium");
        mediumIndicator.Should().NotBeNull();

        // Type strong password
        await passwordInput.FillAsync("Test123!@#$%");
        var strongIndicator = await Page.QuerySelectorAsync(".password-strength-strong");
        strongIndicator.Should().NotBeNull();

        await TakeScreenshotAsync("password_strength_indicator");
    }

    [Fact]
    public async Task EmailVerification_Should_ActivateAccount()
    {
        // Arrange - Register a user
        var email = $"verify.{Guid.NewGuid()}@example.com";
        var password = "Test123!@#";

        await Page.GotoAsync("/register");
        await Page.FillAsync("input[name='firstName']", "Verify");
        await Page.FillAsync("input[name='lastName']", "Test");
        await Page.FillAsync("input[name='email']", email);
        await Page.FillAsync("input[name='password']", password);
        await Page.FillAsync("input[name='confirmPassword']", password);
        await Page.FillAsync("input[name='phoneNumber']", "+905558888888");

        await Page.ClickAsync("button[type='submit']");
        await Page.WaitForSelectorAsync("text=Registration successful");

        // Get verification token from database
        var token = await ExecuteDbContextAsync(async db =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user?.EmailVerificationToken;
        });

        token.Should().NotBeNullOrEmpty();

        // Act - Visit verification link
        await Page.GotoAsync($"/verify-email?token={token}");

        // Assert - Should show success
        var successMessage = await Page.WaitForSelectorAsync("text=Email verified successfully");
        successMessage.Should().NotBeNull();

        // Verify in database
        var isVerified = await ExecuteDbContextAsync(async db =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user?.IsEmailVerified ?? false;
        });

        isVerified.Should().BeTrue();

        await TakeScreenshotAsync("email_verification_success");
    }
}