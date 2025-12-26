using System.Reflection;

using LibreGrabM4BCreator.Core.Services;

namespace LibreGrabM4BCreator.Tests.Core.UnitTests.Services;

public class ConversionServiceSanitizeFileNameTests
{
    private static string InvokeSanitizeFileName(string fileName)
    {
        // Use reflection to test the private static method
        var method = typeof(ConversionService).GetMethod(
            "SanitizeFileName", 
            BindingFlags.NonPublic | BindingFlags.Static);
        
        return (string)method!.Invoke(null, [fileName])!;
    }

    [Fact]
    public void SanitizeFileName_ValidName_ReturnsUnchanged()
    {
        // Arrange
        var fileName = "My Audiobook";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("My Audiobook", result);
    }

    [Fact]
    public void SanitizeFileName_WithColon_ReplacesWithUnderscore()
    {
        // Arrange
        var fileName = "Book: Part 1";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("Book_ Part 1", result);
    }

    [Fact]
    public void SanitizeFileName_WithDoubleQuotes_ReplacesWithSingleQuotes()
    {
        // Arrange
        var fileName = "The \"Great\" Book";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("The 'Great' Book", result);
    }

    [Fact]
    public void SanitizeFileName_WithLeadingSpaces_TrimsSpaces()
    {
        // Arrange
        var fileName = "  Leading Spaces";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("Leading Spaces", result);
    }

    [Fact]
    public void SanitizeFileName_WithTrailingSpaces_TrimsSpaces()
    {
        // Arrange
        var fileName = "Trailing Spaces  ";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("Trailing Spaces", result);
    }

    [Fact]
    public void SanitizeFileName_WithSlash_ReplacesWithUnderscore()
    {
        // Arrange
        var fileName = "Book / Part 1";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("Book _ Part 1", result);
    }

    [Fact]
    public void SanitizeFileName_WithBackslash_BehaviorDependsOnPlatform()
    {
        // Arrange
        var fileName = "Book \\ Part 1";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        // On Windows, backslash is invalid and gets replaced
        // On macOS/Linux, backslash is valid and preserved
        if (OperatingSystem.IsWindows())
        {
            Assert.Equal("Book _ Part 1", result);
        }
        else
        {
            Assert.Equal("Book \\ Part 1", result);
        }
    }

    [Fact]
    public void SanitizeFileName_WithMultipleInvalidChars_ReplacesAll()
    {
        // Arrange
        var fileName = "Book: \"Part\" 1/2";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("Book_ 'Part' 1_2", result);
    }

    [Theory]
    [InlineData("Simple Name", "Simple Name")]
    [InlineData("Name With Numbers 123", "Name With Numbers 123")]
    [InlineData("Name-With-Dashes", "Name-With-Dashes")]
    [InlineData("Name_With_Underscores", "Name_With_Underscores")]
    [InlineData("Name.With.Dots", "Name.With.Dots")]
    public void SanitizeFileName_ValidCharacters_PreservesName(string input, string expected)
    {
        // Act
        var result = InvokeSanitizeFileName(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitizeFileName_InvalidPathCharacters_BehaviorDependsOnPlatform()
    {
        // Path.GetInvalidFileNameChars() returns different characters on different platforms
        // Windows: ?, *, <, >, |, \, /, :, ", null, and control chars
        // macOS/Linux: /, null
        
        // Test colon specifically as it's handled explicitly by the method
        var colonResult = InvokeSanitizeFileName("A:B");
        Assert.Equal("A_B", colonResult);
        
        // Test forward slash - invalid on all platforms
        var slashResult = InvokeSanitizeFileName("A/B");
        Assert.Equal("A_B", slashResult);
        
        // Test double quotes - handled explicitly by the method
        var quotesResult = InvokeSanitizeFileName("A\"B");
        Assert.Equal("A'B", quotesResult);
    }

    [Fact]
    public void SanitizeFileName_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var fileName = "";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void SanitizeFileName_OnlyWhitespace_ReturnsEmptyString()
    {
        // Arrange
        var fileName = "   ";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void SanitizeFileName_UnicodeCharacters_PreservesUnicode()
    {
        // Arrange
        var fileName = "日本語の本";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("日本語の本", result);
    }

    [Fact]
    public void SanitizeFileName_EmojiCharacters_PreservesEmojis()
    {
        // Arrange
        var fileName = "Book 📚 Title";

        // Act
        var result = InvokeSanitizeFileName(fileName);

        // Assert
        Assert.Equal("Book 📚 Title", result);
    }
}
