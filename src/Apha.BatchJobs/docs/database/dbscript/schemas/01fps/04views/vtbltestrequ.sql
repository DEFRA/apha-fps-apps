-- View: fps.vtbltestrequ

CREATE OR REPLACE VIEW fps.vtbltestrequ AS
 SELECT buyer AS jobcode,
    testcode,
    norequired AS notests,
    unitprice AS testprice,
    datecreated,
    projectbuyercode
   FROM fps.tlkptestreqmt;
