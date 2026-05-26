-- Table: fps.tblpostmortem1report
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblpostmortem1report; Type: TABLE; Schema: fps; Owner: -
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
-- Name: TABLE tblpostmortem1report; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON TABLE fps.tblpostmortem1report IS 'Converted from MSSQL to PostgreSQL';
