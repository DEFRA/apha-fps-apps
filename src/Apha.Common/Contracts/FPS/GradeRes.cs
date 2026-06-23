namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a Grade record.
    /// Contains the full RecordSource surface of fps.grade required by CRUD responses.
    /// </summary>
    public class GradeRes
    {
        // TRANSFORMENGINE: GradeCode — composite PK part 1; maps to 'gradecode' (varchar 10) NOT NULL
        /// <summary>Grade code (primary key).</summary>
        public string GradeCode { get; set; } = null!;

        // TRANSFORMENGINE: Description — maps to 'desc_long' (varchar 30); nullable in DDL
        /// <summary>Grade description. Maps to desc_long column.</summary>
        public string? Description { get; set; }

        // TRANSFORMENGINE: AvSalary — maps to 'avsalary' (money DEFAULT 0); nullable in response
        /// <summary>Average salary. Maps to avsalary column.</summary>
        public decimal? AvSalary { get; set; }

        // TRANSFORMENGINE: PactCode — maps to 'pactcode' (varchar 50); not in HTML prototype but present in DDL
        /// <summary>PACT system code. Maps to pactcode column.</summary>
        public string? PactCode { get; set; }

        // TRANSFORMENGINE: AvLeaveHrs — maps to 'avleavehrs' (double precision DEFAULT 0)
        /// <summary>Average leave hours. Maps to avleavehrs column.</summary>
        public double? AvLeaveHrs { get; set; }

        // TRANSFORMENGINE: AvSickHrs — maps to 'avsickhrs' (double precision DEFAULT 0)
        /// <summary>Average sick hours. Maps to avsickhrs column.</summary>
        public double? AvSickHrs { get; set; }

        // TRANSFORMENGINE: FpsYear — composite PK part 2; partition key; required for route binding
        /// <summary>FPS financial year (composite primary key, partition key).</summary>
        public int FpsYear { get; set; }
    }
}
