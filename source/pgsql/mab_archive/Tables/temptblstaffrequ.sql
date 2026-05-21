-- Table: mabarchive.temptblstaffrequ
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: temptblstaffrequ; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.temptblstaffrequ (
    sr_identity integer NOT NULL,
    project integer DEFAULT 0,
    year integer DEFAULT 0,
    wggrade character varying(20),
    name character varying(50),
    nohours double precision DEFAULT 0,
    nodays double precision DEFAULT 0,
    chargerate double precision DEFAULT 0
);
-- Name: temptblstaffrequ_sr_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.temptblstaffrequ_sr_identity_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: temptblstaffrequ_sr_identity_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.temptblstaffrequ_sr_identity_seq OWNED BY mabarchive.temptblstaffrequ.sr_identity;
-- Name: temptblstaffrequ sr_identity; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.temptblstaffrequ ALTER COLUMN sr_identity SET DEFAULT nextval('mabarchive.temptblstaffrequ_sr_identity_seq'::regclass);
-- Name: temptblstaffrequ aaaaatemptblstaffrequ_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.temptblstaffrequ
    ADD CONSTRAINT aaaaatemptblstaffrequ_pk PRIMARY KEY (sr_identity);
-- Name: tblstaffrequproject; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tblstaffrequproject ON mabarchive.temptblstaffrequ USING btree (project);
-- Name: temptblprojectyeartemptblstaffrequ; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX temptblprojectyeartemptblstaffrequ ON mabarchive.temptblstaffrequ USING btree (project, year);
