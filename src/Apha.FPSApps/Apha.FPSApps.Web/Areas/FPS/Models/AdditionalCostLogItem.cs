/*
 * TRANSFORMENGINE MIGRATION — AdditionalCostLogItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — read-only DataGrid Item model for the "Exceptional Cost Changes" audit log tab
 *   - 10 visible columns derived from initializeExceptionalCostChangesTable() JS columns array
 *     plus one hidden SequenceNo PK (not in JS columns; used as KeyProperty)
 *   - showAddButton: false; no edit/delete buttons → AllowAdd=false, AllowEdit=false, AllowDelete=false
 *   - All columns set to GridColumnType.ReadOnly — no editing
 *   - Property names match AdditionalCostLogDto exactly for AutoMapper convention mapping
 *
 * PRESERVED:
 *   - JS column order: jobCode, account, description, itemCost, freq, supplier, dateTime, userId, userEmail, insertDelete
 *   - Display labels from JS column header values
 *   - Column widths from JS DataGridComponent columns array
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UserEmail is NOT in AdditionalCostLogDto. AutoMapper must Ignore() this.
 *     Requires backend/service to resolve email from UserId.
 *   - TRANSFORMENGINE TODO: FpsViewModelMapper.cs CreateMap<AdditionalCostLogDto, AdditionalCostLogItem>()
 *     stub must be uncommented with ForMember Ignore() for UserEmail.
 */
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Read-only DataGrid row model for the Exceptional Cost Changes audit log tab.
    /// Derives from JS initializeExceptionalCostChangesTable() columns array (10 visible columns).
    /// Property names match AdditionalCostLogDto exactly for AutoMapper convention mapping.
    /// </summary>
    public class AdditionalCostLogItem
    {
        // TRANSFORMENGINE: Hidden PK — SequenceNo is not in JS visible columns; used as KeyProperty only
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SequenceNo { get; set; }

        // TRANSFORMENGINE: JS column field=jobCode, header=JobCode, width=120; DTO property: JobCode
        [Display(Name = "JobCode")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=account, header=Account, width=200; DTO property: Account
        [Display(Name = "Account")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Account { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=description, header=Description, width=180; DTO property: Description
        [Display(Name = "Description")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string Description { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=itemCost, header=ItemCost, width=140; DTO property: ItemCost (decimal)
        [Display(Name = "ItemCost")]
        [GridColumn(Width = 140, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal ItemCost { get; set; }

        // TRANSFORMENGINE: JS column field=freq, header=Freq, width=90; DTO property: Freq (string?)
        [Display(Name = "Freq")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Freq { get; set; }

        // TRANSFORMENGINE: JS column field=supplier, header=Supplier, width=130; DTO property: Supplier (string?)
        [Display(Name = "Supplier")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Supplier { get; set; }

        // TRANSFORMENGINE: JS column field=dateTime, header=Date_Time, width=180; DTO property: DateTime
        [Display(Name = "Date_Time")]
        [GridColumn(Width = 180, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateTime { get; set; }

        // TRANSFORMENGINE: JS column field=userId, header=User_ID, width=170; DTO property: UserId
        [Display(Name = "User_ID")]
        [GridColumn(Width = 170, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? UserId { get; set; }

        // TRANSFORMENGINE TODO: UserEmail not in AdditionalCostLogDto — requires backend UserId→email resolution.
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
