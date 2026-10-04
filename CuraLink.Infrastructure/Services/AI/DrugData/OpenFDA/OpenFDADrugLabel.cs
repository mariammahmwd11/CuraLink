using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Services.AI.DrugData.OpenFDA
{
    public class OpenFDADrugLabel
    {
        public string? BrandName { get; set; }

        public string? GenericName { get; set; }

        public List<string> ActiveIngredients { get; set; } = [];

        public List<string> DosageAndAdministration { get; set; } = [];

        public List<string> Warnings { get; set; } = [];

        public List<string> AdverseReactions { get; set; } = [];

        public List<string> DrugInteractions { get; set; } = [];

        public List<string> IndicationsAndUsage { get; set; } = [];
    }
}
