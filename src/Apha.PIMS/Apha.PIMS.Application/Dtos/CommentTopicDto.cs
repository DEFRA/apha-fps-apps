/*
 * TRANSFORMENGINE MIGRATION — CommentTopicDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access RowSource lookup (tlkpCommentTopics combo-box on frmtblComments) → C# DTO for service-layer lookup contracts
 *   - Single-column PK `topic` (varchar 25) surfaced as required string property
 *
 * PRESERVED:
 *   - `required` modifier enforcing NOT NULL constraint from tlkpcommenttopics DDL
 *   - AutoMapper CreateMap<CommentTopic, CommentTopicDto>().ReverseMap() registered in EntityMapper.cs
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
namespace Apha.PIMS.Application.Dtos
{
    public class CommentTopicDto
    {
        public required string Topic { get; set; }
    }
}
