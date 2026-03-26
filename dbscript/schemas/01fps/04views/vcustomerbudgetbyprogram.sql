-- View: fps.vcustomerbudgetbyprogram

CREATE OR REPLACE VIEW fps.vcustomerbudgetbyprogram AS
 SELECT program,
    projectstatus,
    customer,
    sum(budget_cvl) AS customerbudget
   FROM fps.tlkpproject
  GROUP BY program, projectstatus, customer;
