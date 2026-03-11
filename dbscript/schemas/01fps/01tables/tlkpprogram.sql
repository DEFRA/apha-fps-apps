-- Table: fps.tlkpprogram

CREATE TABLE fps.tlkpprogram (
    programno citext NOT NULL,
    programname character varying(80),
    directorate character varying(15),
    minim character varying(7),
    sector_name character varying(50) DEFAULT 'Charge'::character varying,
    customer character varying(50),
    target money DEFAULT 0,
    manager character varying(50),
    fpsyear integer,
    CONSTRAINT pk__tlkpprogram__2180fb33 PRIMARY KEY (programno)
);

