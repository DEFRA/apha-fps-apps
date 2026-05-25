-- Table: mabarchive.temptbladditionalcosts
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: temptbladditionalcosts; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.temptbladditionalcosts (
    ac_identity integer NOT NULL,
    project integer DEFAULT 0,
    year integer DEFAULT 0,
    accountcat character varying(50),
    description character varying(20),
    itemcost double precision DEFAULT 0,
    costentered double precision DEFAULT 0,
    freq character varying(5)
);
-- Name: temptbladditionalcosts_ac_identity_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.temptbladditionalcosts_ac_identity_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: temptbladditionalcosts_ac_identity_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.temptbladditionalcosts_ac_identity_seq OWNED BY mabarchive.temptbladditionalcosts.ac_identity;
-- Name: temptbladditionalcosts ac_identity; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.temptbladditionalcosts ALTER COLUMN ac_identity SET DEFAULT nextval('mabarchive.temptbladditionalcosts_ac_identity_seq'::regclass);
-- Name: temptbladditionalcosts aaaaatemptbladditionalcosts_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.temptbladditionalcosts
    ADD CONSTRAINT aaaaatemptbladditionalcosts_pk PRIMARY KEY (ac_identity);
-- Name: temptbladditionalcosts_tbladditionalcostsproject; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX temptbladditionalcosts_tbladditionalcostsproject ON mabarchive.temptbladditionalcosts USING btree (project);
-- Name: temptblprojectyeartemptbladditionalcosts; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX temptblprojectyeartemptbladditionalcosts ON mabarchive.temptbladditionalcosts USING btree (project, year);
