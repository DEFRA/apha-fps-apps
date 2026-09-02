using System.ComponentModel.DataAnnotations;

namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// One row of the FEC/AGRUP workbook's "Instructions" sheet — plain guidance text, not
    /// business data. Column headers are intentionally blank ([Display(Name = "")]) so the
    /// sheet reads as instructional text rather than a data table; ExcelExportService's shared
    /// multi-sheet builder always writes a header row, so row 1 renders blank rather than
    /// absent (Apha.Common is not touched to add a headerless mode for this one sheet).
    /// </summary>
    public class BulkRatesFecAgrupInstructionRowDto
    {
        [Display(Name = "")]
        public string? Item { get; set; }

        [Display(Name = "")]
        public string? SubItem { get; set; }

        [Display(Name = "")]
        public string? ColumnRef { get; set; }

        [Display(Name = "")]
        public string? Text { get; set; }
    }
}
