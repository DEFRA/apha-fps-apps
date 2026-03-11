-- Table: fps.tblwgemployee

CREATE TABLE fps.tblwgemployee (
    pactid citext NOT NULL,
    spnumber citext NOT NULL,
    workgroupgrade citext NOT NULL,
    personstatus character varying(10) DEFAULT 'A'::character varying NOT NULL,
    personclass character varying(10),
    hrspaid double precision NOT NULL,
    leave double precision NOT NULL,
    sickspecial double precision NOT NULL,
    hrsavail double precision NOT NULL,
    makeavailable integer DEFAULT '-1'::integer NOT NULL,
    timerecorder integer DEFAULT 0 NOT NULL,
    startdate date,
    enddate date,
    hoursperweek double precision,
    fpsyear integer,
    CONSTRAINT pk_tblwgemployee_1__10 PRIMARY KEY (pactid),
    CONSTRAINT fk_tblwgemployee_3__10 FOREIGN KEY (workgroupgrade) REFERENCES fps.workgroupgrade(wggrade),
    CONSTRAINT fk_tblwgemployee_spnumber FOREIGN KEY (spnumber) REFERENCES fps.tblemployee(spnumber)
);

