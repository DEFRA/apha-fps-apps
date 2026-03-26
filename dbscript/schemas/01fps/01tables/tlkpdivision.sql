-- Table: fps.tlkpdivision

CREATE TABLE fps.tlkpdivision (
    divisionid integer,
    agencyid integer NOT NULL,
    divname citext NOT NULL,
    centoverhead money DEFAULT 0,
    CONSTRAINT pk__tlkpdivision__10566f31 PRIMARY KEY (divname),
    CONSTRAINT fk_tlkpdivision_agencyid FOREIGN KEY (agencyid) REFERENCES fps.tlkpagency(agencyid)
);

