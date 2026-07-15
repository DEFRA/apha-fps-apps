namespace Apha.PIMS.Application.Dtos
{
    public class StagingMilestoneDto
    {
        public int Id { get; set; }
        public string? Project { get; set; }
        public string? Number { get; set; }
        public string? Description { get; set; }
        public DateTime DateDue { get; set; }
        public string? Note { get; set; }
        public string? AltDescription { get; set; }
        public string? AltDate { get; set; }
        public string? AltNumber { get; set; }
        public string? TypeId { get; set; }
        public string? CreatedBy { get; set; }
    }
}
