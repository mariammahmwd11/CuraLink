using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Services.AI.Gemini
{
    public class GeminiOptions
    {
        public const string SectionName = "Gemini";

        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;
    }
}
