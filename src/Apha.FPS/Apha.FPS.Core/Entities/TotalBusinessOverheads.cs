using System.ComponentModel.DataAnnotations.Schema;

namespace Apha.FPS.Core.Entities
{
    public partial class TotalBusinessOverheads
    {
        [Column(TypeName = "money")]
        public decimal? BusinessOverheads { get; set; }

        public int FpsYear { get; set; }
    }
}
