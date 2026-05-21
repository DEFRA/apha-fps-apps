CREATE OR REPLACE VIEW mabarchive.vlatestprojectyear AS
 SELECT parentproject,
    max(year) AS year
   FROM mabarchive.my_tlkpproject
  GROUP BY parentproject;
