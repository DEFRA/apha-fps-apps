-- Table: mabarchive.tblpublication
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblpublication; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblpublication (
    uid integer NOT NULL,
    identifier character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    type character varying(3) NOT NULL COLLATE pg_catalog."und-x-icu",
    program character varying(10) NOT NULL COLLATE pg_catalog."und-x-icu",
    subject character varying(500) COLLATE pg_catalog."und-x-icu",
    leadauthor character varying(50) COLLATE pg_catalog."und-x-icu",
    otherauthors character varying(255) COLLATE pg_catalog."und-x-icu",
    targetdate date,
    submitted date,
    published boolean NOT NULL,
    comments text COLLATE pg_catalog."und-x-icu"
);
-- Name: COLUMN tblpublication.targetdate; Type: COMMENT; Schema: mabarchive; Owner: -
COMMENT ON COLUMN mabarchive.tblpublication.targetdate IS 'Converted from SMALLDATETIME to DATE';
-- Name: COLUMN tblpublication.submitted; Type: COMMENT; Schema: mabarchive; Owner: -
COMMENT ON COLUMN mabarchive.tblpublication.submitted IS 'Converted from SMALLDATETIME to DATE';
-- Name: tblpublication_uid_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.tblpublication_uid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tblpublication_uid_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.tblpublication_uid_seq OWNED BY mabarchive.tblpublication.uid;
-- Name: tblpublication uid; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblpublication ALTER COLUMN uid SET DEFAULT nextval('mabarchive.tblpublication_uid_seq'::regclass);
-- Name: tblpublication ix_tblpublication; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblpublication
    ADD CONSTRAINT ix_tblpublication UNIQUE (identifier);
-- Name: tblpublication pk_tblpublication; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblpublication
    ADD CONSTRAINT pk_tblpublication PRIMARY KEY (uid);
