-- Table: fps.profitcentregrade

CREATE TABLE fps.profitcentregrade (
    pcgrade citext NOT NULL,
    divisiongrade citext NOT NULL,
    gradecode citext NOT NULL,
    profitcentre citext NOT NULL,
    chargerate money,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    hrsavailable double precision DEFAULT 0,
    oldchargerate money DEFAULT 0,
    defrachargerate money,
    fpsyear integer,
    CONSTRAINT pk__profitcentregrad__2bde8e15 PRIMARY KEY (pcgrade),
    CONSTRAINT fk_profitcentregrade_divisiongrade FOREIGN KEY (divisiongrade) REFERENCES fps.divisiongrade(divisiongrade),
    CONSTRAINT fk_profitcentregrade_gradecode FOREIGN KEY (gradecode) REFERENCES fps.grade(gradecode),
    CONSTRAINT fk_profitcentregrade_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre)
);

