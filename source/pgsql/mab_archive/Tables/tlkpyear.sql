CREATE TABLE IF NOT EXISTS mabarchive.tlkpyear (
    year integer NOT NULL,
    latestmonthreleased integer,
    CONSTRAINT pk_tlkpyear PRIMARY KEY (year)
);
