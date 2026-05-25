-- Table: fps.staffjob_log
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: staffjob_log; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.staffjob_log (
    sequenceno integer NOT NULL,
    staffid character varying(50) NOT NULL,
    jobcode character varying(20) NOT NULL,
    plannedhours double precision NOT NULL,
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    fpsyear integer NOT NULL
);
-- Name: staffjob_log_sequenceno_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.staffjob_log_sequenceno_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: staffjob_log_sequenceno_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.staffjob_log_sequenceno_seq OWNED BY fps.staffjob_log.sequenceno;
-- Name: staffjob_log sequenceno; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.staffjob_log ALTER COLUMN sequenceno SET DEFAULT nextval('fps.staffjob_log_sequenceno_seq'::regclass);
-- Name: staffjob_log pk_staffjob_log; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.staffjob_log
    ADD CONSTRAINT pk_staffjob_log PRIMARY KEY (sequenceno, fpsyear);
-- Name: staffjob_log_ind_dt; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX staffjob_log_ind_dt ON fps.staffjob_log USING btree (date_time);
-- Name: staffjob_log_ind_jc; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX staffjob_log_ind_jc ON fps.staffjob_log USING btree (jobcode);
-- Name: staffjob_log_pk_idx; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX staffjob_log_pk_idx ON fps.staffjob_log USING btree (sequenceno);
-- Name: staffjob_log fk_staffjob_log_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.staffjob_log
    ADD CONSTRAINT fk_staffjob_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
