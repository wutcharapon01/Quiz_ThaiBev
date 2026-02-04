using Quiz.Api.Security;

namespace Quiz.Api.Tests;

/// <summary>
/// Unit tests for AuthSecurity password hashing and validation
/// </summary>
public class AuthSecurityTests
{
    #region Password Hashing Tests

    [Fact]
    public void HashPassword_ReturnsNonEmptyHashAndSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = AuthSecurity.HashPassword(password);

        // Assert
        Assert.False(string.IsNullOrEmpty(hash));
        Assert.False(string.IsNullOrEmpty(salt));
    }

    [Fact]
    public void HashPassword_DifferentPasswordsProduceDifferentHashes()
    {
        // Arrange
        var password1 = "Password123!";
        var password2 = "DifferentPass456!";

        // Act
        var (hash1, _) = AuthSecurity.HashPassword(password1);
        var (hash2, _) = AuthSecurity.HashPassword(password2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashPassword_SamePasswordProducesDifferentHashes_DueToDifferentSalts()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash1, salt1) = AuthSecurity.HashPassword(password);
        var (hash2, salt2) = AuthSecurity.HashPassword(password);

        // Assert - Same password should have different hashes due to random salt
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(salt1, salt2);
    }

    #endregion

    #region Password Verification Tests

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var (hash, salt) = AuthSecurity.HashPassword(password);

        // Act
        var result = AuthSecurity.VerifyPassword(password, hash, salt);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";
        var (hash, salt) = AuthSecurity.HashPassword(correctPassword);

        // Act
        var result = AuthSecurity.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_InvalidBase64Hash_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "not-valid-base64!!!";
        var (_, salt) = AuthSecurity.HashPassword(password);

        // Act
        var result = AuthSecurity.VerifyPassword(password, invalidHash, salt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_InvalidBase64Salt_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var (hash, _) = AuthSecurity.HashPassword(password);
        var invalidSalt = "not-valid-base64!!!";

        // Act
        var result = AuthSecurity.VerifyPassword(password, hash, invalidSalt);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Password Strength Tests

    [Theory]
    [InlineData("Aa1!xxxx", true)]     // Valid: 8 chars, upper, lower, digit, special
    [InlineData("Password1!", true)]    // Valid: all requirements met
    [InlineData("Abc123!@#xyz", true)]  // Valid: all requirements met
    [InlineData("short1!", false)]      // Invalid: less than 8 characters
    [InlineData("password1!", false)]   // Invalid: no uppercase
    [InlineData("PASSWORD1!", false)]   // Invalid: no lowercase
    [InlineData("Passworddd!", false)]  // Invalid: no digit
    [InlineData("Password123", false)]  // Invalid: no special character
    [InlineData("", false)]             // Invalid: empty
    [InlineData("Ab1!", false)]         // Invalid: too short
    public void IsStrongPassword_ValidatesCorrectly(string password, bool expectedResult)
    {
        // Act
        var result = AuthSecurity.IsStrongPassword(password);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    #endregion

    #region Signing Key Tests

    [Fact]
    public void CreateSigningKey_ReturnsNonNullKey()
    {
        // Arrange
        var keyString = "ThisIsATestKeyThatIsAtLeast32Characters!";

        // Act
        var key = AuthSecurity.CreateSigningKey(keyString);

        // Assert
        Assert.NotNull(key);
        Assert.True(key.KeySize > 0);
    }

    [Fact]
    public void CreateSigningKey_SameInputProducesSameKey()
    {
        // Arrange
        var keyString = "ThisIsATestKeyThatIsAtLeast32Characters!";

        // Act
        var key1 = AuthSecurity.CreateSigningKey(keyString);
        var key2 = AuthSecurity.CreateSigningKey(keyString);

        // Assert
        Assert.Equal(key1.Key, key2.Key);
    }

    #endregion
}
