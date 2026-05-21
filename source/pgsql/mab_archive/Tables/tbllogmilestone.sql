-- Table: mabarchive.tbllogmilestone
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbllogmilestone; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tbllogmilestone (
    project character varying(20),
    number character varying(10),
    description character varying(500),
    datedue timestamp without time zone,
    datecompleted timestamp without time zone,
    dateformreceived timestamp without time zone,
    undersdreview smallint,
    ontarget smallint,
    projectleadercomment text,
    capscomment character varying(250),
    idtype character(1),
    datechanged timestamp without time zone,
    changedby character varying(10),
    updatetype character(1),
    id integer NOT NULL
);
-- Name: tbllogmilestone_id_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.tbllogmilestone_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tbllogmilestone_id_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.tbllogmilestone_id_seq OWNED BY mabarchive.tbllogmilestone.id;
-- Name: tbllogmilestone id; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tbllogmilestone ALTER COLUMN id SET DEFAULT nextval('mabarchive.tbllogmilestone_id_seq'::regclass);
-- Name: tbllogmilestone pk_log_tblmilestone; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tbllogmilestone
    ADD CONSTRAINT pk_log_tblmilestone PRIMARY KEY (id);
