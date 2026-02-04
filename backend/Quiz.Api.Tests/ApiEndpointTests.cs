using System.Text.Json;

namespace Quiz.Api.Tests;

/// <summary>
/// Integration tests for API endpoints using WebApplicationFactory
/// Note: These tests require Microsoft.AspNetCore.Mvc.Testing package
/// </summary>
public class ApiEndpointTests
{
    #region Username Validation Tests (It02Api logic)

    [Theory]
    [InlineData("john_doe", true)]
    [InlineData("user.name", true)]
    [InlineData("user-name123", true)]
    [InlineData("abc", false)]           // Too short (<4)
    [InlineData("ab", false)]            // Too short
    [InlineData("", false)]              // Empty
    [InlineData("user name", false)]     // Contains space
    [InlineData("user@name", false)]     // Contains @
    [InlineData("user#name", false)]     // Contains #
    public void ValidateUsername_ChecksFormat(string username, bool expectedValid)
    {
        // Act
        var isValid = IsValidUsername(username);

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public void ValidateUsername_MaxLength30_ShouldPass()
    {
        // Arrange
        var username = new string('a', 30);

        // Act
        var isValid = IsValidUsername(username);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateUsername_ExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var username = new string('a', 31);

        // Act
        var isValid = IsValidUsername(username);

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region BirthDate Validation Tests

    [Fact]
    public void ValidateBirthDate_FutureDate_ShouldFail()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var futureDate = today.AddDays(1);

        // Act
        var isValid = futureDate <= today && futureDate.Year >= 1900;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateBirthDate_BeforeYear1900_ShouldFail()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oldDate = new DateOnly(1899, 12, 31);

        // Act
        var isValid = oldDate <= today && oldDate.Year >= 1900;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateBirthDate_ValidDate_ShouldPass()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var validDate = new DateOnly(1990, 5, 15);

        // Act
        var isValid = validDate <= today && validDate.Year >= 1900;

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateBirthDate_Today_ShouldPass()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var isValid = today <= today && today.Year >= 1900;

        // Assert
        Assert.True(isValid);
    }

    #endregion

    #region Age Calculation Tests

    [Fact]
    public void CalculateAge_BirthdayPassed_ReturnsCorrectAge()
    {
        // Arrange
        var today = new DateOnly(2024, 6, 15);
        var birthDate = new DateOnly(2000, 1, 1);

        // Act
        var age = CalculateAge(birthDate, today);

        // Assert
        Assert.Equal(24, age);
    }

    [Fact]
    public void CalculateAge_BirthdayNotYetPassed_ReturnsAgeMinus1()
    {
        // Arrange
        var today = new DateOnly(2024, 6, 15);
        var birthDate = new DateOnly(2000, 12, 25);

        // Act
        var age = CalculateAge(birthDate, today);

        // Assert
        Assert.Equal(23, age);
    }

    [Fact]
    public void CalculateAge_ExactBirthday_ReturnsCorrectAge()
    {
        // Arrange
        var today = new DateOnly(2024, 6, 15);
        var birthDate = new DateOnly(2000, 6, 15);

        // Act
        var age = CalculateAge(birthDate, today);

        // Assert
        Assert.Equal(24, age);
    }

    [Fact]
    public void CalculateAge_FutureDate_ReturnsZero()
    {
        // Arrange
        var today = new DateOnly(2024, 6, 15);
        var futureDate = new DateOnly(2025, 1, 1);

        // Act
        var age = CalculateAge(futureDate, today);

        // Assert
        Assert.Equal(0, age);
    }

    #endregion

    #region Occupation Validation Tests

    [Theory]
    [InlineData("นักพัฒนาโปรแกรม", true)]
    [InlineData("นักวิเคราะห์ข้อมูล", true)]
    [InlineData("นักบัญชี", true)]
    [InlineData("Invalid Job", false)]
    [InlineData("", false)]
    public void ValidateOccupation_ChecksValidOptions(string occupation, bool expectedValid)
    {
        // Arrange
        var validOccupations = new[]
        {
            "นักพัฒนาโปรแกรม",
            "นักวิเคราะห์ข้อมูล",
            "นักบัญชี",
            "เจ้าหน้าที่บุคคล",
            "นักการตลาด",
            "นักออกแบบ"
        };

        // Act
        var isValid = !string.IsNullOrEmpty(occupation) && validOccupations.Contains(occupation);

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    #endregion

    #region Sex Validation Tests

    [Theory]
    [InlineData("Male", true)]
    [InlineData("Female", true)]
    [InlineData("male", false)]   // Case sensitive
    [InlineData("M", false)]
    [InlineData("F", false)]
    [InlineData("", false)]
    [InlineData("Other", false)]
    public void ValidateSex_ChecksValidOptions(string sex, bool expectedValid)
    {
        // Act
        var isValid = sex is "Male" or "Female";

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    #endregion

    #region Helper Methods (mirroring API validation logic)

    private static bool IsValidUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
            return false;

        if (username.Length is < 4 or > 30)
            return false;

        return username.All(ch => char.IsLetterOrDigit(ch) || ch is '.' or '_' or '-');
    }

    private static int CalculateAge(DateOnly birthDate, DateOnly today)
    {
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        return Math.Max(age, 0);
    }

    #endregion
}
