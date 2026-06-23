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
    (profitcentregrade.ohr * sum(vstaffjobhours.plannedhours)) AS contribution,
    tblwgemployee.fpsyear,
    tblusers.user_id,
    tblusers.dt2username,
    tblusers.useremail
   FROM fps.tblkpprofitcentre
     JOIN fps.tbluser_profitcentre ON (((tblkpprofitcentre.profitcentre)::text = (tbluser_profitcentre.profitcentre)::text))
     JOIN fps.tblusers ON ((tbluser_profitcentre.user_id = tblusers.user_id))
     JOIN fps.profitcentregrade ON (((tblkpprofitcentre.profitcentre)::text = (profitcentregrade.profitcentre)::text))
     LEFT JOIN fps.vqrytbidsum ON (((profitcentregrade.profitcentre)::text = (vqrytbidsum.profitcentre)::text) AND (profitcentregrade.fpsyear = vqrytbidsum.fpsyear) AND (tblusers.user_id = vqrytbidsum.user_id))
     JOIN fps.workgroupgrade ON (((profitcentregrade.pcgrade)::text = (workgroupgrade.profitcentregrade)::text) AND (profitcentregrade.fpsyear = workgroupgrade.fpsyear))
     JOIN fps.tblwgemployee ON (((workgroupgrade.wggrade)::text = (tblwgemployee.workgroupgrade)::text) AND (workgroupgrade.fpsyear = tblwgemployee.fpsyear))
     LEFT JOIN fps.vapphours ON (((workgroupgrade.wggrade)::text = (vapphours.workgroupgrade)::text) AND (workgroupgrade.fpsyear = vapphours.fpsyear))
     LEFT JOIN fps.vstaffjobhours ON (((tblwgemployee.pactid)::text = (vstaffjobhours.staffid)::text) AND (tblwgemployee.fpsyear = vstaffjobhours.fpsyear))
  GROUP BY tblkpprofitcentre.conttarget, profitcentregrade.profitcentre, profitcentregrade.chargerate, profitcentregrade.ohr, vqrytbidsum.sumofgenbid, workgroupgrade.workgroup, workgroupgrade.profitcentregrade, workgroupgrade.wggrade, vapphours.sumofplannedhours, tblwgemployee.fpsyear, tblusers.user_id, tblusers.dt2username, tblusers.useremail;
