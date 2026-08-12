using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileConverterUltimateApp
{
    internal sealed class ConversionService
    {
        public List<ConversionResult> Convert(ConversionRequest request, Action<string> log)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Directory.CreateDirectory(request.OutputDirectory);

            if (request.IsZipBatch)
            {
                return ConvertZipEntries(request, log);
            }

            return new List<ConversionResult>
            {
                ConvertInput(request.InputPath, request.InputPath, BuildOutputPath(request.InputPath, request), request, log)
            };
        }

        public static List<ConversionOption> GetOptions()
        {
            return new List<ConversionOption>
            {
                new ConversionOption("mp3-m4a", "Audio: MP3 to M4A", "Audio", new[] { ".mp3" }, ".m4a"),
                new ConversionOption("mp3-mp4", "Audio Video: MP3 to MP4 with optional background", "Audio", new[] { ".mp3" }, ".mp4", true),
                new ConversionOption("wav-mp4", "Audio Video: WAV to MP4 with optional background", "Audio", new[] { ".wav" }, ".mp4", true),
                new ConversionOption("m4a-mp4", "Audio Video: M4A to MP4 with optional background", "Audio", new[] { ".m4a" }, ".mp4", true),
                new ConversionOption("voc-mp3", "Audio: VOC or VOQ to MP3", "Audio", new[] { ".voc", ".voq" }, ".mp3"),
                new ConversionOption("voc-m4a", "Audio: VOC or VOQ to M4A", "Audio", new[] { ".voc", ".voq" }, ".m4a"),
                new ConversionOption("m4a-wav", "Audio: M4A to WAV", "Audio", new[] { ".m4a" }, ".wav"),
                new ConversionOption("m4p-mp3", "Audio: M4P to MP3", "Audio", new[] { ".m4p" }, ".mp3"),
                new ConversionOption("m4p-m4a", "Audio: M4P to M4A", "Audio", new[] { ".m4p" }, ".m4a"),
                new ConversionOption("m4p-wav", "Audio: M4P to WAV", "Audio", new[] { ".m4p" }, ".wav"),
                new ConversionOption("wav-mp3", "Audio: WAV to MP3", "Audio", new[] { ".wav" }, ".mp3"),
                new ConversionOption("aiff-mp3", "Audio: AIFF to MP3", "Audio", new[] { ".aiff", ".aif" }, ".mp3"),
                new ConversionOption("aiff-m4a", "Audio: AIFF to M4A", "Audio", new[] { ".aiff", ".aif" }, ".m4a"),
                new ConversionOption("aiff-wav", "Audio: AIFF to WAV", "Audio", new[] { ".aiff", ".aif" }, ".wav"),
                new ConversionOption("flac-mp3", "Audio: FLAC to MP3", "Audio", new[] { ".flac" }, ".mp3"),
                new ConversionOption("flac-m4a", "Audio: FLAC to M4A", "Audio", new[] { ".flac" }, ".m4a"),
                new ConversionOption("wma-mp3", "Audio: WMA to MP3", "Audio", new[] { ".wma" }, ".mp3"),
                new ConversionOption("ogg-mp3", "Audio: OGG to MP3", "Audio", new[] { ".ogg" }, ".mp3"),
                new ConversionOption("ogg-m4a", "Audio: OGG to M4A", "Audio", new[] { ".ogg" }, ".m4a"),
                new ConversionOption("ogg-wav", "Audio: OGG to WAV", "Audio", new[] { ".ogg" }, ".wav"),
                new ConversionOption("mp4-mp3", "Audio: MP4 to MP3", "Audio", new[] { ".mp4" }, ".mp3"),
                new ConversionOption("mp4-mov", "Video: MP4 to MOV", "Video", new[] { ".mp4" }, ".mov"),
                new ConversionOption("mov-mp4", "Video: MOV to MP4", "Video", new[] { ".mov" }, ".mp4"),
                new ConversionOption("mp4-m4v", "Video: MP4 to M4V", "Video", new[] { ".mp4" }, ".m4v"),
                new ConversionOption("anyvideo-mp4", "Video: Any supported video to MP4", "Video", new[] { ".avi", ".mkv", ".wmv", ".mov", ".webm", ".flv", ".mpeg", ".mpg", ".3gp", ".m4v", ".mts", ".m2ts" }, ".mp4"),
                new ConversionOption("png-jpg", "Image: PNG to JPG", "Image", new[] { ".png" }, ".jpg"),
                new ConversionOption("bmp-jpg", "Image: BMP to JPG", "Image", new[] { ".bmp" }, ".jpg"),
                new ConversionOption("heic-png", "Image: HEIC to PNG", "Image", new[] { ".heic" }, ".png"),
                new ConversionOption("heic-jpg", "Image: HEIC to JPG", "Image", new[] { ".heic" }, ".jpg"),
                new ConversionOption("doc-txt", "Document: DOC or DOCX to TXT", "Documents", new[] { ".doc", ".docx" }, ".txt"),
                new ConversionOption("doc-pdf", "Document: DOC or DOCX to PDF", "Documents", new[] { ".doc", ".docx" }, ".pdf"),
                new ConversionOption("txt-pdf", "Document: TXT to PDF", "Documents", new[] { ".txt" }, ".pdf"),
                new ConversionOption("md-txt", "Text: Markdown to TXT", "Documents", new[] { ".md" }, ".txt", false, true),
                new ConversionOption("txt-md", "Text: TXT to Markdown", "Documents", new[] { ".txt" }, ".md", false, true),
                new ConversionOption("kwb-txt", "Text: KWB to TXT", "Documents", new[] { ".kwb" }, ".txt", false, true),
                new ConversionOption("kwt-txt", "Text: KWT to TXT", "Documents", new[] { ".kwt" }, ".txt", false, true),
                new ConversionOption("brf-txt", "Braille: BRF to TXT", "Documents", new[] { ".brf" }, ".txt", false, true),
                new ConversionOption("doc-brf", "Braille: DOC or DOCX to BRF", "Documents", new[] { ".doc", ".docx" }, ".brf", false, true)
            };
        }

        private static List<ConversionResult> ConvertZipEntries(ConversionRequest request, Action<string> log)
        {
            string tempFolder = Path.Combine(Path.GetTempPath(), "FileConverterUltimate", Guid.NewGuid().ToString("N"));
            List<ConversionResult> results = new List<ConversionResult>();
            Directory.CreateDirectory(tempFolder);

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(request.InputPath))
                {
                    List<ZipArchiveEntry> fileEntries = archive.Entries
                        .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
                        .ToList();

                    log("ZIP archive opened. Processing " + fileEntries.Count + " file" + (fileEntries.Count == 1 ? "." : "s one at a time."));

                    for (int index = 0; index < fileEntries.Count; index++)
                    {
                        ZipArchiveEntry entry = fileEntries[index];
                        string entryName = entry.FullName;
                        string extension = Path.GetExtension(entry.Name).ToLowerInvariant();

                        if (!request.Option.InputExtensions.Contains(extension))
                        {
                            results.Add(CreateSkippedResult(entryName, extension, request));
                            continue;
                        }

                        string tempInputPath = Path.Combine(tempFolder, Guid.NewGuid().ToString("N") + extension);
                        try
                        {
                            log("Extracting " + (index + 1) + " of " + fileEntries.Count + ": " + entry.Name + ".");
                            using (Stream input = entry.Open())
                            using (FileStream output = new FileStream(tempInputPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1048576, FileOptions.SequentialScan))
                            {
                                input.CopyTo(output, 1048576);
                            }

                            string outputPath = BuildZipOutputPath(entryName, request);
                            results.Add(ConvertInput(tempInputPath, entryName, outputPath, request, log));
                        }
                        catch (Exception ex)
                        {
                            results.Add(new ConversionResult
                            {
                                InputFile = entryName,
                                Success = false,
                                Message = ex.Message
                            });
                        }
                        finally
                        {
                            if (File.Exists(tempInputPath))
                            {
                                File.Delete(tempInputPath);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }

            return results;
        }

        private static ConversionResult ConvertInput(string inputPath, string displayName, string outputPath, ConversionRequest request, Action<string> log)
        {
            string extension = Path.GetExtension(inputPath).ToLowerInvariant();
            if (!request.Option.InputExtensions.Contains(extension))
            {
                return CreateSkippedResult(displayName, extension, request);
            }

            try
            {
                log("Converting " + Path.GetFileName(displayName) + " to " + request.Option.OutputExtension + ".");
                ConvertSingle(inputPath, outputPath, request, log);
                return new ConversionResult
                {
                    InputFile = displayName,
                    OutputFile = outputPath,
                    Success = true,
                    Message = "Converted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ConversionResult
                {
                    InputFile = displayName,
                    OutputFile = outputPath,
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private static ConversionResult CreateSkippedResult(string inputFile, string extension, ConversionRequest request)
        {
            return new ConversionResult
            {
                InputFile = inputFile,
                Success = false,
                Message = "Skipped because " + (string.IsNullOrWhiteSpace(extension) ? "the file has no extension" : extension + " is not supported by the selected conversion") + "."
            };
        }

        private static string BuildOutputPath(string inputPath, ConversionRequest request)
        {
            Directory.CreateDirectory(request.OutputDirectory);
            return Path.Combine(request.OutputDirectory, Path.GetFileNameWithoutExtension(inputPath) + request.Option.OutputExtension);
        }

        private static string BuildZipOutputPath(string entryName, ConversionRequest request)
        {
            string rootDirectory = Path.GetFullPath(request.OutputDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string normalizedEntryPath = entryName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            string relativeDirectory = Path.GetDirectoryName(normalizedEntryPath) ?? string.Empty;
            string outputDirectory = Path.GetFullPath(Path.Combine(rootDirectory, relativeDirectory));

            if (!string.Equals(outputDirectory, rootDirectory, StringComparison.OrdinalIgnoreCase) &&
                !outputDirectory.StartsWith(rootDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The ZIP archive contains an unsafe file path: " + entryName);
            }

            Directory.CreateDirectory(outputDirectory);
            return Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(normalizedEntryPath) + request.Option.OutputExtension);
        }

        private static void ConvertSingle(string inputPath, string outputPath, ConversionRequest request, Action<string> log)
        {
            switch (request.Option.Id)
            {
                case "png-jpg":
                case "bmp-jpg":
                    ConvertPngToJpg(inputPath, outputPath);
                    return;
                case "heic-png":
                case "heic-jpg":
                    ConvertWithMagick(inputPath, outputPath);
                    return;
                case "txt-pdf":
                    PdfWriter.WriteTextPdf(outputPath, Path.GetFileNameWithoutExtension(outputPath), File.ReadAllText(inputPath));
                    return;
                case "txt-md":
                    File.WriteAllText(outputPath, File.ReadAllText(inputPath), Encoding.UTF8);
                    return;
                case "md-txt":
                case "brf-txt":
                    File.WriteAllText(outputPath, PlainTextFromMarkdownOrText(File.ReadAllText(inputPath)), Encoding.UTF8);
                    return;
                case "kwb-txt":
                    File.WriteAllText(outputPath, ExtractKwbText(inputPath), Encoding.UTF8);
                    return;
                case "kwt-txt":
                    File.WriteAllText(outputPath, ExtractKwtText(inputPath), Encoding.UTF8);
                    return;
                case "doc-txt":
                    ConvertWordToText(inputPath, outputPath);
                    return;
                case "doc-pdf":
                    ConvertWordToPdf(inputPath, outputPath);
                    return;
                case "doc-brf":
                    string text = ExtractWordText(inputPath);
                    File.WriteAllText(outputPath, text, Encoding.ASCII);
                    return;
                case "mp3-mp4":
                case "wav-mp4":
                case "m4a-mp4":
                    ConvertAudioToVideo(inputPath, outputPath, request);
                    return;
                default:
                    if (IsRcaVoc(request.Option, inputPath))
                    {
                        ConvertRcaVoc(inputPath, outputPath, request.Option.OutputExtension, log);
                    }
                    else
                    {
                        ConvertWithFfmpeg(inputPath, outputPath, request.Option.OutputExtension, log);
                    }
                    return;
            }
        }

        private static bool IsRcaVoc(ConversionOption option, string inputPath)
        {
            if (!string.Equals(Path.GetExtension(inputPath), ".voc", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(Path.GetExtension(inputPath), ".voq", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (option == null || (option.Id != "voc-mp3" && option.Id != "voc-m4a"))
            {
                return false;
            }

            using (FileStream stream = File.OpenRead(inputPath))
            {
                byte[] header = new byte[16];
                int bytesRead = stream.Read(header, 0, header.Length);
                string signature = Encoding.ASCII.GetString(header, 0, bytesRead);
                return signature.StartsWith("VCP162_VOC_File", StringComparison.OrdinalIgnoreCase);
            }
        }

        private static void ConvertRcaVoc(string inputPath, string outputPath, string outputExtension, Action<string> log)
        {
            string devocPath = RequireDependency(DependencyLocator.FindDevoc(), "The bundled RCA VOC decoder is missing.");
            string ffmpegPath = RequireDependency(DependencyLocator.FindFfmpeg(), "FFmpeg is required for RCA VOC conversion.");
            string tempWavePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".wav");

            try
            {
                RunProcess(devocPath, "-w " + Quote(inputPath), "RCA VOC decode failed.");
                string generatedWave = Path.ChangeExtension(inputPath, ".wav");
                if (!File.Exists(generatedWave))
                {
                    throw new InvalidOperationException("The RCA VOC decoder did not create a WAV file.");
                }

                if (File.Exists(tempWavePath))
                {
                    File.Delete(tempWavePath);
                }

                File.Move(generatedWave, tempWavePath);
                string codecArguments = GetCodecArguments(outputExtension);
                RunProcess(ffmpegPath, "-y -i " + Quote(tempWavePath) + " " + codecArguments + " " + Quote(outputPath), "FFmpeg conversion failed after RCA VOC decode.");
                log("Used RCA VOC decoder plus FFmpeg for " + Path.GetFileName(inputPath) + ".");
            }
            finally
            {
                if (File.Exists(tempWavePath))
                {
                    File.Delete(tempWavePath);
                }

                string strayWave = Path.ChangeExtension(inputPath, ".wav");
                if (File.Exists(strayWave))
                {
                    File.Delete(strayWave);
                }
            }
        }

        private static void ConvertPngToJpg(string inputPath, string outputPath)
        {
            using (Image image = Image.FromFile(inputPath))
            using (Bitmap bitmap = new Bitmap(image.Width, image.Height))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        private static void ConvertWordToText(string inputPath, string outputPath)
        {
            if (WordAutomation.IsAvailable())
            {
                WordAutomation.SaveAsText(inputPath, outputPath);
                return;
            }

            if (Path.GetExtension(inputPath).Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                File.WriteAllText(outputPath, WordAutomation.ExtractDocxText(inputPath), Encoding.UTF8);
                return;
            }

            throw new InvalidOperationException("DOC conversion requires Microsoft Word on this PC.");
        }

        private static void ConvertWordToPdf(string inputPath, string outputPath)
        {
            if (WordAutomation.IsAvailable())
            {
                WordAutomation.SaveAsPdf(inputPath, outputPath);
                return;
            }

            if (Path.GetExtension(inputPath).Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                string text = WordAutomation.ExtractDocxText(inputPath);
                PdfWriter.WriteTextPdf(outputPath, Path.GetFileNameWithoutExtension(inputPath), text);
                return;
            }

            throw new InvalidOperationException("DOC conversion to PDF requires Microsoft Word on this PC.");
        }

        private static string ExtractWordText(string inputPath)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
            ConvertWordToText(inputPath, tempPath);
            string text = File.ReadAllText(tempPath);
            File.Delete(tempPath);
            return text;
        }

        private static void ConvertAudioToVideo(string inputPath, string outputPath, ConversionRequest request)
        {
            string ffmpegPath = RequireDependency(DependencyLocator.FindFfmpeg(), "FFmpeg is required for audio to video conversions.");
            string imageInput = string.Empty;
            string filter = "scale=1280:720:force_original_aspect_ratio=decrease,pad=1280:720:(ow-iw)/2:(oh-ih)/2,format=yuv420p";
            string arguments;

            if (string.Equals(request.BackgroundMode, "image", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(request.BackgroundImagePath))
            {
                imageInput = " -loop 1 -i " + Quote(request.BackgroundImagePath);
                arguments = "-loglevel error -nostats -y -i " + Quote(inputPath) + imageInput + " -filter_complex \"[1:v]" + filter + "[bg]\" -map 0:a -map \"[bg]\" -shortest -c:v libx264 -preset veryfast -c:a aac -b:a 192k " + Quote(outputPath);
            }
            else
            {
                string color = string.Equals(request.BackgroundMode, "effects", StringComparison.OrdinalIgnoreCase) ? "0x16213E" : "0x1B1B1B";
                arguments = "-loglevel error -nostats -y -i " + Quote(inputPath) + " -f lavfi -i color=c=" + color + ":s=1280x720:d=1 -filter_complex \"[1:v]format=yuv420p,drawtext=text='File Converter Ultimate':fontcolor=white:fontsize=42:x=(w-text_w)/2:y=(h-text_h)/2\" -map 0:a -map 1:v -shortest -c:v libx264 -preset veryfast -c:a aac -b:a 192k " + Quote(outputPath);
            }

            RunProcess(ffmpegPath, arguments, "Audio to MP4 conversion failed.");
        }

        private static void ConvertWithFfmpeg(string inputPath, string outputPath, string outputExtension, Action<string> log)
        {
            string ffmpegPath = RequireDependency(DependencyLocator.FindFfmpeg(), "FFmpeg is required for this conversion.");
            string codecArguments = GetCodecArguments(outputExtension);
            RunProcess(ffmpegPath, "-loglevel error -nostats -y -i " + Quote(inputPath) + " " + codecArguments + " " + Quote(outputPath), "FFmpeg conversion failed.");
            log("Used FFmpeg for " + Path.GetFileName(inputPath) + ".");
        }

        private static void ConvertWithMagick(string inputPath, string outputPath)
        {
            string magickPath = RequireDependency(DependencyLocator.FindMagick(), "ImageMagick is required for HEIC conversion.");
            RunProcess(magickPath, Quote(inputPath) + " " + Quote(outputPath), "Image conversion failed.");
        }

        private static string GetCodecArguments(string outputExtension)
        {
            switch (outputExtension)
            {
                case ".mp3":
                    return "-vn -codec:a libmp3lame -q:a 2";
                case ".m4a":
                    return "-vn -codec:a aac -b:a 192k";
                case ".wav":
                    return "-vn -codec:a pcm_s16le";
                case ".mov":
                    return "-c:v libx264 -c:a aac -movflags +faststart";
                case ".mp4":
                    return "-c:v libx264 -c:a aac -movflags +faststart";
                case ".m4v":
                    return "-c:v libx264 -c:a aac";
                default:
                    return string.Empty;
            }
        }

        private static string PlainTextFromMarkdownOrText(string content)
        {
            string text = Regex.Replace(content ?? string.Empty, @"!\[[^\]]*\]\([^)]+\)", string.Empty);
            text = Regex.Replace(text, @"\[[^\]]+\]\([^)]+\)", "$0");
            text = text.Replace("**", string.Empty).Replace("__", string.Empty).Replace("`", string.Empty).Replace("#", string.Empty);
            return text;
        }

        private static string ExtractKwtText(string inputPath)
        {
            byte[] bytes = File.ReadAllBytes(inputPath);
            StringBuilder asciiBuilder = new StringBuilder(bytes.Length);
            foreach (byte value in bytes)
            {
                if ((value >= 32 && value <= 126) || value == 9 || value == 10 || value == 13)
                {
                    asciiBuilder.Append((char)value);
                }
                else
                {
                    asciiBuilder.Append(' ');
                }
            }

            List<string> lines = asciiBuilder
                .ToString()
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
                .SelectMany(line => Regex.Split(line, "\\s{2,}"))
                .Select(NormalizeExtractedLine)
                .Where(line => line.Length >= 4)
                .Distinct()
                .ToList();

            if (lines.Count == 0)
            {
                throw new InvalidOperationException("No readable text could be extracted from this KWT file.");
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static string ExtractKwbText(string inputPath)
        {
            byte[] bytes = File.ReadAllBytes(inputPath);
            StringBuilder asciiBuilder = new StringBuilder(bytes.Length);
            foreach (byte value in bytes)
            {
                if ((value >= 32 && value <= 126) || value == 9 || value == 10 || value == 13)
                {
                    asciiBuilder.Append((char)value);
                }
                else
                {
                    asciiBuilder.Append(' ');
                }
            }

            List<string> candidateLines = asciiBuilder
                .ToString()
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
                .SelectMany(line => Regex.Split(line, "\\s{2,}"))
                .Select(NormalizeExtractedLine)
                .Where(line => line.Length >= 4)
                .Where(IsLikelyKwbContentLine)
                .ToList();

            List<string> translatedLines = TryBackTranslateKwbLinesWithLibLouis(candidateLines);
            if (translatedLines != null && translatedLines.Count > 0)
            {
                List<string> normalizedTranslatedLines = translatedLines
                    .Select(NormalizeExtractedLine)
                    .Where(line => line.Length >= 2)
                    .ToList();

                if (normalizedTranslatedLines.Count > 0)
                {
                    return ReflowTranslatedLines(normalizedTranslatedLines);
                }
            }

            List<string> lines = candidateLines;

            if (lines.Count == 0)
            {
                throw new InvalidOperationException("No readable text could be extracted from this KWB file.");
            }

            List<string> dedupedLines = new List<string>();
            foreach (string line in lines)
            {
                if (dedupedLines.Count == 0 || !string.Equals(dedupedLines[dedupedLines.Count - 1], line, StringComparison.Ordinal))
                {
                    dedupedLines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, dedupedLines);
        }

        private static string ReflowTranslatedLines(List<string> lines)
        {
            List<string> paragraphs = new List<string>();
            StringBuilder current = new StringBuilder();

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                {
                    FlushParagraph(current, paragraphs);
                    continue;
                }

                if (current.Length == 0)
                {
                    current.Append(line);
                    continue;
                }

                if (ShouldStartNewParagraph(current.ToString(), line))
                {
                    FlushParagraph(current, paragraphs);
                    current.Append(line);
                }
                else
                {
                    if (!EndsWithOpeningQuoteOrDash(current.ToString()))
                    {
                        current.Append(' ');
                    }

                    current.Append(line);
                }
            }

            FlushParagraph(current, paragraphs);
            return string.Join(Environment.NewLine + Environment.NewLine, paragraphs);
        }

        private static bool ShouldStartNewParagraph(string current, string next)
        {
            if (string.IsNullOrWhiteSpace(current))
            {
                return true;
            }

            char lastChar = current[current.Length - 1];
            if (lastChar == '.' || lastChar == '!' || lastChar == '?')
            {
                return true;
            }

            if (next.StartsWith("\"", StringComparison.Ordinal) ||
                next.StartsWith("'", StringComparison.Ordinal))
            {
                return false;
            }

            if (Regex.IsMatch(next, @"^(chapter|page)\b", RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool EndsWithOpeningQuoteOrDash(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            char lastChar = value[value.Length - 1];
            return lastChar == '"' || lastChar == '\'' || lastChar == '-' || lastChar == '(';
        }

        private static void FlushParagraph(StringBuilder current, List<string> paragraphs)
        {
            if (current.Length == 0)
            {
                return;
            }

            string paragraph = Regex.Replace(current.ToString(), "\\s+", " ").Trim();
            if (paragraph.Length > 0)
            {
                paragraphs.Add(paragraph);
            }

            current.Clear();
        }

        private static List<string> TryBackTranslateKwbLinesWithLibLouis(List<string> brailleAsciiLines)
        {
            string louTranslatePath = DependencyLocator.FindLouTranslate();
            string tablesDirectory = DependencyLocator.FindLibLouisTablesDirectory();
            if (string.IsNullOrWhiteSpace(louTranslatePath) || string.IsNullOrWhiteSpace(tablesDirectory) || brailleAsciiLines == null || brailleAsciiLines.Count == 0)
            {
                return null;
            }

            List<string> translatedLines = new List<string>();
            foreach (string line in brailleAsciiLines)
            {
                string translatedLine = RunProcessWithInput(
                    louTranslatePath,
                    "-b -d en-us-brf.dis en-us-g2.ctb",
                    line + Environment.NewLine,
                    "LOUIS_TABLEPATH",
                    tablesDirectory,
                    "LibLouis back-translation failed.");

                translatedLine = translatedLine
                    .Replace("\r\n", "\n")
                    .Replace('\r', '\n')
                    .Trim('\n', '\r', ' ', '\t');

                if (translatedLine.Length > 0)
                {
                    translatedLines.Add(translatedLine);
                }
            }

            return translatedLines;
        }

        private static string NormalizeExtractedLine(string value)
        {
            string line = Regex.Replace(value ?? string.Empty, "\\s+", " ").Trim();
            if (Regex.IsMatch(line, @"^(?:[A-Za-z]\s+){3,}[A-Za-z]$"))
            {
                line = line.Replace(" ", string.Empty);
            }

            if (Regex.IsMatch(line, @"^(.)\1{4,}$"))
            {
                return string.Empty;
            }

            if (Regex.IsMatch(line, @"^[A-Za-z]{1,3}$"))
            {
                return string.Empty;
            }

            if (Regex.IsMatch(line, @"^[A-Za-z]\s[A-Za-z](?:\s[A-Za-z])+$"))
            {
                return string.Empty;
            }

            if (line.StartsWith("laI", StringComparison.Ordinal))
            {
                line = "I" + line.Substring(3);
            }

            return line;
        }

        private static bool IsLikelyKwbContentLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            int letterOrDigitCount = line.Count(char.IsLetterOrDigit);
            if (letterOrDigitCount < 3)
            {
                return false;
            }

            int punctuationCount = line.Count(character => !char.IsLetterOrDigit(character) && !char.IsWhiteSpace(character));
            if (punctuationCount > letterOrDigitCount * 2)
            {
                return false;
            }

            return true;
        }

        private static string RequireDependency(string path, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException(errorMessage);
            }

            return path;
        }

        private static void RunProcess(string fileName, string arguments, string errorPrefix)
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                process.Start();
                Task<string> standardOutputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> standardErrorTask = process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                Task.WaitAll(standardOutputTask, standardErrorTask);
                string standardOutput = standardOutputTask.Result;
                string standardError = standardErrorTask.Result;

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(errorPrefix + Environment.NewLine + standardError + Environment.NewLine + standardOutput);
                }
            }
        }

        private static string RunProcessWithInput(string fileName, string arguments, string standardInput, string environmentVariableName, string environmentVariableValue, string errorPrefix)
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                if (!string.IsNullOrWhiteSpace(environmentVariableName))
                {
                    process.StartInfo.EnvironmentVariables[environmentVariableName] = environmentVariableValue ?? string.Empty;
                }

                process.Start();
                process.StandardInput.Write(standardInput ?? string.Empty);
                process.StandardInput.Close();

                string standardOutput = process.StandardOutput.ReadToEnd();
                string standardError = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(errorPrefix + Environment.NewLine + standardError + Environment.NewLine + standardOutput);
                }

                return standardOutput;
            }
        }

        private static string Quote(string path)
        {
            return "\"" + path + "\"";
        }
    }
}
