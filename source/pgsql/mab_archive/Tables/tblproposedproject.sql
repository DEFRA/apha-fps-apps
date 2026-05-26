-- Table: mabarchive.tblproposedproject
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblproposedproject; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblproposedproject (
    id integer NOT NULL,
    parentproject character varying(20) NOT NULL,
    projecttitle character varying(200),
    program character varying(10),
    customer character varying(50),
    manager character varying(50),
    projectstatus character varying(50),
    costbookno character varying(50),
    disease character varying(50),
    reason character varying(250)
);
-- Name: tblproposedproject_id_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.tblproposedproject_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tblproposedproject_id_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.tblproposedproject_id_seq OWNED BY mabarchive.tblproposedproject.id;
-- Name: tblproposedproject id; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblproposedproject ALTER COLUMN id SET DEFAULT nextval('mabarchive.tblproposedproject_id_seq'::regclass);
-- Name: tblproposedproject pk_tblproposedproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblproposedproject
    ADD CONSTRAINT pk_tblproposedproject PRIMARY KEY (id);
-- Name: project_index; Type: INDEX; Schema: mabarchive; Owner: -
CREATE UNIQUE INDEX project_index ON mabarchive.tblproposedproject USING btree (parentproject);
