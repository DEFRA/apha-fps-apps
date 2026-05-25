-- Table: mabarchive.my_testorproduct
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_testorproduct; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_testorproduct (
    year smallint NOT NULL,
    itemcode character varying(20) NOT NULL,
    itemdescription character varying(200),
    testmanager character varying(50),
    jobstatus character varying(2),
    unitpricevla money,
    priceahvg money,
    owner character varying(2),
    chargemethod character varying(5),
    shortdescription character(18),
    defraunitprice money
);
-- Name: my_testorproduct pk_my_testorproduct; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_testorproduct
    ADD CONSTRAINT pk_my_testorproduct PRIMARY KEY (year, itemcode);
