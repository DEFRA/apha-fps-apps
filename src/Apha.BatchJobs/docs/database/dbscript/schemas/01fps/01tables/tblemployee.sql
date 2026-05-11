-- Table: fps.tblemployee

CREATE TABLE fps.tblemployee (
    spnumber citext NOT NULL,
    firstname character varying(20),
    lastname character varying(20),
    title character varying(4),
    fpsyear integer,
    CONSTRAINT pk___5__10 PRIMARY KEY (spnumber)
);

