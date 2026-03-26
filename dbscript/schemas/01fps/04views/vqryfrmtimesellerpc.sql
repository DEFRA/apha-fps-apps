-- View: fps.vqryfrmtimesellerpc

CREATE OR REPLACE VIEW fps.vqryfrmtimesellerpc AS
 SELECT tblkpprofitcentre.conttarget,
    profitcentregrade.profitcentre AS sellingpc,
    profitcentregrade.chargerate,
    profitcentregrade.ohr,
    vqrytbidsum.sumofgenbid,
    workgroupgrade.workgroup,
    workgroupgrade.profitcentregrade,
    workgroupgrade.wggrade,
    vapphours.sumofplannedhours AS apphours,
    sum(vstaffjobhours.plannedhours) AS hrs,
    sum(tblwgemployee.hrsavail) AS avhrs,
    (sum(vstaffjobhours.plannedhours) * profitcentregrade.chargerate) AS fec,
    (vapphours.sumofplannedhours * profitcentregrade.chargerate) AS appfec,
    (profitcentregrade.ohr * sum(vstaffjobhours.plannedhours)) AS contribution
   FROM ((fps.vapphours
     RIGHT JOIN (((fps.tblkpprofitcentre
     JOIN (fps.profitcentregrade
     LEFT JOIN fps.vqrytbidsum ON (((profitcentregrade.profitcentre)::text = (vqrytbidsum.profitcentre)::text))) ON (((tblkpprofitcentre.profitcentre)::text = (profitcentregrade.profitcentre)::text)))
     JOIN fps.workgroupgrade ON (((profitcentregrade.pcgrade)::text = (workgroupgrade.profitcentregrade)::text)))
     JOIN fps.tblwgemployee ON (((workgroupgrade.wggrade)::text = (tblwgemployee.workgroupgrade)::text))) ON (((vapphours.workgroupgrade)::text = (workgroupgrade.wggrade)::text)))
     LEFT JOIN fps.vstaffjobhours ON (((tblwgemployee.pactid)::text = (vstaffjobhours.staffid)::text)))
  WHERE ((tblkpprofitcentre.profitcentre)::text IN ( SELECT tbluser_profitcentre.profitcentre
           FROM fps.tbluser_profitcentre
          WHERE (tbluser_profitcentre.user_id IN ( SELECT tblusers.user_id
                   FROM fps.tblusers
                  WHERE ((tblusers.dt2username)::text = CURRENT_USER)))))
  GROUP BY tblkpprofitcentre.conttarget, profitcentregrade.profitcentre, profitcentregrade.chargerate, profitcentregrade.ohr, vqrytbidsum.sumofgenbid, workgroupgrade.workgroup, workgroupgrade.profitcentregrade, workgroupgrade.wggrade, vapphours.sumofplannedhours;
