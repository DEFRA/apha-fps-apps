-- View: fps.vpactprogram

CREATE OR REPLACE VIEW fps.vpactprogram AS
 SELECT programno,
    programname,
    directorate,
    minim,
    sector_name,
    customer,
    manager AS leader
   FROM fps.tlkpprogram;
