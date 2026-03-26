-- View: mabarchive.vfiveyearprojectsummary_sub

CREATE OR REPLACE VIEW mabarchive.vfiveyearprojectsummary_sub AS
 SELECT my_projectmonthfinal.year,
    my_projectmonthfinal.project,
    my_tlkpproject.custincome AS cumbudget,
    sum(my_projectmonthfinal.totalcost) AS cumcost
   FROM (mabarchive.my_projectmonthfinal
     JOIN mabarchive.my_tlkpproject ON (((my_projectmonthfinal.year = my_tlkpproject.year) AND ((my_projectmonthfinal.project)::text = (my_tlkpproject.parentproject)::text))))
  GROUP BY my_projectmonthfinal.year, my_projectmonthfinal.project, my_tlkpproject.custincome
 HAVING (my_projectmonthfinal.year >= 2004);
