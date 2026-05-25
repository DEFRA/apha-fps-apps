-- Table: mabarchive.tbladditionalcosts
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbladditionalcosts; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tbladditionalcosts (
    ac_identity integer NOT NULL,
    project character varying(50),
    year integer DEFAULT 0,
    accountcat character varying(50) NOT NULL,
    description character varying(100) NOT NULL,
    itemcost double precision DEFAULT 0,
    costentered double precision DEFAULT 0 NOT NULL,
    freq character varying(5)
);
-- Name: tbladditionalcosts_ac_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.tbladditionalcosts_ac_identity_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tbladditionalcosts_ac_identity_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.tbladditionalcosts_ac_identity_seq OWNED BY mabarchive.tbladditionalcosts.ac_identity;
-- Name: tbladditionalcosts ac_identity; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tbladditionalcosts ALTER COLUMN ac_identity SET DEFAULT nextval('mabarchive.tbladditionalcosts_ac_identity_seq'::regclass);
-- Name: tbladditionalcosts aaaaatbladditionalcosts_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tbladditionalcosts
    ADD CONSTRAINT aaaaatbladditionalcosts_pk PRIMARY KEY (ac_identity);
-- Name: tbladditionalcosts_tbladditionalcostsproject; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tbladditionalcosts_tbladditionalcostsproject ON mabarchive.tbladditionalcosts USING btree (project);
-- Name: tblprojectyeartbladditionalcosts; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tblprojectyeartbladditionalcosts ON mabarchive.tbladditionalcosts USING btree (project, year);
