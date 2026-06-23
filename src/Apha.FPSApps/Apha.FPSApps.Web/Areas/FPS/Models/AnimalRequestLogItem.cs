/*
 * TRANSFORMENGINE MIGRATION — AnimalRequestLogItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — read-only DataGrid Item model for the "Animal Requirement Changes" audit log tab
 *   - 8 visible columns derived from initializeAnimalRequirementChangesTable() JS columns array
 *     plus one hidden SequenceNo PK (not in JS columns; used as KeyProperty)
 *   - showAddButton: false; no edit/delete buttons → AllowAdd=false, AllowEdit=false, AllowDelete=false
 *   - All columns set to GridColumnType.ReadOnly — no editing
 *   - Property names match AnimalRequestLogDto exactly for AutoMapper convention mapping
 *
 * PRESERVED:
 *   - JS column order: jobCode, animalType, numberOfDays, numberOfAnimals, dateTime, userId, userEmail, insertDelete
 *   - Display labels from JS column header values
 *   - Column widths from JS DataGridComponent columns array
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UserEmail is NOT in AnimalRequestLogDto. AutoMapper must Ignore() this.
 *     Requires backend/service to resolve email from UserId.
 *   - TRANSFORMENGINE TODO: NumberOfDays and NumberOfAnimals are double (NOT NULL) in DTO.
 *     Verify rounding/formatting at display boundary.
 *   - TRANSFORMENGINE TODO: FpsViewModelMapper.cs CreateMap<AnimalRequestLogDto, AnimalRequestLogItem>()
 *     stub must be uncommented with ForMember Ignore() for UserEmail.
 */
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Read-only DataGrid row model for the Animal Requirement Changes audit log tab.
    /// Derives from JS initializeAnimalRequirementChangesTable() columns array (8 visible columns).
    /// Property names match AnimalRequestLogDto exactly for AutoMapper convention mapping.
    /// </summary>
    public class AnimalRequestLogItem
    {
        // TRANSFORMENGINE: Hidden PK — SequenceNo is not in JS visible columns; used as KeyProperty only
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SequenceNo { get; set; }

        // TRANSFORMENGINE: JS column field=jobCode, header=JobCode, width=120; DTO property: JobCode
        [Display(Name = "JobCode")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=animalType, header=AnimalType, width=180; DTO property: AnimalType
        [Display(Name = "AnimalType")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string AnimalType { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=numberOfDays, header=NumberOfDays, width=150; DTO property: NumberOfDays (double)
        [Display(Name = "NumberOfDays")]
        [GridColumn(Width = 150, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double NumberOfDays { get; set; }

        // TRANSFORMENGINE: JS column field=numberOfAnimals, header=NumberOfAnimals, width=160; DTO property: NumberOfAnimals (double)
        [Display(Name = "NumberOfAnimals")]
        [GridColumn(Width = 160, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double NumberOfAnimals { get; set; }

        // TRANSFORMENGINE: JS column field=dateTime, header=Date_Time, width=170; DTO property: DateTime
        [Display(Name = "Date_Time")]
        [GridColumn(Width = 170, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateTime { get; set; }

        // TRANSFORMENGINE: JS column field=userId, header=User_ID, width=150; DTO property: UserId
        [Display(Name = "User_ID")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? UserId { get; set; }

        // TRANSFORMENGINE TODO: UserEmail not in AnimalRequestLogDto — requires backend UserId→email resolution.
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
