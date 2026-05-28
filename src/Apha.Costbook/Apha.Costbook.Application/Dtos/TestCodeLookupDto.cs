namespace Apha.Costbook.Application.Dtos;

public class TestCodeLookupDto
{
    public string ItemCode { get; set; } = null!;
    public string? ItemDescription { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? UnitPriceWithInflamation { get; set; }
}
