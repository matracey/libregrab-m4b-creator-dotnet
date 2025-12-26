using LibreGrabM4BCreator.Core.Models;

namespace LibreGrabM4BCreator.Tests.Core.UnitTests.Models;

public class ConversionOptionsTests
{
    [Fact]
    public void ConversionOptions_DefaultTelegramMode_IsFalse()
    {
        // Arrange
        var options = new ConversionOptions
        {
            OutputDirectory = new DirectoryInfo(Path.GetTempPath())
        };

        // Assert
        Assert.False(options.TelegramMode);
    }

    [Fact]
    public void ConversionOptions_TelegramModeTrue_CanBeSet()
    {
        // Arrange
        var options = new ConversionOptions
        {
            OutputDirectory = new DirectoryInfo(Path.GetTempPath()),
            TelegramMode = true
        };

        // Assert
        Assert.True(options.TelegramMode);
    }

    [Fact]
    public void ConversionOptions_OutputDirectory_IsRequired()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var dirInfo = new DirectoryInfo(tempDir);

        var options = new ConversionOptions
        {
            OutputDirectory = dirInfo
        };

        // Assert
        Assert.Equal(tempDir, options.OutputDirectory.FullName);
    }

    [Fact]
    public void ConversionOptions_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var dir = new DirectoryInfo(Path.GetTempPath());
        
        var options1 = new ConversionOptions
        {
            OutputDirectory = dir,
            TelegramMode = true
        };

        var options2 = new ConversionOptions
        {
            OutputDirectory = dir,
            TelegramMode = true
        };

        // Act & Assert
        Assert.Equal(options1, options2);
    }

    [Fact]
    public void ConversionOptions_RecordInequality_WorksCorrectly()
    {
        // Arrange
        var dir = new DirectoryInfo(Path.GetTempPath());
        
        var options1 = new ConversionOptions
        {
            OutputDirectory = dir,
            TelegramMode = false
        };

        var options2 = new ConversionOptions
        {
            OutputDirectory = dir,
            TelegramMode = true
        };

        // Act & Assert
        Assert.NotEqual(options1, options2);
    }
}
