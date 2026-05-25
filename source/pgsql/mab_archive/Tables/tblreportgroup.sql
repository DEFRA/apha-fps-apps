-- Table: mabarchive.tblreportgroup
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblreportgroup; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblreportgroup (
    groupid integer NOT NULL,
    description character varying(50) NOT NULL
);
-- Name: tblreportgroup_groupid_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.tblreportgroup_groupid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tblreportgroup_groupid_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.tblreportgroup_groupid_seq OWNED BY mabarchive.tblreportgroup.groupid;
-- Name: tblreportgroup groupid; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblreportgroup ALTER COLUMN groupid SET DEFAULT nextval('mabarchive.tblreportgroup_groupid_seq'::regclass);
-- Name: tblreportgroup pk_tblreportgroup; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblreportgroup
    ADD CONSTRAINT pk_tblreportgroup PRIMARY KEY (groupid);
