-- 01_delete_fps_totals.sql
-- Replaces: sp_deleteFPSTotals
-- Syntax changes: dbo.FPSYearTotals -> fps.fpsyeartotals

DELETE FROM fps.fpsyeartotals;
