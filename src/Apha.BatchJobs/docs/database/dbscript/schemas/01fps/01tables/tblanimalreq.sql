-- Table: fps.tblanimalreq

CREATE TABLE fps.tblanimalreq (
    jobcode citext NOT NULL,
    animaltype citext NOT NULL,
    numberofdays double precision DEFAULT 0 NOT NULL,
    numberofanimals double precision DEFAULT 0 NOT NULL,
    indcounter integer DEFAULT nextval('fps.tblanimalreq_indcounter_seq'::regclass) NOT NULL,
    fpsyear integer,
    CONSTRAINT pk__tblanimalreq__7271068f PRIMARY KEY (indcounter),
    CONSTRAINT fk_tblanimalreq_animaltype FOREIGN KEY (animaltype) REFERENCES fps.tblanimals(animaltype),
    CONSTRAINT fk_tblanimalreq_jobcode FOREIGN KEY (jobcode) REFERENCES fps.tlkpproject(parentproject)
);

