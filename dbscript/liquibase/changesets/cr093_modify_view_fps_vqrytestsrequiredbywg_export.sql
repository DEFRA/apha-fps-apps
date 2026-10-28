--liquibase formatted sql

--changeset vishal:CR093 labels:ddl context:all runOnChange:true splitStatements:false

-- View: fps.vqrytestsrequiredbywg_export

CREATE OR REPLACE VIEW fps.vqrytestsrequiredbywg_export
 AS
 WITH latest_release AS (
         SELECT tlkpyear.year,
            tlkpyear.latestmonthreleased,
            tlkpyear.year * 100 + tlkpyear.latestmonthreleased AS latest_ym,
                CASE
                    WHEN tlkpyear.latestmonthreleased = 12 THEN tlkpyear.year * 100 + 1
                    ELSE (tlkpyear.year - 1) * 100 + (tlkpyear.latestmonthreleased + 1)
                END AS from_ym
           FROM mabarchive.tlkpyear
          WHERE tlkpyear.year = (( SELECT max(tlkpyear_1.year) AS max
                   FROM mabarchive.tlkpyear tlkpyear_1
                  WHERE tlkpyear_1.latestmonthreleased IS NOT NULL))
        ), last_year_tests AS (
         SELECT upper(mo.testcode::text) AS testcode,
            sum(mo.volume) AS yeartotal
           FROM mabarchive.my_monthlyoutput mo
             CROSS JOIN latest_release lr_1
          WHERE (mo.year::integer * 100 + mo.month::integer) >= lr_1.from_ym AND (mo.year::integer * 100 + mo.month::integer) <= lr_1.latest_ym
          GROUP BY (upper(mo.testcode::text))
        ), last_year_wg_tests AS (
         SELECT mo.testcode,
            upper(mo.workgroup::text) AS workgroup,
            sum(mo.volume) AS testsbywg
           FROM mabarchive.my_monthlyoutput mo
             CROSS JOIN latest_release lr_1
          WHERE (mo.year::integer * 100 + mo.month::integer) >= lr_1.from_ym AND (mo.year::integer * 100 + mo.month::integer) <= lr_1.latest_ym
          GROUP BY mo.testcode, (upper(mo.workgroup::text))
        ), default_workgroup AS (                        -- DLookUp on qryWGTestCapability
         SELECT upper(tc.testcode::text)  AS testcode,
            upper(wg.workgroup::text) AS workgroup,
            row_number() OVER (PARTITION BY upper(tc.testcode::text)
                               ORDER BY wg.workgroup) AS rn
           FROM fps.tlkptestcapability tc
             JOIN fps.workgroup wg
               ON upper(wg.workgroup::text) = upper(tc.workgroup::text)
             JOIN latest_release lr
               ON tc.fpsyear = lr.year
              AND wg.fpsyear = lr.year
        ), tests_required AS (
         SELECT t_1.testcode,
            sum(t_1.norequired) AS norequired,
            min(p.itemdescription::text) AS itemdescription,
            min(p.unitpricevla) AS unitpricevla,
            lr_1.year
           FROM fps.tlkptestreqmt t_1
             CROSS JOIN latest_release lr_1
             LEFT JOIN fps.testorproduct p ON p.itemcode::text = t_1.testcode::text AND p.fpsyear = t_1.fpsyear
          WHERE t_1.norequired <> 0::double precision AND t_1.fpsyear = lr_1.year
          GROUP BY t_1.testcode, lr_1.year
        ), rc_cost AS (
         SELECT rc.testcode,
            wg_1.workgroup,
            rc.price
           FROM fps.tbltestrccost rc
             JOIN fps.workgroup wg_1 ON wg_1.profitcentre::text = rc.profitcentre::text
             JOIN latest_release lr_1 ON rc.fpsyear = lr_1.year AND wg_1.fpsyear = lr_1.year
        ), tests_by_wg AS (
         SELECT COALESCE(lwg.workgroup, dwg.workgroup::text) AS wg,
            tr.testcode,
            tr.itemdescription,
            COALESCE(rc.price, tr.unitpricevla) AS unitprice,
                CASE
                    WHEN COALESCE(lyt.yeartotal, 0::double precision) = 0::double precision THEN round(tr.norequired::numeric)::integer
                    ELSE round((tr.norequired * COALESCE(lwg.testsbywg, 0::double precision) / NULLIF(lyt.yeartotal, 0::double precision) + 0.49::double precision)::numeric)::integer
                END AS projectedtotal
           FROM tests_required tr
             LEFT JOIN last_year_tests lyt ON lyt.testcode = tr.testcode::text
             LEFT JOIN last_year_wg_tests lwg ON lwg.testcode::text = tr.testcode::text
             LEFT JOIN rc_cost rc ON rc.testcode::text = lwg.testcode::text AND rc.workgroup::text = lwg.workgroup
             LEFT JOIN default_workgroup dwg ON dwg.testcode = tr.testcode::text AND dwg.rn = 1
        )
 SELECT wg.profitcentre, wg.workgroup, t.testcode, t.itemdescription, t.projectedtotal, t.unitprice
   FROM tests_by_wg t
     JOIN latest_release lr ON true
     JOIN fps.workgroup wg ON upper(wg.workgroup::text) = t.wg AND wg.fpsyear = lr.year
  WHERE wg.profitcentre IS NOT NULL
  ORDER BY wg.profitcentre, wg.workgroup, t.testcode;

--rollback DROP VIEW IF EXISTS fps.vqrytestsrequiredbywg_export;
