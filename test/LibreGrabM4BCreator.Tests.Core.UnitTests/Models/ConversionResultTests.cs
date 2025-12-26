using LibreGrabM4BCreator.Core.Models;

namespace LibreGrabM4BCreator.Tests.Core.UnitTests.Models;

public class ConversionResultTests
{
    [Fact]
    public void FileSizeFormatted_Bytes_ReturnsCorrectFormat()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            FileSizeBytes = 500
        };

        // Act
        var formatted = result.FileSizeFormatted;

        // Assert
        Assert.Equal("500 B", formatted);
    }

    [Fact]
    public void FileSizeFormatted_Kilobytes_ReturnsCorrectFormat()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            FileSizeBytes = 1024
        };

        // Act
        var formatted = result.FileSizeFormatted;

        // Assert
        Assert.Equal("1 KB", formatted);
    }

    [Fact]
    public void FileSizeFormatted_Megabytes_ReturnsCorrectFormat()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            FileSizeBytes = 1024 * 1024
        };

        // Act
        var formatted = result.FileSizeFormatted;

        // Assert
        Assert.Equal("1 MB", formatted);
    }

    [Fact]
    public void FileSizeFormatted_Gigabytes_ReturnsCorrectFormat()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            FileSizeBytes = 1024L * 1024 * 1024
        };

        // Act
        var formatted = result.FileSizeFormatted;

        // Assert
        Assert.Equal("1 GB", formatted);
    }

    [Fact]
    public void FileSizeFormatted_FractionalMegabytes_ReturnsCorrectFormat()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            FileSizeBytes = (long)(1.5 * 1024 * 1024)
        };

        // Act
        var formatted = result.FileSizeFormatted;

        // Assert
        Assert.Equal("1.5 MB", formatted);
    }

    [Fact]
    public void DurationFormatted_ShortDuration_ReturnsCorrectFormat()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            DurationSeconds = 65
        };

        // Act
        var formatted = result.DurationFormatted;

        // Assert
        Assert.Equal("00:01:05", formatted);
    }

    [Fact]
    public void DurationFormatted_HourDuration_ReturnsCorrectFormat()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            DurationSeconds = 3661 // 1h 1m 1s
        };

        // Act
        var formatted = result.DurationFormatted;

        // Assert
        Assert.Equal("01:01:01", formatted);
    }

    [Fact]
    public void DurationFormatted_ZeroDuration_ReturnsCorrectFormat()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            DurationSeconds = 0
        };

        // Act
        var formatted = result.DurationFormatted;

        // Assert
        Assert.Equal("00:00:00", formatted);
    }

    [Fact]
    public void DurationFormatted_LongDuration_ReturnsCorrectFormat()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            DurationSeconds = 36000 // 10 hours
        };

        // Act
        var formatted = result.DurationFormatted;

        // Assert
        Assert.Equal("10:00:00", formatted);
    }

    [Fact]
    public void Success_True_OutputFileCanBeSet()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        
        try
        {
            var result = new ConversionResult
            {
                Success = true,
                Title = "Test Book",
                OutputFile = new FileInfo(tempFile),
                DurationSeconds = 3600,
                FileSizeBytes = 1024 * 1024 * 50
            };

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.OutputFile);
            Assert.Equal("Test Book", result.Title);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Success_False_ErrorMessageCanBeSet()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = false,
            Title = "Failed Book",
            ErrorMessage = "Test error message"
        };

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Test error message", result.ErrorMessage);
        Assert.Null(result.OutputFile);
    }

    [Fact]
    public void TelegramExtradataValid_True_WarningMessageCanBeNull()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            TelegramExtradataValid = true
        };

        // Assert
        Assert.True(result.TelegramExtradataValid);
        Assert.Null(result.WarningMessage);
    }

    [Fact]
    public void TelegramExtradataValid_False_WarningMessageCanBeSet()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            TelegramExtradataValid = false,
            WarningMessage = "Telegram compatibility warning"
        };

        // Assert
        Assert.False(result.TelegramExtradataValid);
        Assert.Equal("Telegram compatibility warning", result.WarningMessage);
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(512, "512 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(2048, "2 KB")]
    [InlineData(1536, "1.5 KB")]
    public void FileSizeFormatted_VariousValues_ReturnsExpectedFormat(long bytes, string expected)
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            Title = "Test",
            FileSizeBytes = bytes
        };

        // Act & Assert
        Assert.Equal(expected, result.FileSizeFormatted);
    }
}
