-- Table: fps.mt_log
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: mt_log; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.mt_log (
    sequenceno integer NOT NULL,
    pactstaffid character varying(50) NOT NULL,
    timecode character varying(50) NOT NULL,
    month double precision NOT NULL,
    parentproject character varying(20) NOT NULL,
    workgroup character varying(50),
    hours double precision,
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    fpsyear integer NOT NULL
);
-- Name: mt_log_sequenceno_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.mt_log_sequenceno_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: mt_log_sequenceno_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.mt_log_sequenceno_seq OWNED BY fps.mt_log.sequenceno;
-- Name: mt_log sequenceno; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.mt_log ALTER COLUMN sequenceno SET DEFAULT nextval('fps.mt_log_sequenceno_seq'::regclass);
-- Name: mt_log pk_mt_log; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.mt_log
    ADD CONSTRAINT pk_mt_log PRIMARY KEY (sequenceno, fpsyear);
-- Name: mt_log fk_mt_log_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.mt_log
    ADD CONSTRAINT fk_mt_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
