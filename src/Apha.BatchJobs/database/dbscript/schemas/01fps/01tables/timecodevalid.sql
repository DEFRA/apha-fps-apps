-- Table: fps.timecodevalid

CREATE TABLE fps.timecodevalid (
    timecode citext NOT NULL,
    workgroup citext NOT NULL,
    parentproject citext NOT NULL,
    testcode character varying(50),
    jobcode character varying(50),
    portfolio character varying(20),
    active boolean NOT NULL,
    fpsyear integer,
    CONSTRAINT aaaaatimecodevalid_pk PRIMARY KEY (workgroup, timecode, parentproject),
    CONSTRAINT fk_timecodevalid_parentproject FOREIGN KEY (parentproject) REFERENCES fps.tlkpproject(parentproject)
);

