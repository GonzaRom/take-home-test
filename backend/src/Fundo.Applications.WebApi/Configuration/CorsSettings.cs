using System;

namespace Fundo.Applications.WebApi.Configuration
{
    public sealed class CorsSettings
    {
        public const string SectionName = "Cors";

        public string PolicyName { get; set; } = string.Empty;

        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }
}
