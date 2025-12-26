using LibreGrabM4BCreator.Core.Abstractions;
using LibreGrabM4BCreator.Core.Models;
using LibreGrabM4BCreator.Core.Services;

namespace LibreGrabM4BCreator.Tests.Core.UnitTests.Services;

public class ConversionServiceTests
{
    private readonly IProgressReporter _mockProgressReporter;
    private readonly IUserInterface _mockUserInterface;

    public ConversionServiceTests()
    {
        _mockProgressReporter = Substitute.For<IProgressReporter>();
        _mockUserInterface = Substitute.For<IUserInterface>();
    }

    [Fact]
    public async Task ConvertAsync_NonExistentDirectory_ReturnsFailureResult()
    {
        // Arrange
        var ffmpegService = new FFmpegService();
        var discoveryService = new AudiobookDiscoveryService(ffmpegService);
        var conversionService = new ConversionService(
            ffmpegService, 
            discoveryService, 
            _mockProgressReporter, 
            _mockUserInterface);

        var nonExistentDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        var options = new ConversionOptions
        {
            OutputDirectory = new DirectoryInfo(Path.GetTempPath())
        };

        // Act
        var result = await conversionService.ConvertAsync(nonExistentDir, options);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(nonExistentDir.Name, result.Title);
        Assert.Contains("No MP3 files found", result.ErrorMessage);
    }

    [Fact]
    public async Task ConvertAsync_EmptyDirectory_ReturnsFailureResult()
    {
        // Arrange
        var ffmpegService = new FFmpegService();
        var discoveryService = new AudiobookDiscoveryService(ffmpegService);
        var conversionService = new ConversionService(
            ffmpegService, 
            discoveryService, 
            _mockProgressReporter, 
            _mockUserInterface);

        var tempDir = Path.Combine(Path.GetTempPath(), $"test-empty-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var emptyDir = new DirectoryInfo(tempDir);
            var options = new ConversionOptions
            {
                OutputDirectory = new DirectoryInfo(Path.GetTempPath())
            };

            // Act
            var result = await conversionService.ConvertAsync(emptyDir, options);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("No MP3 files found", result.ErrorMessage);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ConvertBatchAsync_EmptyList_ReturnsEmptyResults()
    {
        // Arrange
        var ffmpegService = new FFmpegService();
        var discoveryService = new AudiobookDiscoveryService(ffmpegService);
        var conversionService = new ConversionService(
            ffmpegService, 
            discoveryService, 
            _mockProgressReporter, 
            _mockUserInterface);

        var options = new ConversionOptions
        {
            OutputDirectory = new DirectoryInfo(Path.GetTempPath())
        };

        // Act
        var results = await conversionService.ConvertBatchAsync(
            Enumerable.Empty<DirectoryInfo>(), 
            options);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task ConvertBatchAsync_WithCancellation_StopsProcessing()
    {
        // Arrange
        var ffmpegService = new FFmpegService();
        var discoveryService = new AudiobookDiscoveryService(ffmpegService);
        var conversionService = new ConversionService(
            ffmpegService, 
            discoveryService, 
            _mockProgressReporter, 
            _mockUserInterface);

        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var tempDir = Path.Combine(Path.GetTempPath(), $"test-cancel-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            var dirs = new[] { new DirectoryInfo(tempDir) };
            var options = new ConversionOptions
            {
                OutputDirectory = new DirectoryInfo(Path.GetTempPath())
            };

            // Act
            var results = await conversionService.ConvertBatchAsync(dirs, options, cts.Token);

            // Assert
            Assert.Empty(results);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ConvertBatchAsync_MultipleDirectories_ProcessesAll()
    {
        // Arrange
        var ffmpegService = new FFmpegService();
        var discoveryService = new AudiobookDiscoveryService(ffmpegService);
        var conversionService = new ConversionService(
            ffmpegService, 
            discoveryService, 
            _mockProgressReporter, 
            _mockUserInterface);

        var tempDirs = new List<string>();
        
        try
        {
            for (int i = 0; i < 3; i++)
            {
                var tempDir = Path.Combine(Path.GetTempPath(), $"test-batch-{Guid.NewGuid():N}");
                Directory.CreateDirectory(tempDir);
                tempDirs.Add(tempDir);
            }

            var dirs = tempDirs.Select(d => new DirectoryInfo(d)).ToList();
            var options = new ConversionOptions
            {
                OutputDirectory = new DirectoryInfo(Path.GetTempPath())
            };

            // Act
            var results = await conversionService.ConvertBatchAsync(dirs, options);

            // Assert
            Assert.Equal(3, results.Count);
            Assert.All(results, r => Assert.False(r.Success)); // All fail because empty
            
            // Verify UI methods were called for each directory
            _mockUserInterface.Received(3).DisplayProcessingStatus(Arg.Any<string>());
        }
        finally
        {
            foreach (var dir in tempDirs)
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }
    }

    [Fact]
    public async Task ConvertAsync_DirectoryWithOnlyNonMp3Files_ReturnsFailure()
    {
        // Arrange
        var ffmpegService = new FFmpegService();
        var discoveryService = new AudiobookDiscoveryService(ffmpegService);
        var conversionService = new ConversionService(
            ffmpegService, 
            discoveryService, 
            _mockProgressReporter, 
            _mockUserInterface);

        var tempDir = Path.Combine(Path.GetTempPath(), $"test-non-mp3-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        
        try
        {
            // Create some non-MP3 files
            await File.WriteAllTextAsync(Path.Combine(tempDir, "file1.txt"), "test");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "file2.wav"), "test");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "file3.flac"), "test");

            var dir = new DirectoryInfo(tempDir);
            var options = new ConversionOptions
            {
                OutputDirectory = new DirectoryInfo(Path.GetTempPath())
            };

            // Act
            var result = await conversionService.ConvertAsync(dir, options);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("No MP3 files found", result.ErrorMessage);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
