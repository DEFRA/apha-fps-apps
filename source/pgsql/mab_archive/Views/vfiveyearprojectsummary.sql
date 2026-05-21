CREATE OR REPLACE VIEW mabarchive.vfiveyearprojectsummary AS
 SELECT sub2.project,
    sub2.year,
    sub2.displayyear,
    sub2.custincome,
    sub2.vlaexpeniture,
    sub2.incomelesscost,
    sub2.invoicedincome,
    sub2.invoiceslesscost,
    sub2.budget,
    sub2.budgetremaining,
    sum(sub.cumbudget) AS cumbudget,
    sum(sub.cumcost) AS cumcost
   FROM mabarchive.vfiveyearprojectsummary_sub2 sub2
     JOIN mabarchive.vfiveyearprojectsummary_sub sub ON sub2.project::text = sub.project::text AND sub2.year >= sub.year
  GROUP BY sub2.project, sub2.year, sub2.displayyear, sub2.custincome, sub2.vlaexpeniture, sub2.incomelesscost, sub2.invoicedincome, sub2.invoiceslesscost, sub2.budget, sub2.budgetremaining;
