-- View: mabarchive.vpactprojectyearcosts

CREATE OR REPLACE VIEW mabarchive.vpactprojectyearcosts AS
 SELECT my_projectmonthfinal.project,
        CASE g_tlkpproject_radtrackdata.useprojectyear
            WHEN '-1'::integer THEN date_part('year'::text, (date_trunc('month'::text, (g_tlkpproject_radtrackdata.startdate)::timestamp with time zone) + (((((my_projectmonthfinal.monthno + (3)::double precision) - date_part('month'::text, g_tlkpproject_radtrackdata.startdate)))::integer)::double precision * '1 mon'::interval)))
            ELSE (my_projectmonthfinal.year)::double precision
        END AS year,
    my_projectmonthfinal.monthno,
    sum(my_projectmonthfinal.subcontracts) AS subcontracts,
    sum(my_projectmonthfinal.animals) AS animals,
    sum(my_projectmonthfinal.transfercosts) AS tests,
    sum(vtcc_summary.pay) AS pay,
    sum((vtcc_summary.nonpay + vtcc_summary.overhead)) AS nonpayoh,
    sum(my_projectmonthfinal.totalhours) AS hours,
    sum(my_projectmonthfinal.totalcost) AS totalcosts,
    sum(my_projectmonthfinal.timecosts) AS timecost
   FROM ((mabarchive.my_projectmonthfinal
     LEFT JOIN mabarchive.g_tlkpproject_radtrackdata ON (((my_projectmonthfinal.project)::text = (g_tlkpproject_radtrackdata.parentproject)::text)))
     LEFT JOIN mabarchive.vtcc_summary ON (((my_projectmonthfinal.year = vtcc_summary.year) AND ((my_projectmonthfinal.project)::text = (vtcc_summary.project)::text) AND (my_projectmonthfinal.monthno = vtcc_summary.month))))
  GROUP BY my_projectmonthfinal.project, my_projectmonthfinal.monthno, g_tlkpproject_radtrackdata.useprojectyear, my_projectmonthfinal.year, g_tlkpproject_radtrackdata.startdate;
