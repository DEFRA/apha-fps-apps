-- View: mabarchive.vprojectreports_pmmail
CREATE OR REPLACE VIEW "mabarchive"."vprojectreports_pmmail" AS
SELECT (((((((((('<a href="'::text || (root.setting)::text) || '/'::text) || (vcurrent_projectinfo.projectgroup)::text) || '/'::text) || replace((vcurrent_projectinfo.parentproject)::text, '/'::text, '-'::text)) || ' '::text) || (prepname.setting)::text) || '" target="_blank">'::text) || (vcurrent_projectinfo.parentproject)::text) || '</a><br> '::text) AS hlink,
