-- Table: fps.tblsurvff_submissions

CREATE TABLE fps.tblsurvff_submissions (
    sd_pact_wg character varying(50) NOT NULL,
    contract character varying(20) NOT NULL,
    countofjobname integer,
    fpsyear integer,
    CONSTRAINT pk___1__12 PRIMARY KEY (sd_pact_wg, contract)
);

