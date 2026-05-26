-- Table: mabarchive.tblstaffrequ
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblstaffrequ; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblstaffrequ (
    sr_identity integer NOT NULL,
    project character varying(50),
    year integer DEFAULT 0,
    wggrade character varying(20) NOT NULL,
    name character varying(50),
    nohours double precision DEFAULT 0,
    nodays double precision DEFAULT 0,
    chargerate double precision DEFAULT 0,
    payrate double precision,
    npr double precision,
    ohr double precision
);
-- Name: tblstaffrequ_sr_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.tblstaffrequ_sr_identity_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tblstaffrequ_sr_identity_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.tblstaffrequ_sr_identity_seq OWNED BY mabarchive.tblstaffrequ.sr_identity;
-- Name: tblstaffrequ sr_identity; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblstaffrequ ALTER COLUMN sr_identity SET DEFAULT nextval('mabarchive.tblstaffrequ_sr_identity_seq'::regclass);
-- Name: tblstaffrequ aaaaatblstaffrequ_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblstaffrequ
    ADD CONSTRAINT aaaaatblstaffrequ_pk PRIMARY KEY (sr_identity);
-- Name: tblprojectyeartblstaffrequ; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tblprojectyeartblstaffrequ ON mabarchive.tblstaffrequ USING btree (project, year);
