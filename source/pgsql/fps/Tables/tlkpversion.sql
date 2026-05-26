-- Table: fps.tlkpversion
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpversion; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpversion (
    version integer,
    x bytea,
    islive integer
);
-- Name: tlkpversion version_pk; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpversion
    ADD CONSTRAINT version_pk UNIQUE (version);
