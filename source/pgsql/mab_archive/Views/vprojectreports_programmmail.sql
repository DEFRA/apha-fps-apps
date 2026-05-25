-- View: mabarchive.vprojectreports_programmmail
CREATE OR REPLACE VIEW "mabarchive"."vprojectreports_programmmail" AS
SELECT (((((((('<a href="'::text || (root.setting)::text) || '/'::text) || (sq.program)::text) || '_'::text) || (prepname.setting)::text) || '">'::text) || (sq.program)::text) || '</a><br> '::text) AS hlink,
