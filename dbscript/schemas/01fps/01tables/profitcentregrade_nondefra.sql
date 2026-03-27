-- Table: fps.profitcentregrade_nondefra

CREATE TABLE fps.profitcentregrade_nondefra (
    pcgrade character varying(20) NOT NULL,
    divisiongrade citext NOT NULL,
    gradecode citext NOT NULL,
    profitcentre citext NOT NULL,
    chargerate money DEFAULT 0,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    hrsavailable double precision DEFAULT 0,
    oldchargerate money DEFAULT 0,
    fpsyear integer,
    CONSTRAINT pk__profitcentregrad__666 PRIMARY KEY (pcgrade),
    CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade FOREIGN KEY (divisiongrade) REFERENCES fps.divisiongrade(divisiongrade),
    CONSTRAINT fk_profitcentregrade_nondefra_gradecode FOREIGN KEY (gradecode) REFERENCES fps.grade(gradecode),
    CONSTRAINT fk_profitcentregrade_nondefra_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre)
);

