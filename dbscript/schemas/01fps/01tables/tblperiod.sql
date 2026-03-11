-- Table: fps.tblperiod

CREATE TABLE fps.tblperiod (
    periodname character varying(50) NOT NULL,
    periodtype character varying(50),
    startperiod double precision,
    endperiod double precision,
    finalsummariesrun smallint,
    periodlocked smallint DEFAULT 0 NOT NULL,
    fpsyear integer,
    CONSTRAINT aaaaatblperiod_pk PRIMARY KEY (periodname)
);

