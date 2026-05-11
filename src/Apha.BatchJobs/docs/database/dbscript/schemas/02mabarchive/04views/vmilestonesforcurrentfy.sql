-- View: mabarchive.vmilestonesforcurrentfy

CREATE OR REPLACE VIEW mabarchive.vmilestonesforcurrentfy AS
 SELECT tblmilestone.project,
    tblmilestone.number,
    tblmilestone.description,
    tblmilestone.datedue,
    tblmilestone.datecompleted,
    tblmilestone.dateformreceived,
    tblmilestone.undersdreview,
    tblmilestone.ontarget,
    tblmilestone.projectleadercomment,
    tblmilestone.capscomment,
    tblmilestone.idtype,
        CASE
            WHEN ((vlatestmonthyear.year)::double precision = date_part('year'::text, (tblmilestone.datedue - '3 mons'::interval))) THEN '-1'::integer
            ELSE 0
        END AS inthisfyear
   FROM (mabarchive.tblmilestone
     CROSS JOIN mabarchive.vlatestmonthyear)
  WHERE ((date_part('year'::text, (tblmilestone.datedue - '3 mons'::interval)) = (vlatestmonthyear.year)::double precision) OR (date_part('year'::text, (tblmilestone.datedue - '6 mons'::interval)) = (vlatestmonthyear.year)::double precision));
