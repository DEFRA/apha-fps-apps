-- Table: fps.additionalcosts_log
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: additionalcosts_log; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.additionalcosts_log (
    sequenceno integer NOT NULL,
    jobcode character varying(20) NOT NULL,
    account character varying(50) NOT NULL,
    description character varying(20) NOT NULL,
    itemcost money NOT NULL,
    freq character varying(5),
    supplier character varying(50),
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    fpsyear integer NOT NULL
);
-- Name: additionalcosts_log_sequenceno_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.additionalcosts_log_sequenceno_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: additionalcosts_log_sequenceno_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.additionalcosts_log_sequenceno_seq OWNED BY fps.additionalcosts_log.sequenceno;
-- Name: additionalcosts_log sequenceno; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.additionalcosts_log ALTER COLUMN sequenceno SET DEFAULT nextval('fps.additionalcosts_log_sequenceno_seq'::regclass);
-- Name: additionalcosts_log pk_additionalcosts_log; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.additionalcosts_log
    ADD CONSTRAINT pk_additionalcosts_log PRIMARY KEY (sequenceno, fpsyear);
-- Name: additionalcosts_log_ind_dt; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX additionalcosts_log_ind_dt ON fps.additionalcosts_log USING btree (date_time);
-- Name: additionalcosts_log_ind_jc; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX additionalcosts_log_ind_jc ON fps.additionalcosts_log USING btree (jobcode);
-- Name: additionalcosts_log_pk_additionalcosts_log_idx; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX additionalcosts_log_pk_additionalcosts_log_idx ON fps.additionalcosts_log USING btree (sequenceno);
-- Name: additionalcosts_log fk_additionalcosts_log_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.additionalcosts_log
    ADD CONSTRAINT fk_additionalcosts_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
