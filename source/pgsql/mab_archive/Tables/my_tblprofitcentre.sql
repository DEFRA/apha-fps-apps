CREATE TABLE IF NOT EXISTS mabarchive.my_tblprofitcentre (
    year smallint NOT NULL,
    profitcentre character varying(50) NOT NULL,
    profitcentrename character varying(40) NOT NULL,
    division character varying(10) NOT NULL,
    conttarget money,
    profitcentrehead character varying(50),
    divisionid integer,
    CONSTRAINT pk_my_tblprofitcentre PRIMARY KEY (year, profitcentre)
);
