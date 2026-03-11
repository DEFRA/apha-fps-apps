-- View: fps.vtblwgemployee

CREATE OR REPLACE VIEW fps.vtblwgemployee AS
 SELECT pactid,
    spnumber,
    workgroupgrade,
    personstatus,
    personclass,
    hrspaid,
    leave,
    sickspecial,
    hrsavail,
    makeavailable,
    timerecorder,
    startdate,
    enddate,
    hoursperweek
   FROM fps.tblwgemployee
  WHERE ((workgroupgrade)::text IN ( SELECT vworkgroupgrade.wggrade
           FROM fps.vworkgroupgrade));
