-- Table: mabarchive.temptblanimalreq
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: temptblanimalreq; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.temptblanimalreq (
    ar_identity integer NOT NULL,
    project integer DEFAULT 0,
    year integer DEFAULT 0,
    animaltype character varying(50),
    "number of days" double precision DEFAULT 0,
    "number of animals" double precision DEFAULT 0,
    dailyrate double precision DEFAULT 0
);
-- Name: temptblanimalreq_ar_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.temptblanimalreq_ar_identity_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: temptblanimalreq_ar_identity_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.temptblanimalreq_ar_identity_seq OWNED BY mabarchive.temptblanimalreq.ar_identity;
-- Name: temptblanimalreq ar_identity; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.temptblanimalreq ALTER COLUMN ar_identity SET DEFAULT nextval('mabarchive.temptblanimalreq_ar_identity_seq'::regclass);
-- Name: temptblanimalreq aaaaatemptblanimalreq_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.temptblanimalreq
    ADD CONSTRAINT aaaaatemptblanimalreq_pk PRIMARY KEY (ar_identity);
-- Name: temptblanimalreq_proj_ind; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX temptblanimalreq_proj_ind ON mabarchive.temptblanimalreq USING btree (project, year, animaltype);
-- Name: temptblanimalreq_tblanimalreqproject; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX temptblanimalreq_tblanimalreqproject ON mabarchive.temptblanimalreq USING btree (project);
-- Name: temptblprojectyeartemptblanimalreq; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX temptblprojectyeartemptblanimalreq ON mabarchive.temptblanimalreq USING btree (project, year);
