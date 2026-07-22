/*
 * TRANSFORMENGINE MIGRATION — CommentDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access form-bound field set (frmtblComments RecordSource columns) → C# DTO for service-layer contracts
 *   - Field `comment` (Access/SQL Server column) surfaced as `CommentText` property (EF column mapping in CommentMap.cs preserves DB name)
 *   - `Year` typed as int? to allow nullable filter/display scenarios (entity uses short; service casts when writing)
 *   - `DateEntered` nullable — trigger-managed server-side on INSERT; client value is discarded in AddAsync
 *
 * PRESERVED:
 *   - All 7 tblcomments column nullability semantics surfaced as nullable properties
 *   - CommentNo (PK) present for read/update/delete routing
 *   - AutoMapper CreateMap<Comment, CommentDto>().ReverseMap() registered in EntityMapper.cs
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: CommentText property name differs from DB column `comment` — verify EF column mapping in CommentMap.cs if renaming ever changes
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Application.Dtos
{
    public class CommentDto
    {
        public int CommentNo { get; set; }
        public string? Project { get; set; }
        public int? Year { get; set; }
        public string? Topic { get; set; }
        public string? CommentText { get; set; }
        public string? MadeBy { get; set; }
        public DateTime? DateEntered { get; set; }
    }
}
