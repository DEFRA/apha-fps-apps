/*
 * TRANSFORMENGINE MIGRATION — TestRequirementLogItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — read-only DataGrid Item model for the "Test Requirement Changes" audit log tab
 *   - 11 visible columns derived from initializeTestRequirementChangesTable() JS columns array
 *     plus one hidden SequenceNo PK (not in JS columns; used as KeyProperty)
 *   - showAddButton: false; no edit/delete buttons → AllowAdd=false, AllowEdit=false, AllowDelete=false
 *   - All columns set to GridColumnType.ReadOnly — no editing
 *   - Property names match TestRequirementLogDto exactly for AutoMapper convention mapping
 *
 * PRESERVED:
 *   - JS column order: testCode, buyer, unitPrice, noRequired, projectBuyerCode, testBuyerCode,
 *     active, dateTime, userId, userEmail, insertDelete
 *   - Display labels from JS column header values
 *   - Column widths from JS DataGridComponent columns array
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UserEmail is NOT in TestRequirementLogDto. AutoMapper must Ignore() this.
 *     Requires backend/service to resolve email from UserId.
 *   - TRANSFORMENGINE TODO: UnitPrice is decimal? in DTO (DDL type is double precision); verify
 *     DecimalNumber display type is appropriate or whether GbpValue should be used.
 *   - TRANSFORMENGINE TODO: NoRequired is double? in DTO (DDL type integer); verify display intent.
 *   - TRANSFORMENGINE TODO: Active is short? — confirm display as 0/1 or Yes/No in grid.
 *   - TRANSFORMENGINE TODO: FpsViewModelMapper.cs CreateMap<TestRequirementLogDto, TestRequirementLogItem>()
 *     stub must be uncommented with ForMember Ignore() for UserEmail.
 */
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Read-only DataGrid row model for the Test Requirement Changes audit log tab.
    /// Derives from JS initializeTestRequirementChangesTable() columns array (11 visible columns).
    /// Property names match TestRequirementLogDto exactly for AutoMapper convention mapping.
    /// </summary>
    public class TestRequirementLogItem
    {
        // TRANSFORMENGINE: Hidden PK — SequenceNo is not in JS visible columns; used as KeyProperty only
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SequenceNo { get; set; }

        // TRANSFORMENGINE: JS column field=testCode, header=TestCode, width=120; DTO property: TestCode
        [Display(Name = "TestCode")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? TestCode { get; set; }

        // TRANSFORMENGINE: JS column field=buyer, header=Buyer, width=110; DTO property: Buyer
        [Display(Name = "Buyer")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Buyer { get; set; }

        // TRANSFORMENGINE: JS column field=unitPrice, header=UnitPrice, width=110; DTO property: UnitPrice (decimal?)
        [Display(Name = "UnitPrice")]
        [GridColumn(Width = 110, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? UnitPrice { get; set; }

        // TRANSFORMENGINE: JS column field=noRequired, header=NoRequired, width=120; DTO property: NoRequired (double?)
        [Display(Name = "NoRequired")]
        [GridColumn(Width = 120, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double? NoRequired { get; set; }

        // TRANSFORMENGINE: JS column field=projectBuyerCode, header=ProjectBuyerCode, width=180; DTO property: ProjectBuyerCode
        [Display(Name = "ProjectBuyerCode")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectBuyerCode { get; set; }

        // TRANSFORMENGINE: JS column field=testBuyerCode, header=TestBuyerCode, width=160; DTO property: TestBuyerCode
        [Display(Name = "TestBuyerCode")]
        [GridColumn(Width = 160, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? TestBuyerCode { get; set; }

        // TRANSFORMENGINE: JS column field=active, header=Active, width=90; DTO property: Active (short?)
        [Display(Name = "Active")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public short? Active { get; set; }

        // TRANSFORMENGINE: JS column field=dateTime, header=Date_Time, width=180; DTO property: DateTime
        [Display(Name = "Date_Time")]
        [GridColumn(Width = 180, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateTime { get; set; }

        // TRANSFORMENGINE: JS column field=userId, header=User_ID, width=170; DTO property: UserId
        [Display(Name = "User_ID")]
        [GridColumn(Width = 170, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? UserId { get; set; }

        // TRANSFORMENGINE TODO: UserEmail not in TestRequirementLogDto — requires backend UserId→email resolution.
        // JS column field=userEmail, header=User_Email, width=240
        [Display(Name = "User_Email")]
        [GridColumn(Width = 240, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? UserEmail { get; set; }

        // TRANSFORMENGINE: JS column field=insertDelete, header=Insert_Delete, width=130; DTO property: InsertDelete
        [Display(Name = "Insert_Delete")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? InsertDelete { get; set; }
    }
}
