CREATE TABLE IF NOT EXISTS fps.tblpostmortem1report (
    testcode character varying(20) NOT NULL,
    itemdescription character(18),
    totvol integer,
    ltunitcharge money,
    sdunitcharge money,
    ltfee money,
    sdfee money,
    total_fee money,
    fee_charged money,
    profit_loss money,
    workgroup character varying(50)
);

COMMENT ON TABLE fps.tblpostmortem1report IS 'Converted from MSSQL to PostgreSQL';
