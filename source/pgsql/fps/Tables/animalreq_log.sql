-- Table: fps.animalreq_log
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: animalreq_log; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.animalreq_log (
    sequenceno integer NOT NULL,
    jobcode character varying(20) NOT NULL COLLATE public.latin1_general_ci_as,
    animaltype character varying(50) NOT NULL COLLATE public.latin1_general_ci_as,
    numberofdays double precision NOT NULL,
    numberofanimals double precision NOT NULL,
    date_time timestamp without time zone,
    user_id character varying(20) COLLATE public.latin1_general_ci_as,
    insert_delete character(2) COLLATE public.latin1_general_ci_as,
    fpsyear integer NOT NULL
);
-- Name: animalreq_log_sequenceno_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.animalreq_log_sequenceno_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: animalreq_log_sequenceno_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.animalreq_log_sequenceno_seq OWNED BY fps.animalreq_log.sequenceno;
-- Name: animalreq_log sequenceno; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.animalreq_log ALTER COLUMN sequenceno SET DEFAULT nextval('fps.animalreq_log_sequenceno_seq'::regclass);
-- Name: animalreq_log pk_animalreq_log; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.animalreq_log
    ADD CONSTRAINT pk_animalreq_log PRIMARY KEY (sequenceno, fpsyear);
-- Name: animalreq_log_ind_dt; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX animalreq_log_ind_dt ON fps.animalreq_log USING btree (date_time);
-- Name: animalreq_log_ind_jc; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX animalreq_log_ind_jc ON fps.animalreq_log USING btree (jobcode);
-- Name: animalreq_log fk_animalreq_log_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.animalreq_log
    ADD CONSTRAINT fk_animalreq_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
