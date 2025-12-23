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

## License

This project is open source. See the LICENSE file for details.
