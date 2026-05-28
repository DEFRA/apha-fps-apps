CREATE TABLE IF NOT EXISTS mabarchive.tlkpfrequency (
    frequencyid integer NOT NULL,
    frequency character varying(50),
    CONSTRAINT pk_tlkpfrequency PRIMARY KEY (frequencyid)
);
