-- Table: fps.tblstatus
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblstatus; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblstatus (
    status public.citext NOT NULL
);
-- Name: tblstatus pk___3__10; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblstatus
    ADD CONSTRAINT pk___3__10 PRIMARY KEY (status);
