-- Table: fps.tlkpsubaccount
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpsubaccount; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpsubaccount (
    subaccountcode public.citext NOT NULL,
    subaccount character varying(50)
);
-- Name: tlkpsubaccount pk_tlkpsubaccount; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpsubaccount
    ADD CONSTRAINT pk_tlkpsubaccount PRIMARY KEY (subaccountcode);
