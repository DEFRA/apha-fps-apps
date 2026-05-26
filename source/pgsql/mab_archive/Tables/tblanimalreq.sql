-- Table: mabarchive.tblanimalreq
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblanimalreq; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblanimalreq (
    ar_identity integer NOT NULL,
    project character varying(50),
    year integer DEFAULT 0,
    animaltype character varying(50) NOT NULL,
    "number of days" double precision,
    "number of animals" double precision DEFAULT 0,
    dailyrate double precision DEFAULT 0
);
-- Name: tblanimalreq_ar_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.tblanimalreq_ar_identity_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tblanimalreq_ar_identity_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.tblanimalreq_ar_identity_seq OWNED BY mabarchive.tblanimalreq.ar_identity;
-- Name: tblanimalreq ar_identity; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblanimalreq ALTER COLUMN ar_identity SET DEFAULT nextval('mabarchive.tblanimalreq_ar_identity_seq'::regclass);
-- Name: tblanimalreq aaaaatblanimalreq_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblanimalreq
    ADD CONSTRAINT aaaaatblanimalreq_pk PRIMARY KEY (ar_identity);
-- Name: tblanimalreq_proj_ind; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tblanimalreq_proj_ind ON mabarchive.tblanimalreq USING btree (project, year, animaltype);
-- Name: tblanimalreq_tblanimalreqproject; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tblanimalreq_tblanimalreqproject ON mabarchive.tblanimalreq USING btree (project);
-- Name: tblprojectyeartblanimalreq; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tblprojectyeartblanimalreq ON mabarchive.tblanimalreq USING btree (project, year);
