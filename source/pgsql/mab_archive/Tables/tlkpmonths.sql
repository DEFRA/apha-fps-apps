CREATE TABLE IF NOT EXISTS mabarchive.tlkpmonths (
    fmonthno integer NOT NULL,
    monthno integer,
    monthname character varying(50),
    CONSTRAINT pk_tlkpmonths PRIMARY KEY (fmonthno)
);
