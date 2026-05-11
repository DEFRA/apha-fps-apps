-- Table: mabarchive.my_monthlytime

CREATE TABLE mabarchive.my_monthlytime (
    year smallint NOT NULL,
    pactstaffid character varying(50) NOT NULL,
    timecode character varying(50) NOT NULL,
    month double precision NOT NULL,
    parentproject character varying(20) NOT NULL,
    workgroup character varying(50),
    hours double precision,
    CONSTRAINT pk_my_monthlytime PRIMARY KEY (year, pactstaffid, timecode, month, parentproject)
);

