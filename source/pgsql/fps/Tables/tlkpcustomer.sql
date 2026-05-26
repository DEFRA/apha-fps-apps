-- Table: fps.tlkpcustomer
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpcustomer; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpcustomer (
    customer public.citext NOT NULL
);
-- Name: tlkpcustomer pk___1__15; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpcustomer
    ADD CONSTRAINT pk___1__15 PRIMARY KEY (customer);
