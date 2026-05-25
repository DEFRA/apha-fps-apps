-- Table: mabarchive.my_milestoneformdates
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_milestoneformdates; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_milestoneformdates (
    year smallint NOT NULL,
    parentproject character varying(20) NOT NULL,
    jan timestamp without time zone,
    feb timestamp without time zone,
    mar timestamp without time zone,
    apr timestamp without time zone,
    may timestamp without time zone,
    jun timestamp without time zone,
    jul timestamp without time zone,
    aug timestamp without time zone,
    sep timestamp without time zone,
    oct timestamp without time zone,
    nov timestamp without time zone,
    "dec" timestamp without time zone
);
-- Name: my_milestoneformdates pk_my_milestoneformdates; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_milestoneformdates
    ADD CONSTRAINT pk_my_milestoneformdates PRIMARY KEY (year, parentproject);
