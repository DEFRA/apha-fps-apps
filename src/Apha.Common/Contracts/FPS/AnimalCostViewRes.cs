using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Common.Contracts.FPS
{
    public class AnimalCostViewRes
    {
        public int IndCounter { get; set; }
        public string? Programme { get; set; }
        public string? AnimalType { get; set; }
        public string? JobCode { get; set; }
        public double NumberOfDays { get; set; }
        public double NumberOfAnimals { get; set; }
        public decimal? DailyRate { get; set; }
        public decimal? AnimalCost { get; set; }
        public double TotalDays { get; set; }
    }
}
