-- 13_create_project_month_final.sql
-- Replaces: sp_qryJobMonth_Final @Month
-- Parameter: @month (int) — supplied by CreateProjectMonthFinalStep
-- Syntax changes:
--   dbo.* / [Table] -> fps.* (lower-case)
--   @month          -> :month  (Npgsql named parameter syntax)
-- No formula changes.

INSERT INTO fps.projectmonthfinal (
    project,
    monthno,
    costprofile,
    subcontracts,
    animals,
    nonanimals,
    timecosts,
    transfercosts,
    totalcost,
    invoices,
    coiw,
    portsales,
    cumcost,
    cumprofile,
    periodname,
    sumofcostprofile,
    cuminvoices,
    cumcoiw,
    cumportsales,
    mstonedue,
    due__done,
    ontime,
    sumofmstonedue,
    sumofdue__done,
    sumofontime,
    cumflag,
    cwdebit,
    cwcredit,
    cumcwdebit,
    cumcwcredit,
    totalhours,
    cumtotalhours,
    cumsubcontracts,
    cumtestcosts,
    paycosts,
    cumpaycosts
)
SELECT DISTINCT
    projectmonth2.project,
    projectmonth2.monthno,
    projectmonth2.costprofile,
    projectmonth2.subcontracts,
    projectmonth2.animals,
    projectmonth2.nonanimal,
    projectmonth2.timecosts::numeric::money,
    projectmonth2.transfercosts::numeric::money,
    projectmonth2.totalcost,
    projectmonth2.invoices,
    projectmonth2.coiw,
    projectmonth2.portsales::numeric::money,
    CASE WHEN projectmonth2.monthno <= :month THEN cumcost * 1     ELSE NULL END AS cumcost,
    projectmonth3.cumprofile,
    projectmonth3.periodname,
    projectmonth3.sumofcostprofile,
    CASE WHEN projectmonth2.monthno <= :month THEN cuminvoices * 1 ELSE NULL END AS cuminvoices,
    CASE WHEN projectmonth2.monthno <= :month THEN cumcoiw * 1     ELSE NULL END AS cumcoiw,
    CASE
        WHEN projectmonth2.monthno <= :month THEN cumportsales::numeric::money
        ELSE NULL
    END AS cumportsales,
    projectmonth2.mstonedue,
    projectmonth2.due__done,
    projectmonth2.ontime,
    projectmonth3.sumofmstonedue,
    CASE WHEN projectmonth2.monthno <= :month THEN sumofdue__done * 1 ELSE NULL END AS sumofdue__done,
    CASE WHEN projectmonth2.monthno <= :month THEN sumofontime * 1    ELSE NULL END AS sumofontime,
    CASE WHEN projectmonth2.monthno <= :month THEN 1                  ELSE NULL END AS cumflag,
    CASE
        WHEN projectmonth2.monthno <= :month THEN projectmonthcasework.cwdebit::numeric::money
        ELSE NULL
    END,
    CASE
        WHEN projectmonth2.monthno <= :month THEN projectmonthcasework.cwcredit::numeric::money
        ELSE NULL
    END,
    CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumcwdebit      ELSE NULL END,
    CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumcwcredit     ELSE NULL END,
    projectmonth2.totalhours,
    CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumtotalhours   ELSE NULL END,
    CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumsubcontracts ELSE NULL END,
    CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumtestcosts    ELSE NULL END,
    projectmonth2.paycosts,
    CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumpaycosts     ELSE NULL END

FROM fps.projectmonth2
    INNER JOIN fps.projectmonth3
        ON  projectmonth2.project = projectmonth3.project
        AND projectmonth2.monthno = projectmonth3.endperiod
    INNER JOIN fps.projectmonthcasework
        ON  projectmonth2.project = projectmonthcasework.project
        AND projectmonth2.monthno = projectmonthcasework.monthno;
