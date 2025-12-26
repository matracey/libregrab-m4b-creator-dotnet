using LibreGrabM4BCreator.Core.Models;
using LibreGrabM4BCreator.Core.Services;

namespace LibreGrabM4BCreator.Tests.Core.UnitTests.Services;

public class FFmpegServiceTests
{
    private readonly FFmpegService _ffmpegService;

    public FFmpegServiceTests()
    {
        _ffmpegService = new FFmpegService();
    }

    [Fact]
    public void CheckDependencies_ReturnsResult()
    {
        // Act
        var (available, errorMessage) = _ffmpegService.CheckDependencies();

        // Assert - We just verify it returns without throwing
        // The actual result depends on whether FFmpeg is installed
        if (available)
        {
            Assert.Null(errorMessage);
        }
        else
        {
            Assert.NotNull(errorMessage);
        }
    }

    [Fact]
    public async Task CreateConcatFileListAsync_EmptyList_CreatesEmptyFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-concat-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var files = Array.Empty<FileInfo>();

            // Act
            var result = await _ffmpegService.CreateConcatFileListAsync(files, tempDir);

            // Assert
            Assert.True(File.Exists(result));
            Assert.Equal("concat_list.txt", Path.GetFileName(result));
            
            var content = await File.ReadAllTextAsync(result);
            Assert.Empty(content.Trim());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task CreateConcatFileListAsync_MultipleFiles_CreatesValidFileList()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-concat-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var file1 = Path.Combine(tempDir, "audio1.mp3");
            var file2 = Path.Combine(tempDir, "audio2.mp3");
            await File.WriteAllTextAsync(file1, "fake");
            await File.WriteAllTextAsync(file2, "fake");

            var files = new[] { new FileInfo(file1), new FileInfo(file2) };

            // Act
            var result = await _ffmpegService.CreateConcatFileListAsync(files, tempDir);

            // Assert
            Assert.True(File.Exists(result));
            var content = await File.ReadAllTextAsync(result);
            
            Assert.Contains("file '", content);
            Assert.Contains("audio1.mp3", content);
            Assert.Contains("audio2.mp3", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task CreateConcatFileListAsync_FileWithQuotes_EscapesCorrectly()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-concat-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var fileName = "audio's.mp3";
            var filePath = Path.Combine(tempDir, fileName);
            await File.WriteAllTextAsync(filePath, "fake");

            var files = new[] { new FileInfo(filePath) };

            // Act
            var result = await _ffmpegService.CreateConcatFileListAsync(files, tempDir);

            // Assert
            var content = await File.ReadAllTextAsync(result);
            // Single quotes should be escaped for FFmpeg concat demuxer
            Assert.Contains(@"'\''", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task CreateChapterMetadataFileAsync_EmptyChapters_CreatesValidMetadataFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-chapters-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var chapters = Array.Empty<Chapter>();

            // Act
            var result = await _ffmpegService.CreateChapterMetadataFileAsync(
                chapters, "Test Title", "Test Author", tempDir);

            // Assert
            Assert.True(File.Exists(result));
            Assert.Equal("chapters.txt", Path.GetFileName(result));
            
            var content = await File.ReadAllTextAsync(result);
            Assert.Contains(";FFMETADATA1", content);
            Assert.Contains("title=Test Title", content);
            Assert.Contains("artist=Test Author", content);
            Assert.Contains("genre=Audiobook", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task CreateChapterMetadataFileAsync_WithChapters_CreatesValidChapterEntries()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-chapters-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var chapters = new List<Chapter>
            {
                new Chapter { Title = "Chapter 1", StartSeconds = 0, EndSeconds = 100 },
                new Chapter { Title = "Chapter 2", StartSeconds = 100, EndSeconds = 200 }
            };

            // Act
            var result = await _ffmpegService.CreateChapterMetadataFileAsync(
                chapters, "Test", "Author", tempDir);

            // Assert
            var content = await File.ReadAllTextAsync(result);
            
            Assert.Contains("[CHAPTER]", content);
            Assert.Contains("TIMEBASE=1/1", content);
            Assert.Contains("START=0", content);
            Assert.Contains("END=100", content);
            Assert.Contains("title=Chapter 1", content);
            Assert.Contains("START=100", content);
            Assert.Contains("END=200", content);
            Assert.Contains("title=Chapter 2", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task CreateChapterMetadataFileAsync_NullTitle_OmitsTitleAndAlbum()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-chapters-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var chapters = Array.Empty<Chapter>();

            // Act
            var result = await _ffmpegService.CreateChapterMetadataFileAsync(
                chapters, null, "Test Author", tempDir);

            // Assert
            var content = await File.ReadAllTextAsync(result);
            
            Assert.DoesNotContain("title=", content.Split("genre=")[0]); // No title before genre
            Assert.DoesNotContain("album=", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task CreateChapterMetadataFileAsync_NullAuthor_OmitsArtist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-chapters-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var chapters = Array.Empty<Chapter>();

            // Act
            var result = await _ffmpegService.CreateChapterMetadataFileAsync(
                chapters, "Test Title", null, tempDir);

            // Assert
            var content = await File.ReadAllTextAsync(result);
            
            Assert.DoesNotContain("artist=", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task CreateChapterMetadataFileAsync_SpecialCharacters_EscapesCorrectly()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-chapters-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var chapters = new List<Chapter>
            {
                new Chapter { Title = "Chapter=1;Test#Special", StartSeconds = 0, EndSeconds = 100 }
            };

            // Act
            var result = await _ffmpegService.CreateChapterMetadataFileAsync(
                chapters, "Title=Test", "Author;Name", tempDir);

            // Assert
            var content = await File.ReadAllTextAsync(result);
            
            // Verify special characters are escaped
            Assert.Contains(@"title=Title\=Test", content);
            Assert.Contains(@"artist=Author\;Name", content);
            Assert.Contains(@"title=Chapter\=1\;Test\#Special", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task GetBestAacEncoderAsync_ReturnsCachedValue()
    {
        // Arrange & Act
        var encoder1 = await _ffmpegService.GetBestAacEncoderAsync();
        var encoder2 = await _ffmpegService.GetBestAacEncoderAsync();

        // Assert
        Assert.Equal(encoder1, encoder2);
        Assert.NotNull(encoder1);
    }

    [Fact]
    public async Task GetBestAacEncoderAsync_ReturnsValidEncoder()
    {
        // Act
        var encoder = await _ffmpegService.GetBestAacEncoderAsync();

        // Assert
        Assert.NotNull(encoder);
        // Should be one of the known encoders
        Assert.Contains(encoder, new[] { "aac_at", "libfdk_aac", "aac" });
    }

    [Fact]
    public async Task CreateChapterMetadataFileAsync_ChapterWithFractionalSeconds_UsesFlooredValues()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-chapters-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var chapters = new List<Chapter>
            {
                new Chapter { Title = "Chapter 1", StartSeconds = 10.75, EndSeconds = 20.99 }
            };

            // Act
            var result = await _ffmpegService.CreateChapterMetadataFileAsync(
                chapters, "Test", "Author", tempDir);

            // Assert
            var content = await File.ReadAllTextAsync(result);
            
            // The Chapter record's StartSecondsInt and EndSecondsInt properties floor the values
            Assert.Contains("START=10", content);
            Assert.Contains("END=20", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
