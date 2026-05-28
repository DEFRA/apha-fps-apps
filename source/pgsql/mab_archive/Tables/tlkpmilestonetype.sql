CREATE TABLE IF NOT EXISTS mabarchive.tlkpmilestonetype (
    idtype character(1) NOT NULL,
    type character varying(50),
    milestonedeliverable character(1),
    CONSTRAINT pk_tlkpmilestonetype PRIMARY KEY (idtype)
);
