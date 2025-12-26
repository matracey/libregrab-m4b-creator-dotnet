using LibreGrabM4BCreator.Core.Models;

namespace LibreGrabM4BCreator.Tests.Core.UnitTests.Models;

public class AudiobookInfoTests
{
    [Fact]
    public void AudiobookInfo_AllPropertiesCanBeSet()
    {
        // Arrange
        var tempDir = new DirectoryInfo(Path.GetTempPath());
        var tempFile = Path.GetTempFileName();
        
        try
        {
            var audioFiles = new List<FileInfo> { new FileInfo(tempFile) };
            var chapters = new List<Chapter>
            {
                new Chapter { Title = "Chapter 1", StartSeconds = 0, EndSeconds = 100 }
            };

            var audiobook = new AudiobookInfo
            {
                SourceDirectory = tempDir,
                Title = "Test Audiobook",
                Author = "Test Author",
                AudioFiles = audioFiles,
                Chapters = chapters,
                TotalDurationSeconds = 3600,
                SourceBitrateKbps = 128,
                SourceChannels = 2
            };

            // Assert
            Assert.Equal(tempDir, audiobook.SourceDirectory);
            Assert.Equal("Test Audiobook", audiobook.Title);
            Assert.Equal("Test Author", audiobook.Author);
            Assert.Single(audiobook.AudioFiles);
            Assert.Single(audiobook.Chapters);
            Assert.Equal(3600, audiobook.TotalDurationSeconds);
            Assert.Equal(128, audiobook.SourceBitrateKbps);
            Assert.Equal(2, audiobook.SourceChannels);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void AudiobookInfo_AuthorCanBeNull()
    {
        // Arrange
        var tempDir = new DirectoryInfo(Path.GetTempPath());
        
        var audiobook = new AudiobookInfo
        {
            SourceDirectory = tempDir,
            Title = "Test Audiobook",
            Author = null,
            AudioFiles = [],
            Chapters = [],
            TotalDurationSeconds = 0,
            SourceBitrateKbps = 128,
            SourceChannels = 2
        };

        // Assert
        Assert.Null(audiobook.Author);
    }

    [Fact]
    public void AudiobookInfo_MultipleAudioFiles_ArePreserved()
    {
        // Arrange
        var tempDir = new DirectoryInfo(Path.GetTempPath());
        var tempFiles = new List<string>();
        var audioFiles = new List<FileInfo>();

        try
        {
            for (int i = 0; i < 5; i++)
            {
                var tempFile = Path.GetTempFileName();
                tempFiles.Add(tempFile);
                audioFiles.Add(new FileInfo(tempFile));
            }

            var audiobook = new AudiobookInfo
            {
                SourceDirectory = tempDir,
                Title = "Multi-file Audiobook",
                AudioFiles = audioFiles,
                Chapters = [],
                TotalDurationSeconds = 18000,
                SourceBitrateKbps = 192,
                SourceChannels = 1
            };

            // Assert
            Assert.Equal(5, audiobook.AudioFiles.Count);
        }
        finally
        {
            foreach (var file in tempFiles)
            {
                File.Delete(file);
            }
        }
    }

    [Fact]
    public void AudiobookInfo_MultipleChapters_ArePreserved()
    {
        // Arrange
        var tempDir = new DirectoryInfo(Path.GetTempPath());
        var chapters = new List<Chapter>
        {
            new Chapter { Title = "Chapter 1", StartSeconds = 0, EndSeconds = 600 },
            new Chapter { Title = "Chapter 2", StartSeconds = 600, EndSeconds = 1200 },
            new Chapter { Title = "Chapter 3", StartSeconds = 1200, EndSeconds = 1800 }
        };

        var audiobook = new AudiobookInfo
        {
            SourceDirectory = tempDir,
            Title = "Multi-chapter Book",
            AudioFiles = [],
            Chapters = chapters,
            TotalDurationSeconds = 1800,
            SourceBitrateKbps = 128,
            SourceChannels = 2
        };

        // Assert
        Assert.Equal(3, audiobook.Chapters.Count);
        Assert.Equal("Chapter 1", audiobook.Chapters[0].Title);
        Assert.Equal("Chapter 2", audiobook.Chapters[1].Title);
        Assert.Equal("Chapter 3", audiobook.Chapters[2].Title);
    }

    [Theory]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(192)]
    [InlineData(256)]
    [InlineData(320)]
    public void AudiobookInfo_VariousBitrates_AreAccepted(int bitrate)
    {
        // Arrange
        var tempDir = new DirectoryInfo(Path.GetTempPath());
        
        var audiobook = new AudiobookInfo
        {
            SourceDirectory = tempDir,
            Title = "Test",
            AudioFiles = [],
            Chapters = [],
            TotalDurationSeconds = 100,
            SourceBitrateKbps = bitrate,
            SourceChannels = 2
        };

        // Assert
        Assert.Equal(bitrate, audiobook.SourceBitrateKbps);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(6)]
    public void AudiobookInfo_VariousChannels_AreAccepted(int channels)
    {
        // Arrange
        var tempDir = new DirectoryInfo(Path.GetTempPath());
        
        var audiobook = new AudiobookInfo
        {
            SourceDirectory = tempDir,
            Title = "Test",
            AudioFiles = [],
            Chapters = [],
            TotalDurationSeconds = 100,
            SourceBitrateKbps = 128,
            SourceChannels = channels
        };

        // Assert
        Assert.Equal(channels, audiobook.SourceChannels);
    }

    [Fact]
    public void AudiobookInfo_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var tempDir = new DirectoryInfo(Path.GetTempPath());
        var audioFiles = new List<FileInfo>();
        var chapters = new List<Chapter>();

        var audiobook1 = new AudiobookInfo
        {
            SourceDirectory = tempDir,
            Title = "Test",
            Author = "Author",
            AudioFiles = audioFiles,
            Chapters = chapters,
            TotalDurationSeconds = 100,
            SourceBitrateKbps = 128,
            SourceChannels = 2
        };

        var audiobook2 = new AudiobookInfo
        {
            SourceDirectory = tempDir,
            Title = "Test",
            Author = "Author",
            AudioFiles = audioFiles,
            Chapters = chapters,
            TotalDurationSeconds = 100,
            SourceBitrateKbps = 128,
            SourceChannels = 2
        };

        // Assert
        Assert.Equal(audiobook1, audiobook2);
    }
}
