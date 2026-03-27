-- Table: fps.divisiongrade

CREATE TABLE fps.divisiongrade (
    divisiongrade citext NOT NULL,
    gradecode citext NOT NULL,
    division citext NOT NULL,
    chargerate money DEFAULT 0,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    fpsyear integer,
    CONSTRAINT pk__divisiongrade__225523db PRIMARY KEY (divisiongrade),
    CONSTRAINT fk_divisiongrade_division FOREIGN KEY (division) REFERENCES fps.tlkpdivision(divname),
    CONSTRAINT fk_divisiongrade_gradecode FOREIGN KEY (gradecode) REFERENCES fps.grade(gradecode)
);

