-- Table: mabarchive.my_tblanimalreq
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tblanimalreq; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tblanimalreq (
    year smallint NOT NULL,
    jobcode character varying(20) NOT NULL,
    animaltype character varying(50) NOT NULL,
    numberofdays double precision NOT NULL,
    numberofanimals double precision NOT NULL,
    ar_counter integer NOT NULL
);
-- Name: my_tblanimalreq_AR_Counter_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive."my_tblanimalreq_AR_Counter_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: my_tblanimalreq_AR_Counter_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive."my_tblanimalreq_AR_Counter_seq" OWNED BY mabarchive.my_tblanimalreq.ar_counter;
-- Name: my_tblanimalreq ar_counter; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tblanimalreq ALTER COLUMN ar_counter SET DEFAULT nextval('mabarchive."my_tblanimalreq_AR_Counter_seq"'::regclass);
-- Name: my_tblanimalreq pk_my_tblanimalreq; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tblanimalreq
    ADD CONSTRAINT pk_my_tblanimalreq PRIMARY KEY (ar_counter);
