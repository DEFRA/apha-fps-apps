CREATE TABLE IF NOT EXISTS mabarchive.my_tblcontract (
    year smallint NOT NULL,
    contractno character varying(10) NOT NULL,
    category character varying(20) NOT NULL,
    manager character varying(50),
    customer character varying(50),
    title character varying(100),
    registereddate timestamp without time zone,
    startdate timestamp without time zone,
    enddate timestamp without time zone,
    contractdoc bytea,
    duration integer,
    CONSTRAINT pk_my_tblcontract PRIMARY KEY (year, contractno)
);
