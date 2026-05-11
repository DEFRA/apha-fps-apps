-- 05_create_time_cost_calcs.sql
-- Replaces: sp_CreateTimeCostCalcs
-- Syntax changes:
--   dbo.* / [Table]  -> fps.* (lower-case)
--   ISNULL(x, 0)     -> COALESCE(x, 0)   (none present in this body)
--   [ColumnName]     -> columnname
-- No formula changes.

INSERT INTO fps.timecostcalcs (
    workgroup,
    jobcode,
    project,
    month,
    staffid,
    gradecode,
    name,
    chargerate,
    class,
    time,
    cost,
    division,
    pay,
    nonpay,
    overhead
)
SELECT DISTINCT
    workgroupgrade.workgroup,
    monthlytime.timecode         AS jobcode,
    timecodevalid.parentproject  AS project,
    monthlytime.month,
    vpacttblstaff.pactid         AS staffid,
    workgroupgrade.gradecode,
    vpacttblstaff.name,
    CASE tlkpproject.isdefraproject
        WHEN 0 THEN profitcentregrade.chargerate
        ELSE        profitcentregrade.defrachargerate
    END AS chargerate,
    CASE
        WHEN tlkpprogram.sector_name = 'Charge' THEN 'Charge'
        ELSE 'Free'
    END AS class,
    monthlytime.hours            AS time,
    CASE
        WHEN tlkpprogram.sector_name = 'Charge' THEN hours
        ELSE 0
    END *
    CASE tlkpproject.isdefraproject
        WHEN 0 THEN profitcentregrade.chargerate
        ELSE        profitcentregrade.defrachargerate
    END AS cost,
    fps.tblkpprofitcentre.division,
    monthlytime.hours * profitcentregrade.payrate AS pay,
    monthlytime.hours * profitcentregrade.npr     AS nonpay,
    monthlytime.hours * profitcentregrade.ohr     AS overhead

FROM (((fps.tblkpprofitcentre
    INNER JOIN fps.profitcentregrade
        ON fps.tblkpprofitcentre.profitcentre = profitcentregrade.profitcentre)
    INNER JOIN fps.workgroupgrade
        ON profitcentregrade.pcgrade = workgroupgrade.profitcentregrade)
    INNER JOIN (fps.timecodevalid
    INNER JOIN (fps.vpacttblstaff
    INNER JOIN fps.monthlytime
        ON vpacttblstaff.pactid = monthlytime.pactstaffid)
        ON (timecodevalid.workgroup     = monthlytime.workgroup)
        AND (timecodevalid.timecode     = monthlytime.timecode)
        AND (timecodevalid.parentproject = monthlytime.parentproject))
        ON workgroupgrade.wggrade = vpacttblstaff.workgroupgrade)
    INNER JOIN fps.tlkpproject  ON timecodevalid.parentproject = tlkpproject.parentproject
    INNER JOIN fps.tlkpprogram  ON tlkpprogram.programno       = tlkpproject.program;
