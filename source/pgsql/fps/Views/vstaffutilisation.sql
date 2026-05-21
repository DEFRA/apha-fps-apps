CREATE OR REPLACE VIEW fps.vstaffutilisation AS
 SELECT vstaffutilisation_time.workgroup,
    tlkpmonthhours.month,
    tlkpmonthhours.cvlhours AS fthourspermonth,
    vstaffutilisation_time.fthoursperweek,
    vstaffutilisation_time.name,
    vstaffutilisation_time.staffid,
    vstaffutilisation_time.gradecode,
    vstaffutilisation_time.timerecorder,
    vstaffutilisation_time.fpsyear,
    (vstaffutilisation_time.hoursperweek *
        CASE
            WHEN COALESCE(vstaffutilisation_time.startdate::date, '2000-01-01'::date) <= make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) AND COALESCE(vstaffutilisation_time.enddate::date, '2200-01-01'::date) >= (make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) + '1 mon'::interval - '1 day'::interval)::date THEN 1::numeric
            WHEN COALESCE(vstaffutilisation_time.startdate::date, '2000-01-01'::date) > (make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) + '1 mon'::interval - '1 day'::interval)::date OR COALESCE(vstaffutilisation_time.enddate::date, '2200-01-01'::date) < make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) THEN 0::numeric
            ELSE (LEAST(COALESCE(vstaffutilisation_time.enddate::date, '2200-01-01'::date), (make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) + '1 mon'::interval - '1 day'::interval)::date) - GREATEST(COALESCE(vstaffutilisation_time.startdate::date, '2000-01-01'::date), make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1)))::numeric(9,7) / ((make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) + '1 mon'::interval - '1 day'::interval)::date - make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1))::numeric(9,7)
        END::double precision)::numeric(9,2) AS hoursperweek,
    vstaffutilisation_time.hrspaid,
    sum(
        CASE vtimecostcalcs_allstaff.project
            WHEN 'ZTLeave'::text THEN 0::double precision
            WHEN 'ZTWork'::text THEN 0::double precision
            ELSE vtimecostcalcs_allstaff."time"
        END) AS chargedhours,
    sum(
        CASE vtimecostcalcs_allstaff.project
            WHEN 'ZTLeave'::text THEN vtimecostcalcs_allstaff."time"
            ELSE 0::numeric::double precision
        END) AS ztleave,
    sum(
        CASE vtimecostcalcs_allstaff.project
            WHEN 'ZTWork'::text THEN vtimecostcalcs_allstaff."time"
            ELSE 0::numeric::double precision
        END) AS ztwork
   FROM fps.tlkpmonthhours
     LEFT JOIN fps.vtimecostcalcs_allstaff ON tlkpmonthhours.fmonth::double precision = vtimecostcalcs_allstaff.month AND tlkpmonthhours.fpsyear = vtimecostcalcs_allstaff.fpsyear
     RIGHT JOIN fps.vstaffutilisation_time ON vtimecostcalcs_allstaff.staffid::text = vstaffutilisation_time.staffid::text AND vtimecostcalcs_allstaff.fpsyear = vstaffutilisation_time.fpsyear
  GROUP BY vstaffutilisation_time.workgroup, tlkpmonthhours.month, tlkpmonthhours.cvlhours, vstaffutilisation_time.name, vstaffutilisation_time.gradecode, vstaffutilisation_time.hrspaid, vstaffutilisation_time.timerecorder, vstaffutilisation_time.fthoursperweek, vstaffutilisation_time.staffid, vstaffutilisation_time.fpsyear, ((vstaffutilisation_time.hoursperweek *
        CASE
            WHEN COALESCE(vstaffutilisation_time.startdate::date, '2000-01-01'::date) <= make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) AND COALESCE(vstaffutilisation_time.enddate::date, '2200-01-01'::date) >= (make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) + '1 mon'::interval - '1 day'::interval)::date THEN 1::numeric
            WHEN COALESCE(vstaffutilisation_time.startdate::date, '2000-01-01'::date) > (make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) + '1 mon'::interval - '1 day'::interval)::date OR COALESCE(vstaffutilisation_time.enddate::date, '2200-01-01'::date) < make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) THEN 0::numeric
            ELSE (LEAST(COALESCE(vstaffutilisation_time.enddate::date, '2200-01-01'::date), (make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) + '1 mon'::interval - '1 day'::interval)::date) - GREATEST(COALESCE(vstaffutilisation_time.startdate::date, '2000-01-01'::date), make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1)))::numeric(9,7) / ((make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1) + '1 mon'::interval - '1 day'::interval)::date - make_date(tlkpmonthhours.year::integer, tlkpmonthhours.month::integer, 1))::numeric(9,7)
        END::double precision)::numeric(9,2));
