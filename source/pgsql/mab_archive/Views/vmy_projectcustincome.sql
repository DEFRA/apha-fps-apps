-- View: mabarchive.vmy_projectcustincome
CREATE OR REPLACE VIEW "mabarchive"."vmy_projectcustincome" AS
SELECT COALESCE(pims.year, fps.year) AS year,
