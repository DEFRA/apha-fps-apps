-- View: fps.vcontractadditionalcosts

CREATE OR REPLACE VIEW fps.vcontractadditionalcosts AS
 SELECT jobcode,
    account,
    description,
    itemcost
   FROM fps.tbladditionalcosts
  WHERE ((jobcode)::text IN ( SELECT vcontractproject.parentproject
           FROM fps.vcontractproject));
