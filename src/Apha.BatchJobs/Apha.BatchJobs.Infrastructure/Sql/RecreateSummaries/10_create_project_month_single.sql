-- 10_create_project_month_single.sql
-- Replaces: sp_qryJobMonth_Single
-- Syntax changes:
--   dbo.* / [Table] -> fps.* (lower-case)
--   ISNULL(x, 0)    -> COALESCE(x, 0)
-- No formula changes.

INSERT INTO fps.projectmonth2 (
    project,
    monthno,
    costprofile,
    subcontracts,
    animals,
    nonanimal,
    timecosts,
    transfercosts,
    totalcost,
    invoices,
    coiw,
    sumofcostprofile,
    portsales,
    mstoneddue,
    due__done,
    ontime,
    totalhours,
    paycosts
)
SELECT
    projectmonth.project,
    projectmonth.monthno,
    projectmonth.costprofile,
    CASE WHEN total     IS NULL THEN 0 ELSE total     END AS subcontracts,
    CASE WHEN animals   IS NULL THEN 0 ELSE animals   END AS animals,
    CASE WHEN other     IS NULL THEN 0 ELSE other     END AS nonanimal,
    CASE WHEN sumofcost IS NULL THEN 0 ELSE sumofcost END AS timecosts,
    CASE WHEN sumoftransfercost IS NULL THEN 0 ELSE sumoftransfercost END AS transfercosts,
    (COALESCE(total, 0) + COALESCE(sumofcost, 0) + COALESCE(sumoftransfercost, 0)) AS totalcost,
    CASE WHEN sumofamount1 IS NULL THEN 0 ELSE sumofamount1 END AS invoices,
    CASE WHEN workcost     IS NULL THEN 0 ELSE workcost     END AS coiw,
    qryjobmonth_totprofile.sumofcostprofile,
    CASE WHEN fee          IS NULL THEN 0 ELSE fee          END AS portsales,
    qryjobmonthmilestone.mstoneddue,
    qryjobmonthmilestone.due__done,
    qryjobmonthmilestone.ontime,
    CASE WHEN sumofhours   IS NULL THEN 0 ELSE sumofhours   END AS totalhours,
    CASE WHEN sumofpayrate IS NULL THEN 0 ELSE sumofpayrate END AS paycosts

FROM ((((((fps.projectmonth
LEFT JOIN fps.qryjobmonth_subcontracts
    ON  projectmonth.monthno  = qryjobmonth_subcontracts.month
    AND projectmonth.project  = qryjobmonth_subcontracts.project)
LEFT JOIN fps.qryjobmonth_time
    ON  projectmonth.monthno  = qryjobmonth_time.month
    AND projectmonth.project  = qryjobmonth_time.project)
LEFT JOIN fps.qryjobmonthmilestone
    ON  projectmonth.monthno  = qryjobmonthmilestone.duemonth
    AND projectmonth.project  = qryjobmonthmilestone.project)
LEFT JOIN fps.qryjobmonth_transferstotal
    ON  projectmonth.monthno  = qryjobmonth_transferstotal.month
    AND projectmonth.project  = qryjobmonth_transferstotal.project)
LEFT JOIN fps.qryjobmonth_invoices
    ON  projectmonth.monthno  = qryjobmonth_invoices.month
    AND projectmonth.project  = qryjobmonth_invoices.projectparent)
LEFT JOIN fps.qryjobmonthportfoliosales
    ON  projectmonth.monthno  = qryjobmonthportfoliosales.month
    AND projectmonth.project  = qryjobmonthportfoliosales.planportfolio)
LEFT JOIN fps.qryjobmonth_totprofile
    ON  projectmonth.project  = qryjobmonth_totprofile.project;
