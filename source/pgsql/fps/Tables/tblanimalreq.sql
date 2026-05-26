-- Table: fps.tblanimalreq
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblanimalreq; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblanimalreq (
    jobcode public.citext NOT NULL,
    animaltype public.citext NOT NULL,
    numberofdays double precision DEFAULT 0 NOT NULL,
    numberofanimals double precision DEFAULT 0 NOT NULL,
    indcounter integer NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tblanimalreq_indcounter_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.tblanimalreq_indcounter_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tblanimalreq_indcounter_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.tblanimalreq_indcounter_seq OWNED BY fps.tblanimalreq.indcounter;
-- Name: tblanimalreq indcounter; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblanimalreq ALTER COLUMN indcounter SET DEFAULT nextval('fps.tblanimalreq_indcounter_seq'::regclass);
-- Name: tblanimalreq pk_tblanimalreq; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblanimalreq
    ADD CONSTRAINT pk_tblanimalreq PRIMARY KEY (indcounter, fpsyear);
-- Name: tblanimalreq fk_tblanimalreq_animaltype; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblanimalreq
    ADD CONSTRAINT fk_tblanimalreq_animaltype FOREIGN KEY (animaltype, fpsyear) REFERENCES fps.tblanimals(animaltype, fpsyear);
-- Name: tblanimalreq fk_tblanimalreq_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblanimalreq
    ADD CONSTRAINT fk_tblanimalreq_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tblanimalreq fk_tblanimalreq_jobcode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblanimalreq
    ADD CONSTRAINT fk_tblanimalreq_jobcode FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
