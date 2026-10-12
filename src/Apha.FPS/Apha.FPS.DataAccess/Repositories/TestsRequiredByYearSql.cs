namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Year-specific equivalent of fps.vqrytestsrequiredbywg_export / fps.vqrytestsrequiredbyrc_export.
    /// The views always use the latest released year; here the FPS year is supplied as a parameter.
    /// {0} = FPS year, {1} = resource centre (profit centre) filter, empty string for all.
    /// </summary>
    public static class TestsRequiredByYearSql
    {
        private const string WgExportCte = @"
WITH params AS (
    SELECT {0}::integer AS fpsyear, COALESCE({1}::text, '') AS pc
), release_window AS (
    -- Last 12 released months (Access: fnOneYearBackFromMonthReleased .. fnLatestYearMonthReleased)
    SELECT y.year * 100 + y.latestmonthreleased AS latest_ym,
           CASE WHEN y.latestmonthreleased = 12 THEN y.year * 100 + 1
                ELSE (y.year - 1) * 100 + (y.latestmonthreleased + 1) END AS from_ym
    FROM mabarchive.tlkpyear y
    CROSS JOIN params p
    WHERE y.latestmonthreleased IS NOT NULL
      AND y.year <= p.fpsyear
    ORDER BY y.year DESC
    LIMIT 1
), monthly_output AS (
    SELECT upper(trim(mo.testcode::text)) AS testcode,
           upper(trim(mo.workgroup::text)) AS workgroup,
           mo.volume
    FROM mabarchive.my_monthlyoutput mo
    CROSS JOIN release_window rw
    WHERE (mo.year::integer * 100 + mo.month::integer) BETWEEN rw.from_ym AND rw.latest_ym
), last_year_tests AS (
    -- qryLastYearsTests
    SELECT testcode, sum(volume) AS yeartotal
    FROM monthly_output
    GROUP BY testcode
), last_year_wg_tests AS (
    -- qryLastYearsWGTests
    SELECT testcode, workgroup, sum(volume) AS testsbywg
    FROM monthly_output
    GROUP BY testcode, workgroup
), year_workgroups AS (
    SELECT upper(trim(w.workgroup::text)) AS wgkey,
           w.workgroup::text AS workgroup,
           w.profitcentre::text AS profitcentre
    FROM fps.workgroup w
    JOIN params p ON w.fpsyear = p.fpsyear
), default_workgroup AS (
    -- DLookUp on qryWGTestCapability (tlkpTestCapability INNER JOIN WorkGroup)
    SELECT upper(trim(tc.testcode::text)) AS testcode,
           yw.wgkey AS workgroup,
           row_number() OVER (PARTITION BY upper(trim(tc.testcode::text)) ORDER BY yw.wgkey) AS rn
    FROM fps.tlkptestcapability tc
    JOIN params p ON tc.fpsyear = p.fpsyear
    JOIN year_workgroups yw ON yw.wgkey = upper(trim(tc.workgroup::text))
), tests_required AS (
    -- qryTestsRequiredThisYear WHERE NoRequired <> 0
    SELECT upper(trim(t.testcode::text)) AS testcode,
           sum(t.norequired) AS norequired,
           min(tp.itemdescription::text) AS itemdescription,
           min(tp.unitpricevla) AS unitpricevla
    FROM fps.tlkptestreqmt t
    JOIN params p ON t.fpsyear = p.fpsyear
    LEFT JOIN fps.testorproduct tp
           ON upper(trim(tp.itemcode::text)) = upper(trim(t.testcode::text))
          AND tp.fpsyear = t.fpsyear
    GROUP BY upper(trim(t.testcode::text))
    HAVING sum(t.norequired) <> 0
), rc_cost AS (
    -- qryTestsRequiredByWG_RCCost
    SELECT upper(trim(rc.testcode::text)) AS testcode,
           yw.wgkey AS workgroup,
           rc.price
    FROM fps.tbltestrccost rc
    JOIN params p ON rc.fpsyear = p.fpsyear
    JOIN year_workgroups yw ON yw.profitcentre = rc.profitcentre::text
), tests_by_wg AS (
    -- qryTestsRequiredByWG
    SELECT COALESCE(lwg.workgroup, dwg.workgroup) AS wg,
           tr.testcode,
           tr.itemdescription,
           COALESCE(rc.price, tr.unitpricevla)::numeric AS unitprice,
           CASE
               WHEN COALESCE(lyt.yeartotal, 0) = 0 THEN round(tr.norequired::numeric)::integer
               ELSE round((tr.norequired * COALESCE(lwg.testsbywg, 0) / lyt.yeartotal + 0.49)::numeric)::integer
           END AS projectedtotal
    FROM tests_required tr
    LEFT JOIN last_year_tests lyt ON lyt.testcode = tr.testcode
    LEFT JOIN last_year_wg_tests lwg ON lwg.testcode = lyt.testcode
    LEFT JOIN rc_cost rc ON rc.testcode = lwg.testcode AND rc.workgroup = lwg.workgroup
    LEFT JOIN default_workgroup dwg ON dwg.testcode = tr.testcode AND dwg.rn = 1
), wg_export AS (
    SELECT yw.profitcentre,
           yw.workgroup,
           t.testcode,
           t.itemdescription,
           t.projectedtotal,
           t.unitprice
    FROM tests_by_wg t
    JOIN year_workgroups yw ON yw.wgkey = t.wg
    CROSS JOIN params p
    WHERE yw.profitcentre IS NOT NULL
      AND (p.pc = '' OR lower(yw.profitcentre) = lower(p.pc))
)";

        public const string ByWorkGroup = WgExportCte + @"
SELECT profitcentre, workgroup, testcode, itemdescription, projectedtotal, unitprice
FROM wg_export
ORDER BY profitcentre, workgroup, testcode";

        public const string ByResourceCentre = WgExportCte + @"
SELECT profitcentre,
       testcode,
       itemdescription,
       sum(projectedtotal)::integer AS projectedtotal,
       unitprice
FROM wg_export
GROUP BY profitcentre, testcode, itemdescription, unitprice
ORDER BY profitcentre, testcode";
    }
}
