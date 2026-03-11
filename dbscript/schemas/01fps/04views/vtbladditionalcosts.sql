-- View: fps.vtbladditionalcosts

CREATE OR REPLACE VIEW fps.vtbladditionalcosts AS
 SELECT jobcode,
    account,
    description,
    itemcost,
    freq,
    supplier
   FROM fps.tbladditionalcosts
  WHERE ((jobcode)::text IN ( SELECT vtlkpproject.parentproject
           FROM fps.vtlkpproject));
