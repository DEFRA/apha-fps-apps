-- 14_log_recreate_summaries.sql
-- Replaces: usp_LogRecreateSummaries @Month
-- Parameters:
--   :userId  (varchar) — triggering user, replaces sp_Get_SP_No / SYSTEM_USER (Phase 7)
--   :month   (smallint)
-- Syntax changes:
--   dbo.RecreateSummaries_Log -> fps.recreatesummaries_log
--   getdate()                 -> CURRENT_TIMESTAMP

INSERT INTO fps.recreatesummaries_log (userid, period, datedone)
VALUES (:userId, :month, CURRENT_TIMESTAMP);
