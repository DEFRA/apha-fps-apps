-- Table: fps.tblcbsummary
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblcbsummary; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblcbsummary (
    cbproject character varying(50) NOT NULL COLLATE public.latin1_general_ci_as,
    financialyear smallint NOT NULL,
    cbprojecttitle character varying(100) COLLATE public.latin1_general_ci_as,
    startdate date NOT NULL,
    animalcost money,
    testcost money,
    staffcost money,
    linecost money
);
-- Name: COLUMN tblcbsummary.startdate; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblcbsummary.startdate IS 'Converted from DATETIME in MSSQL to DATE in PostgreSQL';
-- Name: tblcbsummary pk_tblcbsummary_1__10; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblcbsummary
    ADD CONSTRAINT pk_tblcbsummary_1__10 PRIMARY KEY (cbproject, financialyear);
