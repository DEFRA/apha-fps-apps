-- View: fps.qryjobmonth_transferstotal

CREATE OR REPLACE VIEW fps.qryjobmonth_transferstotal AS
 SELECT DISTINCT project,
    month,
    sum(transfercost) AS sumoftransfercost
   FROM fps.qryjobmonth_transferunion
  GROUP BY project, month;
