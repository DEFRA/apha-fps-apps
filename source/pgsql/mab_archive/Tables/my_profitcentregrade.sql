-- Table: mabarchive.my_profitcentregrade
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_profitcentregrade; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_profitcentregrade (
    year integer NOT NULL,
    pcgrade character varying(20) NOT NULL,
    divisiongrade character varying(10) NOT NULL,
    gradecode character varying(10) NOT NULL,
    profitcentre character varying(50) NOT NULL,
    chargerate money,
    directrate money,
    payrate money,
    npr money,
    ohr money
);
-- Name: my_profitcentregrade pk__my_profitcentregrad__2bde8e15; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_profitcentregrade
    ADD CONSTRAINT pk__my_profitcentregrad__2bde8e15 PRIMARY KEY (year, pcgrade);
