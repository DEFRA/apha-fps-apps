CREATE OR REPLACE VIEW mabarchive.vfiveyearprojectsummary_sub2 AS
 SELECT my_tlkpproject.parentproject AS project,
    my_tlkpproject.year,
    (my_tlkpproject.year::character(4)::text || '/'::text) || "right"(((my_tlkpproject.year + 1))::character(4)::text, 2) AS displayyear,
    my_tlkpproject.custincome,
    my_projectmonthfinal.cumcost AS vlaexpeniture,
    my_tlkpproject.custincome - my_projectmonthfinal.cumcost AS incomelesscost,
    my_projectmonthfinal.cuminvoices AS invoicedincome,
    my_projectmonthfinal.cuminvoices - my_projectmonthfinal.cumcost AS invoiceslesscost,
    my_tlkpproject.budget_cvl AS budget,
    my_tlkpproject.budget_cvl - my_projectmonthfinal.cumcost AS budgetremaining
   FROM mabarchive.my_projectmonthfinal
     JOIN mabarchive.my_tlkpproject ON my_projectmonthfinal.year = my_tlkpproject.year AND my_projectmonthfinal.project::text = my_tlkpproject.parentproject::text
     JOIN ( SELECT my_projectmonthfinal_1.year,
            max(my_projectmonthfinal_1.monthno) AS latestmonth
           FROM mabarchive.my_projectmonthfinal my_projectmonthfinal_1
          WHERE my_projectmonthfinal_1.cumflag = 1::double precision
          GROUP BY my_projectmonthfinal_1.year) l ON my_projectmonthfinal.year = l.year AND my_projectmonthfinal.monthno = l.latestmonth
     CROSS JOIN mabarchive.vlatestmonthyear
  WHERE my_tlkpproject.year >= (vlatestmonthyear.year - 5) AND my_tlkpproject.year <=
        CASE
            WHEN "right"(my_tlkpproject.program::text, 4) = '_Res'::text THEN vlatestmonthyear.year - 1
            WHEN "right"(my_tlkpproject.program::text, 5) = '_SURV'::text THEN vlatestmonthyear.year - 1
            WHEN my_tlkpproject.program::text = 'OM_WORK'::text THEN vlatestmonthyear.year - 1
            ELSE vlatestmonthyear.year
        END;
