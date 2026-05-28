CREATE OR REPLACE VIEW mabarchive.vprojectreports_pmmilestonemail AS
 SELECT hlink,
    projectmanager,
    mnumber,
    parentproject,
    email,
    editlink,
    year,
    disable
   FROM mabarchive.vprojectreports_pmmail
  WHERE (EXISTS ( SELECT tblmilestone.project,
            tblmilestone.number,
            tblmilestone.description,
            tblmilestone.datedue,
            tblmilestone.datecompleted,
            tblmilestone.dateformreceived,
            tblmilestone.undersdreview,
            tblmilestone.ontarget,
            tblmilestone.projectleadercomment,
            tblmilestone.capscomment,
            tblmilestone.idtype
           FROM mabarchive.tblmilestone
          WHERE vprojectreports_pmmail.parentproject::text = tblmilestone.project::text AND vprojectreports_pmmail.year::numeric = EXTRACT(year FROM tblmilestone.datedue)));
