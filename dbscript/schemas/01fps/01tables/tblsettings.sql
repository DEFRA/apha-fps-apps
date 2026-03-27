-- Table: fps.tblsettings

CREATE TABLE fps.tblsettings (
    id character varying(50) NOT NULL,
    setting character varying(255),
    notes character varying(255),
    testsetting character varying(255),
    fpsyear integer,
    CONSTRAINT pk_tblsettings PRIMARY KEY (id)
);

