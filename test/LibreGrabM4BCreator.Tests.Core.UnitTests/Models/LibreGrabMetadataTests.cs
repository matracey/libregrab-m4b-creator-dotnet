using LibreGrabM4BCreator.Core.Models;

namespace LibreGrabM4BCreator.Tests.Core.UnitTests.Models;

public class LibreGrabMetadataTests
{
    [Fact]
    public void GetAuthor_WithAuthorRole_ReturnsAuthorName()
    {
        // Arrange
        var metadata = new LibreGrabMetadata
        {
            Title = "Test Book",
            Creator =
            [
                new Creator { Name = "John Doe", Role = "author" },
                new Creator { Name = "Jane Smith", Role = "narrator" }
            ]
        };

        // Act
        var author = metadata.GetAuthor();

        // Assert
        Assert.Equal("John Doe", author);
    }

    [Fact]
    public void GetAuthor_WithAuthorRoleCaseInsensitive_ReturnsAuthorName()
    {
        // Arrange
        var metadata = new LibreGrabMetadata
        {
            Title = "Test Book",
            Creator =
            [
                new Creator { Name = "Jane Smith", Role = "narrator" },
                new Creator { Name = "John Doe", Role = "AUTHOR" }
            ]
        };

        // Act
        var author = metadata.GetAuthor();

        // Assert
        Assert.Equal("John Doe", author);
    }

    [Fact]
    public void GetAuthor_NoAuthorRole_ReturnsFirstCreator()
    {
        // Arrange
        var metadata = new LibreGrabMetadata
        {
            Title = "Test Book",
            Creator =
            [
                new Creator { Name = "First Creator", Role = "narrator" },
                new Creator { Name = "Second Creator", Role = "editor" }
            ]
        };

        // Act
        var author = metadata.GetAuthor();

        // Assert
        Assert.Equal("First Creator", author);
    }

    [Fact]
    public void GetAuthor_EmptyCreatorList_ReturnsNull()
    {
        // Arrange
        var metadata = new LibreGrabMetadata
        {
            Title = "Test Book",
            Creator = []
        };

        // Act
        var author = metadata.GetAuthor();

        // Assert
        Assert.Null(author);
    }

    [Fact]
    public void GetAuthor_NullCreatorList_ReturnsNull()
    {
        // Arrange
        var metadata = new LibreGrabMetadata
        {
            Title = "Test Book",
            Creator = null
        };

        // Act
        var author = metadata.GetAuthor();

        // Assert
        Assert.Null(author);
    }

    [Fact]
    public void GetAuthor_CreatorWithNullRole_ReturnsFirstCreator()
    {
        // Arrange
        var metadata = new LibreGrabMetadata
        {
            Title = "Test Book",
            Creator =
            [
                new Creator { Name = "Author Name", Role = null }
            ]
        };

        // Act
        var author = metadata.GetAuthor();

        // Assert
        Assert.Equal("Author Name", author);
    }

    [Fact]
    public void LibreGrabMetadata_AllPropertiesCanBeSet()
    {
        // Arrange
        var metadata = new LibreGrabMetadata
        {
            Title = "My Audiobook",
            Creator =
            [
                new Creator { Name = "Test Author", Role = "author" }
            ],
            Spine =
            [
                new SpineItem { Duration = 120.5 },
                new SpineItem { Duration = 180.0 }
            ],
            Chapters =
            [
                new ChapterInfo { Title = "Chapter 1", Spine = 0, Offset = 0 },
                new ChapterInfo { Title = "Chapter 2", Spine = 1, Offset = 30.5 }
            ]
        };

        // Assert
        Assert.Equal("My Audiobook", metadata.Title);
        Assert.NotNull(metadata.Creator);
        Assert.Single(metadata.Creator);
        Assert.NotNull(metadata.Spine);
        Assert.Equal(2, metadata.Spine.Count);
        Assert.NotNull(metadata.Chapters);
        Assert.Equal(2, metadata.Chapters.Count);
    }

    [Fact]
    public void SpineItem_Duration_CanBeSet()
    {
        // Arrange
        var spine = new SpineItem { Duration = 123.456 };

        // Assert
        Assert.Equal(123.456, spine.Duration);
    }

    [Fact]
    public void ChapterInfo_AllProperties_CanBeSet()
    {
        // Arrange
        var chapter = new ChapterInfo
        {
            Title = "Introduction",
            Spine = 2,
            Offset = 45.5
        };

        // Assert
        Assert.Equal("Introduction", chapter.Title);
        Assert.Equal(2, chapter.Spine);
        Assert.Equal(45.5, chapter.Offset);
    }

    [Fact]
    public void Creator_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var creator1 = new Creator { Name = "John", Role = "author" };
        var creator2 = new Creator { Name = "John", Role = "author" };

        // Assert
        Assert.Equal(creator1, creator2);
    }
}
