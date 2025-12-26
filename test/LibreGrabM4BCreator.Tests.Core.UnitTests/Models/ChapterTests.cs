using LibreGrabM4BCreator.Core.Models;

namespace LibreGrabM4BCreator.Tests.Core.UnitTests.Models;

public class ChapterTests
{
    [Fact]
    public void StartSecondsInt_ReturnsFlooredValue()
    {
        // Arrange
        var chapter = new Chapter
        {
            Title = "Test Chapter",
            StartSeconds = 10.75,
            EndSeconds = 20.25
        };

        // Act
        var result = chapter.StartSecondsInt;

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void EndSecondsInt_ReturnsFlooredValue()
    {
        // Arrange
        var chapter = new Chapter
        {
            Title = "Test Chapter",
            StartSeconds = 10.75,
            EndSeconds = 20.99
        };

        // Act
        var result = chapter.EndSecondsInt;

        // Assert
        Assert.Equal(20, result);
    }

    [Fact]
    public void StartSecondsInt_WithExactInteger_ReturnsExactValue()
    {
        // Arrange
        var chapter = new Chapter
        {
            Title = "Test Chapter",
            StartSeconds = 15.0,
            EndSeconds = 30.0
        };

        // Act
        var result = chapter.StartSecondsInt;

        // Assert
        Assert.Equal(15, result);
    }

    [Fact]
    public void EndSecondsInt_WithExactInteger_ReturnsExactValue()
    {
        // Arrange
        var chapter = new Chapter
        {
            Title = "Test Chapter",
            StartSeconds = 15.0,
            EndSeconds = 30.0
        };

        // Act
        var result = chapter.EndSecondsInt;

        // Assert
        Assert.Equal(30, result);
    }

    [Fact]
    public void Chapter_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var chapter1 = new Chapter
        {
            Title = "Test Chapter",
            StartSeconds = 10.5,
            EndSeconds = 20.5
        };

        var chapter2 = new Chapter
        {
            Title = "Test Chapter",
            StartSeconds = 10.5,
            EndSeconds = 20.5
        };

        // Act & Assert
        Assert.Equal(chapter1, chapter2);
    }

    [Fact]
    public void Chapter_RecordInequality_WorksCorrectly()
    {
        // Arrange
        var chapter1 = new Chapter
        {
            Title = "Chapter 1",
            StartSeconds = 0,
            EndSeconds = 10
        };

        var chapter2 = new Chapter
        {
            Title = "Chapter 2",
            StartSeconds = 10,
            EndSeconds = 20
        };

        // Act & Assert
        Assert.NotEqual(chapter1, chapter2);
    }

    [Theory]
    [InlineData(0.0, 0)]
    [InlineData(0.1, 0)]
    [InlineData(0.9, 0)]
    [InlineData(1.0, 1)]
    [InlineData(999.999, 999)]
    public void StartSecondsInt_VariousValues_ReturnsCorrectFloor(double input, long expected)
    {
        // Arrange
        var chapter = new Chapter
        {
            Title = "Test",
            StartSeconds = input,
            EndSeconds = input + 1
        };

        // Act & Assert
        Assert.Equal(expected, chapter.StartSecondsInt);
    }
}
