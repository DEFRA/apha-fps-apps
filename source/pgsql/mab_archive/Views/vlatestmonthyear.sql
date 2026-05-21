CREATE OR REPLACE VIEW mabarchive.vlatestmonthyear AS
 SELECT tlkpyear.year,
    tlkpyear.latestmonthreleased,
        CASE
            WHEN tlkpyear.latestmonthreleased = 1 THEN 'April '::text || tlkpyear.year::character(4)::text
            WHEN tlkpyear.latestmonthreleased < 10 THEN (('April - '::text || tlkpmonths.monthname::text) || ' '::text) || tlkpyear.year::character(4)::text
            ELSE (((('April '::text || tlkpyear.year::character(4)::text) || ' - '::text) || tlkpmonths.monthname::text) || ' '::text) || ((tlkpyear.year + 1))::character(4)::text
        END AS period
   FROM mabarchive.tlkpyear
     JOIN mabarchive.tlkpmonths ON tlkpyear.latestmonthreleased = tlkpmonths.fmonthno
  WHERE tlkpyear.year = (( SELECT max(tlkpyear_1.year) AS expr1
           FROM mabarchive.tlkpyear tlkpyear_1
          WHERE tlkpyear_1.latestmonthreleased > 0));
