-- View: mabarchive.vprogramreports_mail
CREATE OR REPLACE VIEW "mabarchive"."vprogramreports_mail" AS
SELECT (((((((('<a href="'::text || (root.setting)::text) || '/'::text) || (sq.program)::text) || '_'::text) || (prepname.setting)::text) || '">'::text) || (sq.program)::text) || '</a><br> '::text) AS hlink,
