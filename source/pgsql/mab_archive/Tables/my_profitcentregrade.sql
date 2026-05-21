CREATE TABLE IF NOT EXISTS mabarchive.my_profitcentregrade (
    year integer NOT NULL,
    pcgrade character varying(20) NOT NULL,
    divisiongrade character varying(10) NOT NULL,
    gradecode character varying(10) NOT NULL,
    profitcentre character varying(50) NOT NULL,
    chargerate money,
    directrate money,
    payrate money,
    npr money,
    ohr money,
    CONSTRAINT pk_my_profitcentregrade PRIMARY KEY (year, pcgrade)
);
