-- Table: fps.tblanimals

CREATE TABLE fps.tblanimals (
    animaltype citext NOT NULL,
    species character varying(50),
    security_level character varying(50),
    dailyrate money,
    planbyweek boolean DEFAULT false NOT NULL,
    defradailyrate money,
    fpsyear integer,
    CONSTRAINT pk__tblanimals__18ebb532 PRIMARY KEY (animaltype)
);

