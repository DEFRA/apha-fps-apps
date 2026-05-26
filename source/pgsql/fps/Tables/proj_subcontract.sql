-- Table: fps.proj_subcontract
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: proj_subcontract; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.proj_subcontract (
    subcontcounter integer NOT NULL,
    project public.citext,
    testjob character varying(50),
    month double precision,
    amount money,
    workgroup character varying(50),
    acctcode character varying(30),
    supplier character varying(50),
    description character varying(255),
    suppliernumber integer,
    dailyrate money,
    animaldays integer,
    fpsyear integer NOT NULL
);
-- Name: proj_subcontract_subcontcounter_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.proj_subcontract_subcontcounter_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: proj_subcontract_subcontcounter_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.proj_subcontract_subcontcounter_seq OWNED BY fps.proj_subcontract.subcontcounter;
-- Name: proj_subcontract subcontcounter; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.proj_subcontract ALTER COLUMN subcontcounter SET DEFAULT nextval('fps.proj_subcontract_subcontcounter_seq'::regclass);
-- Name: proj_subcontract pk_proj_subcontract; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.proj_subcontract
    ADD CONSTRAINT pk_proj_subcontract PRIMARY KEY (subcontcounter, fpsyear);
-- Name: proj_subcontract fk_proj_subcontract_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.proj_subcontract
    ADD CONSTRAINT fk_proj_subcontract_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: proj_subcontract fk_proj_subcontract_project; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.proj_subcontract
    ADD CONSTRAINT fk_proj_subcontract_project FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
