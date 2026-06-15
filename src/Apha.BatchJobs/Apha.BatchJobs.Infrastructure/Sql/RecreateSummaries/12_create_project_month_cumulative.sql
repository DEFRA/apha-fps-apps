-- 12_create_project_month_cumulative.sql
-- Replaces: sp_qryJobMonthCum
-- Syntax changes:
--   dbo.* / [Table] -> fps.* (lower-case)
-- No formula changes.

INSERT INTO fps.projectmonth3 (
    endperiod,
    periodname,
    project,
    cumcost,
    cuminvoices,
    cumcoiw,
    cumportsales,
    cumprofile,
    sumofcostprofile,
    sumofmstonedue,
    sumofdue__done,
    sumofontime,
    cumcwdebit,
    cumcwcredit,
    cumtotalhours,
    cumsubcontracts,
    cumtestcosts,
    cumpaycosts
)
SELECT DISTINCT
    tblperiod.endperiod,
    tblperiod.periodname,
    projectmonth2.project,
    SUM(projectmonth2.totalcost)           AS cumcost,
    SUM(projectmonth2.invoices)            AS cuminvoices,
    SUM(projectmonth2.coiw)                AS cumcoiw,
    SUM(projectmonth2.portsales)           AS cumportsales,
    SUM(projectmonth2.costprofile)         AS cumprofile,
    projectmonth2.sumofcostprofile,
    SUM(projectmonth2.mstonedue)           AS sumofmstonedue,
    SUM(projectmonth2.due__done)           AS sumofdue__done,
    SUM(projectmonth2.ontime)              AS sumofontime,
    SUM(projectmonthcasework.cwdebit)::numeric::money   AS cumcwdebit,
    SUM(projectmonthcasework.cwcredit)::numeric::money  AS cumcwcredit,
    SUM(projectmonth2.totalhours)          AS cumtotalhours,
    SUM(projectmonth2.subcontracts::numeric::double precision) AS cumsubcontracts,
    SUM(projectmonth2.transfercosts)       AS cumtestcosts,
    SUM(projectmonth2.paycosts)            AS cumpaycosts

FROM (fps.tblperiod
    INNER JOIN fps.tblkperiodmonth
        ON tblperiod.periodname = tblkperiodmonth.periodname)
    INNER JOIN fps.projectmonth2
        ON tblkperiodmonth.monthno = projectmonth2.monthno
    INNER JOIN fps.projectmonthcasework
        ON  projectmonth2.monthno = projectmonthcasework.monthno
        AND projectmonth2.project = projectmonthcasework.project

GROUP BY
    tblperiod.endperiod,
    tblperiod.periodname,
    projectmonth2.project,
    projectmonth2.sumofcostprofile;
