-- View: fps.vtbladditionalcosts_bygroup

CREATE OR REPLACE VIEW fps.vtbladditionalcosts_bygroup AS
 SELECT jobcode,
    account,
    description,
    itemcost,
    freq,
    supplier
   FROM fps.tbladditionalcosts
  WHERE ((jobcode)::text IN ( SELECT vtlkpproject_bygroup.parentproject
           FROM fps.vtlkpproject_bygroup));
