-- Table: fps.recreatesummaries_log
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: recreatesummaries_log; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.recreatesummaries_log (
    id integer NOT NULL,
    userid character varying(20),
    period smallint,
    datedone timestamp without time zone,
    fpsyear integer NOT NULL
);
-- Name: recreatesummaries_log_id_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.recreatesummaries_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: recreatesummaries_log_id_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.recreatesummaries_log_id_seq OWNED BY fps.recreatesummaries_log.id;
-- Name: recreatesummaries_log id; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.recreatesummaries_log ALTER COLUMN id SET DEFAULT nextval('fps.recreatesummaries_log_id_seq'::regclass);
-- Name: recreatesummaries_log pk_recreatesummaries_log; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.recreatesummaries_log
    ADD CONSTRAINT pk_recreatesummaries_log PRIMARY KEY (id, fpsyear);
-- Name: recreatesummaries_log fk_recreatesummaries_log_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.recreatesummaries_log
    ADD CONSTRAINT fk_recreatesummaries_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
