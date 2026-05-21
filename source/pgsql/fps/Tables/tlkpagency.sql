-- Table: fps.tlkpagency
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpagency; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpagency (
    agencyid integer NOT NULL,
    agencyname character varying(18) NOT NULL
);
-- Name: tlkpagency_agencyid_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.tlkpagency_agencyid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tlkpagency_agencyid_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.tlkpagency_agencyid_seq OWNED BY fps.tlkpagency.agencyid;
-- Name: tlkpagency agencyid; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpagency ALTER COLUMN agencyid SET DEFAULT nextval('fps.tlkpagency_agencyid_seq'::regclass);
-- Name: tlkpagency agencyname; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpagency
    ADD CONSTRAINT agencyname UNIQUE (agencyname);
-- Name: tlkpagency pk__tlkpagency__089551d8; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpagency
    ADD CONSTRAINT pk__tlkpagency__089551d8 PRIMARY KEY (agencyid);
