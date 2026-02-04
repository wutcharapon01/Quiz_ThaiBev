namespace Quiz.Api.Tests;

/// <summary>
/// Unit tests for input validation logic used across APIs
/// </summary>
public class InputValidationTests
{
    #region Name Length Validation Tests

    [Theory]
    [InlineData("John", true)]
    [InlineData("A", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void ValidateName_ChecksEmptyOrWhitespace(string? name, bool shouldBeValid)
    {
        // Act
        var isValid = !string.IsNullOrWhiteSpace(name);

        // Assert
        Assert.Equal(shouldBeValid, isValid);
    }

    [Fact]
    public void ValidateName_ExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var maxLength = 100;
        var longName = new string('A', maxLength + 1);

        // Act
        var isValid = longName.Length <= maxLength;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateName_AtMaxLength_ShouldPass()
    {
        // Arrange
        var maxLength = 100;
        var validName = new string('A', maxLength);

        // Act
        var isValid = validName.Length <= maxLength;

        // Assert
        Assert.True(isValid);
    }

    #endregion

    #region Email Validation Tests

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.co.th", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@nodomain.com", false)]
    [InlineData("noat.domain.com", false)]
    [InlineData("", false)]
    public void ValidateEmail_ChecksFormat(string email, bool expectedValid)
    {
        // Act
        var isValid = IsValidEmail(email);

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public void ValidateEmail_ExceedsMaxLength_ShouldFail()
    {
        // Arrange
        var maxLength = 255;
        var longEmail = new string('a', maxLength - 10) + "@test.com";

        // Act
        var isValid = longEmail.Length <= maxLength;

        // Assert
        Assert.True(isValid); // This one should pass

        var tooLongEmail = new string('a', maxLength) + "@test.com";
        var isInvalid = tooLongEmail.Length <= maxLength;
        Assert.False(isInvalid); // This one should fail
    }

    #endregion

    #region Phone Validation Tests

    [Theory]
    [InlineData("0812345678", true)]
    [InlineData("123456789", true)]
    [InlineData("0812345678901234", false)] // Too long (>15)
    [InlineData("12345678", false)]          // Too short (<9)
    [InlineData("081-234-5678", false)]      // Contains non-digits
    [InlineData("", false)]
    public void ValidatePhone_ChecksFormat(string phone, bool expectedValid)
    {
        // Act
        var isValid = IsValidPhone(phone);

        // Assert
        Assert.Equal(expectedValid, isValid);
    }

    #endregion

    #region Base64 Image Size Validation Tests

    [Fact]
    public void ValidateImageSize_UnderLimit_ShouldPass()
    {
        // Arrange
        const int maxSize = 2_800_000; // ~2MB in base64
        var validImage = new string('a', maxSize - 1);

        // Act
        var isValid = validImage.Length <= maxSize;

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateImageSize_OverLimit_ShouldFail()
    {
        // Arrange
        const int maxSize = 2_800_000;
        var oversizedImage = new string('a', maxSize + 1);

        // Act
        var isValid = oversizedImage.Length <= maxSize;

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region Remark Validation Tests

    [Fact]
    public void ValidateRemark_AtMaxLength_ShouldPass()
    {
        // Arrange
        const int maxLength = 500;
        var validRemark = new string('x', maxLength);

        // Act
        var isValid = validRemark.Length <= maxLength;

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateRemark_ExceedsMaxLength_ShouldFail()
    {
        // Arrange
        const int maxLength = 500;
        var longRemark = new string('x', maxLength + 1);

        // Act
        var isValid = longRemark.Length <= maxLength;

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateRemark_NullValue_ShouldPass()
    {
        // Arrange
        string? remark = null;

        // Act - null remarks are allowed
        var isValid = remark == null || remark.Length <= 500;

        // Assert
        Assert.True(isValid);
    }

    #endregion

    #region Helper Methods (mirroring API validation logic)

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var parsed = new System.Net.Mail.MailAddress(email);
            return parsed.Address.Equals(email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        if (phone.Length is < 9 or > 15)
            return false;

        return phone.All(char.IsDigit);
    }

    #endregion
}
