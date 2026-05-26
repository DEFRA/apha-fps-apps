-- Table: fps.tlkpaccountcode
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpaccountcode; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpaccountcode (
    code public.citext NOT NULL,
    description character varying(50) NOT NULL
);
-- Name: tlkpaccountcode pk_tlkpaccountcode; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpaccountcode
    ADD CONSTRAINT pk_tlkpaccountcode PRIMARY KEY (code);
