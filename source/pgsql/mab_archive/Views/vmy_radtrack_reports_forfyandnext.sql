CREATE OR REPLACE VIEW mabarchive.vmy_radtrack_reports_forfyandnext AS
 SELECT my_radtrack_reports.year,
    my_radtrack_reports.project,
    my_radtrack_reports.type,
    my_radtrack_reports.reminder1,
    my_radtrack_reports.reminder2,
    my_radtrack_reports.replyreceived,
    my_radtrack_reports.senttoprogmanager,
    my_radtrack_reports.senttoprojleader,
    my_radtrack_reports.emailedtocustomer,
    my_radtrack_reports.signedcopytocustomer,
    my_radtrack_reports.repduedate,
    my_radtrack_reports.id,
        CASE
            WHEN my_radtrack_reports.emailedtocustomer IS NULL THEN NULL::text
            WHEN my_radtrack_reports.repduedate IS NULL THEN NULL::text
            WHEN my_radtrack_reports.emailedtocustomer <= my_radtrack_reports.repduedate THEN 'Yes'::text
            ELSE 'No'::text
        END AS ontime
   FROM mabarchive.vlatestmonthyear
     CROSS JOIN mabarchive.my_radtrack_reports
  WHERE vlatestmonthyear.year = my_radtrack_reports.year OR (vlatestmonthyear.year + 1) = my_radtrack_reports.year;
