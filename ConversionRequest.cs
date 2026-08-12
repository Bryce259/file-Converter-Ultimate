namespace FileConverterUltimateApp
{
    internal sealed class ConversionRequest
    {
        public ConversionOption Option { get; set; }
        public string InputPath { get; set; }
        public string OutputDirectory { get; set; }
        public bool IsZipBatch { get; set; }
        public string ExtractedRootDirectory { get; set; }
        public string BackgroundMode { get; set; }
        public string BackgroundImagePath { get; set; }
    }
}
