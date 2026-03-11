-- View: fps.vprojectstaffcount

CREATE OR REPLACE VIEW fps.vprojectstaffcount AS
 SELECT jobcode,
    count(staffid) AS countofstaff
   FROM fps.tblstaffjob
  GROUP BY jobcode;
