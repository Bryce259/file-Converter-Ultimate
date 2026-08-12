namespace FileConverterUltimateApp
{
    internal sealed class ConversionResult
    {
        public bool Success { get; set; }
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public string Message { get; set; }
    }
}
