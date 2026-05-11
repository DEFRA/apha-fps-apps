-- Table: fps.tblpostmortem1report

CREATE TABLE fps.tblpostmortem1report (
    testcode character varying(20) NOT NULL,
    itemdescription character(18),
    totvol integer,
    ltunitcharge money,
    sdunitcharge money,
    ltfee money,
    sdfee money,
    "total fee" money,
    "fee charged" money,
    "profit/loss" money,
    workgroup character varying(50)
);

COMMENT ON TABLE fps.tblpostmortem1report IS $$Converted from MSSQL to PostgreSQL$$;

