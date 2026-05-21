CREATE TABLE IF NOT EXISTS mabarchive.tlkprisk (
    riskid integer NOT NULL,
    riskrating character varying(15) NOT NULL,
    CONSTRAINT pk_tlkprisk PRIMARY KEY (riskid)
);
