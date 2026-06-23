/*
 * TRANSFORMENGINE MIGRATION — StaffJobLogItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — read-only DataGrid Item model for the "Staff Plan Changes" audit log tab
 *   - 8 visible columns derived from initializeStaffPlanChangesTable() JS columns array
 *     plus one hidden SequenceNo PK (not in JS columns; used as KeyProperty)
 *   - showAddButton: false; no edit/delete buttons → AllowAdd=false, AllowEdit=false, AllowDelete=false
 *   - All columns set to GridColumnType.ReadOnly — no editing
 *   - Property names match StaffJobLogDto exactly for AutoMapper convention mapping
 *
 * PRESERVED:
 *   - JS column order: staffId, name, jobcode→JobCode, plannedHours, dateTime, userId, userEmail, insertDelete
 *   - Display labels from JS column header values
 *   - Column widths from JS DataGridComponent columns array
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Name property is NOT in StaffJobLogDto (backend DTO only has StaffId, JobCode).
 *     StaffJobLogRes.Name requires a lookup join. AutoMapper must Ignore() this field until
 *     the service/repository is updated to include the staff display name.
 *   - TRANSFORMENGINE TODO: UserEmail is NOT in StaffJobLogDto. AutoMapper must Ignore() this field.
 *     Requires backend/service to resolve email from UserId.
 *   - TRANSFORMENGINE TODO: FpsViewModelMapper.cs CreateMap<StaffJobLogDto, StaffJobLogItem>()
 *     stub must be uncommented with ForMember Ignore() for Name and UserEmail.
 */
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Read-only DataGrid row model for the Staff Plan Changes audit log tab.
    /// Derives from JS initializeStaffPlanChangesTable() columns array (8 visible columns).
    /// Property names match StaffJobLogDto exactly where applicable; Name and UserEmail are
    /// display-only fields not present in the DTO (see DEFERRED notes).
    /// </summary>
    public class StaffJobLogItem
    {
        // TRANSFORMENGINE: Hidden PK — SequenceNo is not in JS visible columns; used as KeyProperty only
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SequenceNo { get; set; }

        // TRANSFORMENGINE: JS column field=staffId, header=StaffID, width=100; DTO property: StaffId
        [Display(Name = "StaffID")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string StaffId { get; set; } = null!;

        // TRANSFORMENGINE TODO: Name not in StaffJobLogDto — requires staff lookup join.
        // JS column field=name, header=Name, width=240
        [Display(Name = "Name")]
        [GridColumn(Width = 240, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        // TRANSFORMENGINE: JS column field=jobcode, header=Jobcode, width=120; DTO property: JobCode
        [Display(Name = "Jobcode")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=plannedHours, header=Plannedhours, width=140; DTO property: PlannedHours (double)
        [Display(Name = "Plannedhours")]
        [GridColumn(Width = 140, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double PlannedHours { get; set; }

        // TRANSFORMENGINE: JS column field=dateTime, header=Date_Time, width=180; DTO property: DateTime
        [Display(Name = "Date_Time")]
        [GridColumn(Width = 180, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateTime { get; set; }

        // TRANSFORMENGINE: JS column field=userId, header=User_ID, width=170; DTO property: UserId
        [Display(Name = "User_ID")]
        [GridColumn(Width = 170, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? UserId { get; set; }

        // TRANSFORMENGINE TODO: UserEmail not in StaffJobLogDto — requires backend UserId→email resolution.
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
