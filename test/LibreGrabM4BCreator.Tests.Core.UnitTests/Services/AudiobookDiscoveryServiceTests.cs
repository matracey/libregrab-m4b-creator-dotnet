using LibreGrabM4BCreator.Core.Services;

namespace LibreGrabM4BCreator.Tests.Core.UnitTests.Services;

public class AudiobookDiscoveryServiceTests
{
    private readonly FFmpegService _ffmpegService;
    private readonly AudiobookDiscoveryService _discoveryService;

    public AudiobookDiscoveryServiceTests()
    {
        _ffmpegService = new FFmpegService();
        _discoveryService = new AudiobookDiscoveryService(_ffmpegService);
    }

    [Fact]
    public async Task DiscoverAsync_NonExistentDirectory_ReturnsNull()
    {
        // Arrange
        var nonExistentDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

        // Act
        var result = await _discoveryService.DiscoverAsync(nonExistentDir);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DiscoverAsync_EmptyDirectory_ReturnsNull()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-empty-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var dir = new DirectoryInfo(tempDir);

            // Act
            var result = await _discoveryService.DiscoverAsync(dir);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task DiscoverAsync_DirectoryWithNoMp3Files_ReturnsNull()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-no-mp3-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            // Create non-MP3 files
            await File.WriteAllTextAsync(Path.Combine(tempDir, "file1.txt"), "test");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "file2.wav"), "test");
            
            var dir = new DirectoryInfo(tempDir);

            // Act
            var result = await _discoveryService.DiscoverAsync(dir);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task DiscoverAsync_DirectoryWithMp3FilesInSubdirectory_ReturnsNull()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-subdir-{Guid.NewGuid():N}");
        var subDir = Path.Combine(tempDir, "subfolder");
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(subDir);
        
        try
        {
            // Create MP3 file only in subdirectory (not root)
            await File.WriteAllTextAsync(Path.Combine(subDir, "audio.mp3"), "fake mp3");
            
            var dir = new DirectoryInfo(tempDir);

            // Act
            var result = await _discoveryService.DiscoverAsync(dir);

            // Assert - Should return null because we only look in root
            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
