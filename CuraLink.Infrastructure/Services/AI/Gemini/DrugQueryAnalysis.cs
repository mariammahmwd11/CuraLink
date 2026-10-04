using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Services.AI.Gemini
{
    public class DrugQueryAnalysis
    {
        public string Intent { get; set; } = string.Empty;

        public List<string> Medications { get; set; } = [];
    }
}
