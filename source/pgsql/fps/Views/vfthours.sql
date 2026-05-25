-- View: fps.vfthours
CREATE OR REPLACE VIEW "fps"."vfthours" AS
SELECT (1.0 * ( SELECT (tblsettings.setting)::numeric AS setting
