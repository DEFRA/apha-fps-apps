-- Table: mabarchive.my_tblanimals

CREATE TABLE mabarchive.my_tblanimals (
    year smallint NOT NULL,
    animaltype character varying(50) NOT NULL,
    species character varying(50),
    security_level character varying(50),
    dailyrate money,
    planbyweek boolean,
    defradailyrate money,
    CONSTRAINT pk__my_tblanimals__18ebb532 PRIMARY KEY (year, animaltype)
);

