# LibreGrab M4B Creator

A cross-platform .NET command-line tool that converts directories of MP3 files into M4B audiobook files with chapter markers. Designed primarily for use with audiobooks downloaded via [LibreGrab](https://greasyfork.org/en/scripts/498782-libregrab).

## Features

- **Batch Conversion** - Process multiple audiobook directories in a single command
- **Automatic Chapter Detection** - Reads LibreGrab metadata to create chapter markers
- **Smart Encoder Selection** - Automatically uses the best available AAC encoder:
  - `aac_at` (Apple AudioToolbox) on macOS
  - `libfdk_aac` if available
  - Native `aac` as fallback
- **Telegram Mode** - Optional encoding settings optimized for Telegram compatibility
- **Progress Tracking** - Real-time progress display with estimated time remaining
- **Cross-Platform** - Runs on Windows, macOS, and Linux

## Requirements

- [.NET 8.0](https://dotnet.microsoft.com/download), [.NET 9.0](https://dotnet.microsoft.com/download), or [.NET 10.0](https://dotnet.microsoft.com/download) Runtime
- [FFmpeg](https://ffmpeg.org/) installed and available in your PATH

### Installing FFmpeg

**macOS:**

```bash
brew install ffmpeg
```

**Linux (Debian/Ubuntu):**

```bash
sudo apt install ffmpeg
```

**Windows:**

```powershell
winget install ffmpeg
```

## Installation

### From Source

```bash
git clone https://github.com/matracey/libregrab-m4b-creator-dotnet.git
cd libregrab-m4b-creator-dotnet
dotnet build
```

## Usage

```bash
# Basic usage - convert a single directory
dotnet run --project src/LibreGrabM4BCreator.Console -- /path/to/audiobook

# Convert multiple directories
dotnet run --project src/LibreGrabM4BCreator.Console -- /path/to/book1 /path/to/book2

# Specify output directory
dotnet run --project src/LibreGrabM4BCreator.Console -- --output-dir /path/to/output /path/to/audiobook

# Enable Telegram-compatible encoding
dotnet run --project src/LibreGrabM4BCreator.Console -- --telegram /path/to/audiobook
```

### Command-Line Options

| Option                | Description                                                    |
| --------------------- | -------------------------------------------------------------- |
| `--output-dir <path>` | Output directory for M4B files (defaults to current directory) |
| `--telegram`          | Use Telegram-compatible encoding settings                      |

### Input Directory Structure

The tool expects audiobook directories with the following structure (as created by LibreGrab):

```text
AudiobookTitle/
├── metadata.json       # LibreGrab metadata file (optional)
├── 01 - Chapter One.mp3
├── 02 - Chapter Two.mp3
└── ...
```

If `metadata.json` is present, chapter titles and audiobook metadata will be extracted from it. Otherwise, chapter names are derived from MP3 filenames.

## Output

The tool creates M4B files with:

- **Embedded Chapters** - Navigate between chapters in your audiobook player
- **Metadata** - Title, author, and genre tags
- **Optimized Encoding** - Fast-start flag for streaming compatibility

## Telegram Mode

When using `--telegram`, the tool applies special encoding settings to ensure compatibility with Telegram's audio player:

- AAC-LC profile
- Fast-start container flags
- Specific extradata formatting

## Building

```bash
# Build for all target frameworks
dotnet build

# Build for a specific framework
dotnet build -f net8.0

# Create a self-contained executable
dotnet publish -c Release -r osx-arm64 --self-contained
```

## License

This project is open source. See the LICENSE file for details.

## Acknowledgments

- [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore) - FFmpeg .NET wrapper
- [Spectre.Console](https://spectreconsole.net/) - Beautiful console output
- [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) - Zero-dependency CLI framework
- [LibreGrab](https://greasyfork.org/en/scripts/498782-libregrab) - Audiobook downloader
