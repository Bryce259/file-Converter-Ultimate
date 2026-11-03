using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileConverterUltimateApp
{
    internal static class PdfWriter
    {
        public static void WriteTextPdf(string filePath, string title, string content)
        {
            string normalizedContent = (content ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            List<string> lines = WrapLines(normalizedContent, 90);
            if (lines.Count == 0)
            {
                lines.Add(string.Empty);
            }

            const int linesPerPage = 44;
            List<string> pages = new List<string>();
            for (int index = 0; index < lines.Count; index += linesPerPage)
            {
                IEnumerable<string> pageLines = lines.Skip(index).Take(linesPerPage);
                StringBuilder pageBuilder = new StringBuilder();
                pageBuilder.AppendLine("BT");
                pageBuilder.AppendLine("/F1 11 Tf");
                pageBuilder.AppendLine("50 792 Td");
                pageBuilder.AppendLine("14 TL");

                bool firstLine = true;
                foreach (string line in pageLines)
                {
                    if (firstLine)
                    {
                        pageBuilder.AppendLine("(" + EscapePdfText(line) + ") Tj");
                        firstLine = false;
                    }
                    else
                    {
                        pageBuilder.AppendLine("T* (" + EscapePdfText(line) + ") Tj");
                    }
                }

                pageBuilder.AppendLine("ET");
                pages.Add(pageBuilder.ToString());
            }

            using (FileStream stream = File.Create(filePath))
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII))
            {
                List<long> offsets = new List<long>();
                writer.Write(Encoding.ASCII.GetBytes("%PDF-1.4\n"));

                offsets.Add(stream.Position);
                WriteObject(writer, 1, "<< /Type /Catalog /Pages 2 0 R >>");

                offsets.Add(stream.Position);
                string kids = string.Join(" ", Enumerable.Range(0, pages.Count).Select(index => (3 + index * 2) + " 0 R"));
                WriteObject(writer, 2, "<< /Type /Pages /Count " + pages.Count + " /Kids [ " + kids + " ] >>");

                for (int pageIndex = 0; pageIndex < pages.Count; pageIndex++)
                {
                    int pageObjectId = 3 + pageIndex * 2;
                    int contentObjectId = pageObjectId + 1;

                    offsets.Add(stream.Position);
                    WriteObject(writer, pageObjectId, "<< /Type /Page /Parent 2 0 R /MediaBox [ 0 0 612 792 ] /Resources << /Font << /F1 100 0 R >> >> /Contents " + contentObjectId + " 0 R >>");

                    string pageContent = pages[pageIndex];
                    byte[] pageBytes = Encoding.ASCII.GetBytes(pageContent);
                    offsets.Add(stream.Position);
                    WriteObject(writer, contentObjectId, "<< /Length " + pageBytes.Length + " >>\nstream\n" + pageContent + "endstream");
                }

                offsets.Add(stream.Position);
                WriteObject(writer, 100, "<< /Type /Font /Subtype /Type1 /BaseFont /Courier >>");

                long xrefOffset = stream.Position;
                writer.Write(Encoding.ASCII.GetBytes("xref\n"));
                writer.Write(Encoding.ASCII.GetBytes("0 " + (offsets.Count + 1) + "\n"));
                writer.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
                foreach (long offset in offsets)
                {
                    writer.Write(Encoding.ASCII.GetBytes(offset.ToString("D10") + " 00000 n \n"));
                }

                string infoTitle = EscapePdfText(title ?? Path.GetFileNameWithoutExtension(filePath));
                writer.Write(Encoding.ASCII.GetBytes("trailer\n<< /Size " + (offsets.Count + 1) + " /Root 1 0 R /Info << /Title (" + infoTitle + ") >> >>\n"));
                writer.Write(Encoding.ASCII.GetBytes("startxref\n" + xrefOffset + "\n%%EOF"));
            }
        }

        private static void WriteObject(BinaryWriter writer, int objectId, string content)
        {
            writer.Write(Encoding.ASCII.GetBytes(objectId + " 0 obj\n" + content + "\nendobj\n"));
        }

        private static string EscapePdfText(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("\t", "    ");
        }

        private static List<string> WrapLines(string content, int width)
        {
            List<string> result = new List<string>();
            foreach (string originalLine in content.Split('\n'))
            {
                string line = originalLine;
                if (line.Length == 0)
                {
                    result.Add(string.Empty);
                    continue;
                }

                while (line.Length > width)
                {
                    int splitIndex = line.LastIndexOf(' ', width);
                    if (splitIndex <= 0)
                    {
                        splitIndex = width;
                    }

                    result.Add(line.Substring(0, splitIndex).TrimEnd());
                    line = line.Substring(splitIndex).TrimStart();
                }

                result.Add(line);
            }

            return result;
        }
    }
}
