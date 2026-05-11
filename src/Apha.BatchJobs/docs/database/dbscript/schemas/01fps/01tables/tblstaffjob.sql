-- Table: fps.tblstaffjob

CREATE TABLE fps.tblstaffjob (
    staffid citext NOT NULL,
    jobcode citext NOT NULL,
    plannedhours double precision DEFAULT 0 NOT NULL,
    fpsyear integer,
    CONSTRAINT pk__tblstaffjob__30392ede PRIMARY KEY (staffid, jobcode),
    CONSTRAINT fk_tblstaffjob_1__10 FOREIGN KEY (staffid) REFERENCES fps.tblwgemployee(pactid),
    CONSTRAINT fk_tblstaffjob_jobcode FOREIGN KEY (jobcode) REFERENCES fps.tlkpproject(parentproject)
);

