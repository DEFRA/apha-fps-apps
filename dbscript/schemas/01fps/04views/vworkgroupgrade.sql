-- View: fps.vworkgroupgrade

CREATE OR REPLACE VIEW fps.vworkgroupgrade AS
 SELECT wggrade,
    profitcentregrade,
    gradecode,
    workgroup,
    chargeratewg,
    directratewg,
    payratewg,
    nprwg,
    ohrwg,
    avsalary,
    hrschangedby
   FROM fps.workgroupgrade
  WHERE ((workgroup)::text IN ( SELECT vworkgroup.workgroup
           FROM fps.vworkgroup));
