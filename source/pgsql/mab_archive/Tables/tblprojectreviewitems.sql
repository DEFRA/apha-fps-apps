-- Table: mabarchive.tblprojectreviewitems
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblprojectreviewitems; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblprojectreviewitems (
    project character varying(50) NOT NULL,
    itemid integer NOT NULL,
    frequencyid integer
);
-- Name: tblprojectreviewitems pk_tblprojectreviewitems; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblprojectreviewitems
    ADD CONSTRAINT pk_tblprojectreviewitems PRIMARY KEY (project, itemid);
