-- Function: fps.fnproratapartmonth

CREATE OR REPLACE FUNCTION fps.fnproratapartmonth(p_startdate date, p_enddate date, p_month smallint, p_year smallint)
 RETURNS numeric
 LANGUAGE plpgsql
 IMMUTABLE
AS $function$
DECLARE
    v_startofmonth date;
    v_endofmonth   date;
    v_sd           date;
    v_ed           date;
    v_result       numeric(9,8);
BEGIN
    -- Apply NULL defaults (matching T-SQL ISNULL defaults)
    p_startdate := COALESCE(p_startdate, '2000-01-01'::date);
    p_enddate   := COALESCE(p_enddate,   '2200-01-01'::date);

    v_startofmonth := make_date(p_year::int, p_month::int, 1);
    v_endofmonth   := (v_startofmonth + interval '1 month' - interval '1 day')::date;

    IF p_startdate <= v_startofmonth AND p_enddate >= v_endofmonth THEN
        -- Full month covered
        v_result := 1;

    ELSIF p_startdate > v_endofmonth OR p_enddate < v_startofmonth THEN
        -- No overlap with this month
        v_result := 0;

    ELSE
        -- Partial month: clamp range to [startofmonth, endofmonth]
        v_sd := GREATEST(p_startdate, v_startofmonth);
        v_ed := LEAST(p_enddate, v_endofmonth);

        -- Fraction = days_covered / days_in_month
        -- Date subtraction in PG returns integer days, matching DATEDIFF(day,...)
        v_result := (v_ed - v_sd)::numeric(9,7)
                  / (v_endofmonth - v_startofmonth)::numeric(9,7);
    END IF;

    RETURN v_result;
END;
$function$;
