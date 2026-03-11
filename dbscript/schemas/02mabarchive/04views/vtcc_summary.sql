-- View: mabarchive.vtcc_summary

CREATE OR REPLACE VIEW mabarchive.vtcc_summary AS
 SELECT year,
    project,
    month,
    sum(pay) AS pay,
    sum(nonpay) AS nonpay,
    sum(overhead) AS overhead
   FROM mabarchive.my_timecostcalcs
  GROUP BY year, project, month;
