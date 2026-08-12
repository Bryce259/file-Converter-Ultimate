using System;
using System.Collections.Generic;
using System.IO;

namespace FileConverterUltimateApp
{
    internal static class DependencyLocator
    {
        public static string FindFfmpeg()
        {
            return FindExecutable("ffmpeg.exe", new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "ffmpeg.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin", "ffmpeg.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ffmpeg", "bin", "ffmpeg.exe")
            });
        }

        public static string FindDevoc()
        {
            return FindExecutable("devoc.exe", new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "devoc.exe")
            });
        }

        public static string FindLouTranslate()
        {
            return FindExecutable("lou_translate.exe", new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "LibLouis", "bin", "lou_translate.exe")
            });
        }

        public static string FindLibLouisTablesDirectory()
        {
            string tablesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "LibLouis", "share", "liblouis", "tables");
            return Directory.Exists(tablesDirectory) ? tablesDirectory : null;
        }

        public static string FindPandoc()
        {
            return FindExecutable("pandoc.exe", new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "pandoc.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Pandoc", "pandoc.exe")
            });
        }

        public static string FindMagick()
        {
            return FindExecutable("magick.exe", new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "magick.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "ImageMagick", "magick.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ImageMagick-7.1.1-Q16-HDRI", "magick.exe")
            });
        }

        private static string FindExecutable(string executableName, IEnumerable<string> fallbacks)
        {
            foreach (string fallback in fallbacks)
            {
                if (File.Exists(fallback))
                {
                    return fallback;
                }
            }

            string pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (string folder in pathValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = Path.Combine(folder.Trim(), executableName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
