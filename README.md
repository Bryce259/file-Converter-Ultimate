# File Converter Ultimate

File Converter Ultimate is an accessible Windows desktop app for converting audio, video, image, text, and document files. If you don't want to spend time on the internet on file conversion sites, getting redirected to potentially harmful websites, and you want great file conversion options, File Converter Ultimate is for you. It supports old files like voc and kwb conversions.

## What it does

- Shows a list of conversion choices as soon as the app opens.
- Lets you convert a single file or a whole ZIP archive of files.
- Reveals background options and text overlay only when you choose an audio-to-MP4 conversion.
- Tries to stay screen-reader friendly with labeled controls, simple tab order, and plain status updates.

## Notes

- Most audio and video conversions require `ffmpeg.exe`.
- HEIC image conversion requires `magick.exe` from ImageMagick.
- DOC to TXT or PDF works best when Microsoft Word is installed.
- DOCX can still be turned into plain text or a text-based PDF without Word.
- `KWB`, `BRF`, `TXT`, and `MD` conversions are handled as plain-text conversions in this version.

## Bundled helper tools

This build bundles:

- `ffmpeg.exe`
- `ffprobe.exe`
- ImageMagick portable files in `Tools\ImageMagick`

The app will use these bundled tools first, then fall back to system-wide installs if needed.

## Output

Converted files are saved into the output folder you choose in the app (e.g. "C:\Users\you're name \Documents\File Converter Ultimate Output"). It saves in documents by default.
