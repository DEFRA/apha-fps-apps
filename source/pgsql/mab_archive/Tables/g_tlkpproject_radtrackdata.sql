-- Table: mabarchive.g_tlkpproject_radtrackdata
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: g_tlkpproject_radtrackdata; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.g_tlkpproject_radtrackdata (
    parentproject character varying(20) NOT NULL,
    version character varying(10),
    fileref character varying(20),
    customerref character varying(20),
    startdate date,
    enddate date,
    finalreportreceived date,
    finalreportsent date,
    inflation smallint DEFAULT 0,
    closeddate date,
    useprojectyear smallint DEFAULT 0 NOT NULL,
    status character varying(50),
    pcforecastspend double precision,
    riskid integer,
    costbooknumber character varying(10),
    revisedenddate date,
    formrequired boolean DEFAULT true NOT NULL,
    overallcustincome money
);
-- Name: g_tlkpproject_radtrackdata pk_g_tlkpproject_radtrackdata; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.g_tlkpproject_radtrackdata
    ADD CONSTRAINT pk_g_tlkpproject_radtrackdata PRIMARY KEY (parentproject);
