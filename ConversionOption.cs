using System;
using System.Collections.Generic;
using System.Linq;

namespace FileConverterUltimateApp
{
    internal sealed class ConversionOption
    {
        public ConversionOption(string id, string displayName, string category, IEnumerable<string> inputExtensions, string outputExtension, bool allowsVideoBackground = false, bool usesPlainTextBridge = false)
        {
            Id = id;
            DisplayName = displayName;
            Category = category;
            InputExtensions = inputExtensions.Select(extension => extension.ToLowerInvariant()).ToArray();
            OutputExtension = outputExtension.ToLowerInvariant();
            AllowsVideoBackground = allowsVideoBackground;
            UsesPlainTextBridge = usesPlainTextBridge;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public IReadOnlyList<string> InputExtensions { get; }
        public string OutputExtension { get; }
        public bool AllowsVideoBackground { get; }
        public bool UsesPlainTextBridge { get; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
