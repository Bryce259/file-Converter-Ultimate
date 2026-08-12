using System;
using System.IO;

namespace FileConverterUltimateApp
{
    internal static class WordAutomation
    {
        public static bool IsAvailable()
        {
            try
            {
                Type wordType = Type.GetTypeFromProgID("Word.Application");
                return wordType != null;
            }
            catch
            {
                return false;
            }
        }

        public static void SaveAsText(string inputPath, string outputPath)
        {
            SaveDocument(inputPath, outputPath, 2);
        }

        public static void SaveAsPdf(string inputPath, string outputPath)
        {
            SaveDocument(inputPath, outputPath, 17);
        }

        private static void SaveDocument(string inputPath, string outputPath, int format)
        {
            Type wordType = Type.GetTypeFromProgID("Word.Application");
            if (wordType == null)
            {
                throw new InvalidOperationException("Microsoft Word is not installed.");
            }

            object application = Activator.CreateInstance(wordType);
            object documents = null;
            object document = null;

            try
            {
                wordType.InvokeMember("Visible", System.Reflection.BindingFlags.SetProperty, null, application, new object[] { false });
                documents = wordType.InvokeMember("Documents", System.Reflection.BindingFlags.GetProperty, null, application, null);
                document = documents.GetType().InvokeMember("Open", System.Reflection.BindingFlags.InvokeMethod, null, documents, new object[] { inputPath, false, true });

                object[] saveArguments = new object[16];
                saveArguments[0] = outputPath;
                saveArguments[1] = format;
                document.GetType().InvokeMember("SaveAs2", System.Reflection.BindingFlags.InvokeMethod, null, document, saveArguments);
            }
            finally
            {
                if (document != null)
                {
                    try
                    {
                        document.GetType().InvokeMember("Close", System.Reflection.BindingFlags.InvokeMethod, null, document, new object[] { false });
                    }
                    catch
                    {
                    }

                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(document);
                }

                if (documents != null)
                {
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(documents);
                }

                if (application != null)
                {
                    try
                    {
                        wordType.InvokeMember("Quit", System.Reflection.BindingFlags.InvokeMethod, null, application, null);
                    }
                    catch
                    {
                    }

                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(application);
                }
            }
        }

        public static string ExtractDocxText(string inputPath)
        {
            using (var archive = System.IO.Compression.ZipFile.OpenRead(inputPath))
            {
                var entry = archive.GetEntry("word/document.xml");
                if (entry == null)
                {
                    throw new InvalidOperationException("The DOCX file does not contain document.xml.");
                }

                using (Stream stream = entry.Open())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string xml = reader.ReadToEnd();
                    string text = System.Text.RegularExpressions.Regex.Replace(xml, "<[^>]+>", " ");
                    text = System.Net.WebUtility.HtmlDecode(text);
                    return System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ").Trim();
                }
            }
        }
    }
}
