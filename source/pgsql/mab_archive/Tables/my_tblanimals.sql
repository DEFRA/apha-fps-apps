CREATE TABLE IF NOT EXISTS mabarchive.my_tblanimals (
    year smallint NOT NULL,
    animaltype character varying(50) NOT NULL,
    species character varying(50),
    security_level character varying(50),
    dailyrate money,
    planbyweek boolean,
    defradailyrate money,
    CONSTRAINT pk_my_tblanimals PRIMARY KEY (year, animaltype)
);
