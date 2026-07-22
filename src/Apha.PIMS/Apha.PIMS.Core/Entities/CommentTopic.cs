/*
 * TRANSFORMENGINE MIGRATION — CommentTopic.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access / SQL Server lookup table `mabarchive.tlkpcommenttopics` → C# entity class
 *   - Single-column PK `topic` (varchar 25) exposed as `required string Topic`
 *
 * PRESERVED:
 *   - Column name and NOT NULL constraint preserved via `required` modifier
 *   - EF column mapping handled in CommentTopicMap.cs (infrastructure layer)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
namespace Apha.PIMS.Core.Entities
{
    public class CommentTopic
    {
        public required string Topic { get; set; }
    }
}