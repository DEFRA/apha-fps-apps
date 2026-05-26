-- Table: fps.testreq_log
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: testreq_log; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.testreq_log (
    sequenceno integer NOT NULL,
    testcode character varying(20),
    buyer character varying(20) COLLATE public.latin1_general_ci_as,
    unitprice double precision,
    norequired integer,
    projectbuyercode character varying(50) COLLATE public.latin1_general_ci_as,
    testbuyercode character varying(50) COLLATE public.latin1_general_ci_as,
    active smallint,
    date_time timestamp without time zone,
    user_id character varying(20) COLLATE public.latin1_general_ci_as,
    insert_delete character(2) COLLATE public.latin1_general_ci_as,
    jobcode character varying(50),
    fpsyear integer NOT NULL
);
-- Name: COLUMN testreq_log.jobcode; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.testreq_log.jobcode IS 'Generated column based on projectbuyercode';
-- Name: testreq_log_sequenceno_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.testreq_log_sequenceno_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: testreq_log_sequenceno_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.testreq_log_sequenceno_seq OWNED BY fps.testreq_log.sequenceno;
-- Name: testreq_log sequenceno; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.testreq_log ALTER COLUMN sequenceno SET DEFAULT nextval('fps.testreq_log_sequenceno_seq'::regclass);
-- Name: testreq_log pk_testreq_log; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.testreq_log
    ADD CONSTRAINT pk_testreq_log PRIMARY KEY (sequenceno, fpsyear);
-- Name: idx_testreqlog_sequenceno; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX idx_testreqlog_sequenceno ON fps.testreq_log USING btree (sequenceno) WITH (fillfactor='100', deduplicate_items='true');
-- Name: testreq_log_ind_dt; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX testreq_log_ind_dt ON fps.testreq_log USING btree (date_time);
-- Name: testreq_log_ind_jc; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX testreq_log_ind_jc ON fps.testreq_log USING btree (jobcode);
-- Name: testreq_log fk_testreq_log_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.testreq_log
    ADD CONSTRAINT fk_testreq_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
