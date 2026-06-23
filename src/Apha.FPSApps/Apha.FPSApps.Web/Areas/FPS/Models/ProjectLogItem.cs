/*
 * TRANSFORMENGINE MIGRATION — ProjectLogItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - New file — read-only DataGrid Item model for the "Project Detail Changes" audit log tab
 *   - 33 visible columns derived from initializeProjectAuditTrailTable() JS columns array
 *     plus one hidden SequenceNo PK (not in JS columns; used as KeyProperty)
 *   - showAddButton: false; no edit/delete buttons → AllowAdd=false, AllowEdit=false, AllowDelete=false
 *   - All columns set to GridColumnType.ReadOnly (or formatting-aware subtypes) — no editing
 *   - Property names match ProjectLogDto exactly for AutoMapper convention mapping
 *
 * PRESERVED:
 *   - JS column order: parentProject, projectTitle, program, customer, manager, transferIncome,
 *     custIncome, wipEc→WipEoy, wipLim→WipLimit, wipC→WipCurrent, projectStatus, costBookNo,
 *     dateCreated, feCost→FecCost, profit, budgetCvl, dateCosted, disease, contract,
 *     projectParent, shortTitle, caseworkSub→CaseWorkSub, pvsIncome, planCaseworkDebit→PlanCaseWorkDebit,
 *     finished, owningRc→OwningRc, comments, carryOver, carryOverSeed, dateTime, userId,
 *     userEmail, insertDelete
 *   - Display labels from JS column header values
 *   - Column widths from JS DataGridComponent columns array
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: UserEmail property is NOT in ProjectLogDto (DTO has UserId only).
 *     AutoMapper must Ignore() this field. It requires either a DTO update to include UserEmail
 *     from ProjectLogRes, or a ForMember lookup at the service boundary.
 *   - TRANSFORMENGINE TODO: FpsViewModelMapper.cs CreateMap<ProjectLogDto, ProjectLogItem>()
 *     stub must be uncommented and ForMember(d => d.UserEmail, o => o.Ignore()) added.
 *   - TRANSFORMENGINE TODO: Finished (short?) display — confirm whether 0/1 renders as Yes/No or raw value.
 */
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Read-only DataGrid row model for the Project Detail Changes audit log tab.
    /// Derives from JS initializeProjectAuditTrailTable() columns array (33 visible columns).
    /// Property names match ProjectLogDto exactly for AutoMapper convention mapping.
    /// </summary>
    public class ProjectLogItem
    {
        // TRANSFORMENGINE: Hidden PK — SequenceNo is not in JS visible columns; used as KeyProperty only
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SequenceNo { get; set; }

        // TRANSFORMENGINE: JS column field=parentProject, header=ParentProject, width=150
        [Display(Name = "ParentProject")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=projectTitle, header=ProjectTitle, width=260
        [Display(Name = "ProjectTitle")]
        [GridColumn(Width = 260, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ProjectTitle { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=program, header=Program, width=120
        [Display(Name = "Program")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Program { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=customer, header=Customer, width=120
        [Display(Name = "Customer")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Customer { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=manager, header=Manager, width=180
        [Display(Name = "Manager")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }

        // TRANSFORMENGINE: JS column field=transferIncome, header=TransferIncome, width=150
        [Display(Name = "TransferIncome")]
        [GridColumn(Width = 150, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TransferIncome { get; set; }

        // TRANSFORMENGINE: JS column field=custIncome, header=CustIncome, width=140
        [Display(Name = "CustIncome")]
        [GridColumn(Width = 140, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal CustIncome { get; set; }

        // TRANSFORMENGINE: JS column field=wipEc, header=WIP_EOY, width=120; DTO property: WipEoy
        [Display(Name = "WIP_EOY")]
        [GridColumn(Width = 120, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? WipEoy { get; set; }

        // TRANSFORMENGINE: JS column field=wipLim, header=WIP_Limit, width=120; DTO property: WipLimit
        [Display(Name = "WIP_Limit")]
        [GridColumn(Width = 120, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? WipLimit { get; set; }

        // TRANSFORMENGINE: JS column field=wipC, header=WIP_Current, width=130; DTO property: WipCurrent
        [Display(Name = "WIP_Current")]
        [GridColumn(Width = 130, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? WipCurrent { get; set; }

        // TRANSFORMENGINE: JS column field=projectStatus, header=ProjectStatus, width=130
        [Display(Name = "ProjectStatus")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ProjectStatus { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=costBookNo, header=CostBookNo, width=120
        [Display(Name = "CostBookNo")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? CostBookNo { get; set; }

        // TRANSFORMENGINE: JS column field=dateCreated, header=DateCreated, width=180
        [Display(Name = "DateCreated")]
        [GridColumn(Width = 180, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateCreated { get; set; }

        // TRANSFORMENGINE: JS column field=feCost, header=FECost, width=100; DTO property: FecCost
        [Display(Name = "FECost")]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? FecCost { get; set; }

        // TRANSFORMENGINE: JS column field=profit, header=Profit, width=100
        [Display(Name = "Profit")]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? Profit { get; set; }

        // TRANSFORMENGINE: JS column field=budgetCvl, header=Budget_CVL, width=140
        [Display(Name = "Budget_CVL")]
        [GridColumn(Width = 140, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? BudgetCvl { get; set; }

        // TRANSFORMENGINE: JS column field=dateCosted, header=DateCosted, width=120
        [Display(Name = "DateCosted")]
        [GridColumn(Width = 120, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateCosted { get; set; }

        // TRANSFORMENGINE: JS column field=disease, header=Disease, width=100
        [Display(Name = "Disease")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Disease { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=contract, header=Contract, width=120
        [Display(Name = "Contract")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Contract { get; set; } = null!;

        // TRANSFORMENGINE: JS column field=projectParent, header=ProjectParent, width=140
        [Display(Name = "ProjectParent")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectParent { get; set; }

        // TRANSFORMENGINE: JS column field=shortTitle, header=ShortTitle, width=180
        [Display(Name = "ShortTitle")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ShortTitle { get; set; }

        // TRANSFORMENGINE: JS column field=caseworkSub, header=CaseworkSub, width=140; DTO property: CaseWorkSub
        [Display(Name = "CaseworkSub")]
        [GridColumn(Width = 140, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? CaseWorkSub { get; set; }

        // TRANSFORMENGINE: JS column field=pvsIncome, header=PVSIncome, width=120
        [Display(Name = "PVSIncome")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? PvsIncome { get; set; }

        // TRANSFORMENGINE: JS column field=planCaseworkDebit, header=PlanCaseworkDebit, width=180; DTO property: PlanCaseWorkDebit
        [Display(Name = "PlanCaseworkDebit")]
        [GridColumn(Width = 180, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? PlanCaseWorkDebit { get; set; }

        // TRANSFORMENGINE: JS column field=finished, header=Finished, width=90; DTO property: Finished (short?)
        [Display(Name = "Finished")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public short? Finished { get; set; }

        // TRANSFORMENGINE: JS column field=owningRc, header=OwningRC, width=110; DTO property: OwningRc
        [Display(Name = "OwningRC")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OwningRc { get; set; }

        // TRANSFORMENGINE: JS column field=comments, header=Comments, width=120
        [Display(Name = "Comments")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? Comments { get; set; }

        // TRANSFORMENGINE: JS column field=carryOver, header=CarryOver, width=120
        [Display(Name = "CarryOver")]
        [GridColumn(Width = 120, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? CarryOver { get; set; }

        // TRANSFORMENGINE: JS column field=carryOverSeed, header=CarryOverSeed, width=140
        [Display(Name = "CarryOverSeed")]
        [GridColumn(Width = 140, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? CarryOverSeed { get; set; }

        // TRANSFORMENGINE: JS column field=dateTime, header=Date_Time, width=180; DTO property: DateTime
        [Display(Name = "Date_Time")]
        [GridColumn(Width = 180, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateTime { get; set; }

        // TRANSFORMENGINE: JS column field=userId, header=User_ID, width=160
        [Display(Name = "User_ID")]
        [GridColumn(Width = 160, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? UserId { get; set; }

        // TRANSFORMENGINE TODO: UserEmail is NOT in ProjectLogDto — AutoMapper must Ignore() this.
        // Populated by JS decorateAuditRowsWithEmail(); requires backend/service to resolve from UserId.
        // JS column field=userEmail, header=User_Email, width=240
        [Display(Name = "User_Email")]
        [GridColumn(Width = 240, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? UserEmail { get; set; }

        // TRANSFORMENGINE: JS column field=insertDelete, header=Insert_Delete, width=130
        [Display(Name = "Insert_Delete")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? InsertDelete { get; set; }
    }
}
