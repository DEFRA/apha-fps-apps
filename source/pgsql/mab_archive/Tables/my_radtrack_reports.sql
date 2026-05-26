-- Table: mabarchive.my_radtrack_reports
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_radtrack_reports; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_radtrack_reports (
    year smallint NOT NULL,
    project character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    type character varying(10) NOT NULL COLLATE pg_catalog."und-x-icu",
    reminder1 date,
    reminder2 date,
    replyreceived date,
    senttoprogmanager date,
    senttoprojleader date,
    emailedtocustomer date,
    signedcopytocustomer date,
    repduedate date,
    id integer NOT NULL,
    reportagreeddate date
);
-- Name: COLUMN my_radtrack_reports.id; Type: COMMENT; Schema: mabarchive; Owner: -
COMMENT ON COLUMN mabarchive.my_radtrack_reports.id IS 'Converted from IDENTITY(1,1) in MSSQL to serial in PostgreSQL';
-- Name: my_radtrack_reports_id_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.my_radtrack_reports_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: my_radtrack_reports_id_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.my_radtrack_reports_id_seq OWNED BY mabarchive.my_radtrack_reports.id;
-- Name: my_radtrack_reports id; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_radtrack_reports ALTER COLUMN id SET DEFAULT nextval('mabarchive.my_radtrack_reports_id_seq'::regclass);
-- Name: my_radtrack_reports pk_my_radtrack_reports; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_radtrack_reports
    ADD CONSTRAINT pk_my_radtrack_reports PRIMARY KEY (id);
-- Name: my_radtrack_reports_pk_my_radtrack_reports_idx; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX my_radtrack_reports_pk_my_radtrack_reports_idx ON mabarchive.my_radtrack_reports USING btree (id);
