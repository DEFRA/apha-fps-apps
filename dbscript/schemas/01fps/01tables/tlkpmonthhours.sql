-- Table: fps.tlkpmonthhours

CREATE TABLE fps.tlkpmonthhours (
    year smallint NOT NULL,
    month smallint NOT NULL,
    days numeric(5,1),
    cvlhours numeric(5,1),
    vidhours numeric(5,1),
    fmonth smallint,
    fpsyear integer,
    CONSTRAINT tlkpmonthhours_pk UNIQUE (year, month)
);

