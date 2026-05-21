CREATE OR REPLACE VIEW fps.qryjobmonthmilestone AS
 SELECT project,
    duemonth,
    count(milestoneref) AS mstonedue,
    sum(completeflag) AS due__done,
    sum(ontimeflag) AS ontime
   FROM fps.qrymilestone1
  GROUP BY project, duemonth;
