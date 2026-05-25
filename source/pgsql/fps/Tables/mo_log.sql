-- Table: fps.mo_log
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: mo_log; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.mo_log (
    sequenceno integer NOT NULL,
    testcode character varying(20),
    buyer character varying(20),
    month double precision,
    workgroup character varying(50),
    volume double precision,
    wgbuyer character varying(50),
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    fpsyear integer NOT NULL
);
-- Name: TABLE mo_log; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON TABLE fps.mo_log IS 'Note: PostgreSQL does not support column-level collations. The original SQL Server collation was Latin1_General_CI_AS.';
-- Name: COLUMN mo_log.sequenceno; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.mo_log.sequenceno IS 'This column uses GENERATED ALWAYS AS IDENTITY, which is equivalent to IDENTITY in SQL Server.';
-- Name: mo_log_sequenceno_seq; Type: SEQUENCE; Schema: fps; Owner: -
ALTER TABLE fps.mo_log ALTER COLUMN sequenceno ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME fps.mo_log_sequenceno_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);
-- Name: mo_log pk_mo_log; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.mo_log
    ADD CONSTRAINT pk_mo_log PRIMARY KEY (sequenceno, fpsyear);
-- Name: mo_log fk_mo_log_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.mo_log
    ADD CONSTRAINT fk_mo_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
