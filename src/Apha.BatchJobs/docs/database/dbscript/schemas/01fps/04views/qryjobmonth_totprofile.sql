-- View: fps.qryjobmonth_totprofile

CREATE OR REPLACE VIEW fps.qryjobmonth_totprofile AS
 SELECT DISTINCT project,
    sum(costprofile) AS sumofcostprofile
   FROM fps.projectmonth
  GROUP BY project;
