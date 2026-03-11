-- Table: fps.tlkpmanager

CREATE TABLE fps.tlkpmanager (
    manager character varying(50) NOT NULL,
    title character varying(10),
    workgroup character varying(50) NOT NULL,
    gradecode character varying(10) NOT NULL,
    fpsyear integer,
    CONSTRAINT pk___1__18 PRIMARY KEY (manager)
);

