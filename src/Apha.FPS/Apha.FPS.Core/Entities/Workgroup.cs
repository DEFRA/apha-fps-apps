/*
 * TRANSFORMENGINE MIGRATION — Workgroup.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Annotation header added; no structural changes required (entity was pre-existing and complete)
 *
 * PRESERVED:
 *   - All entity properties mapped from fps.workgroup DDL:
 *     WorkGroupName (workgroup PK), ProfitCentre, CostCentre (double?), Owner,
 *     Description, CentralOverhead (decimal? money), SysTimestamp, SendEmail,
 *     Cos90, CostCentreOld, EmailRecipient, FpsYear (partition key / HasQueryFilter)
 *   - Composite primary key: WorkGroupName + FpsYear (matches pk_workgroup constraint)
 *   - All nullable annotations preserved as-is from DDL nullability
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: SysTimestamp property is present on the entity but has no
 *     corresponding column in the fps.workgroup DDL and is not mapped in WorkgroupMap.cs —
 *     verify whether this column exists in older partitions or can be safely removed
 */
using System;
using System.Collections.Generic;

namespace Apha.FPS.Core.Entities
{
    public partial class Workgroup
    {
        public string WorkGroupName { get; set; } = null!;

        public string ProfitCentre { get; set; } = null!;

        public double? CostCentre { get; set; }

        public string? Owner { get; set; }

        public string? Description { get; set; }

        public decimal? CentralOverhead { get; set; }

        public DateTime? SysTimestamp { get; set; }

        public short? SendEmail { get; set; }

        public short? Cos90 { get; set; }

        public double? CostCentreOld { get; set; }

        public string? EmailRecipient { get; set; }

        public int? FpsYear { get; set; }
    }
}