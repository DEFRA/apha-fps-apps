CREATE TABLE IF NOT EXISTS mabarchive.tbl_settings (
    id character varying(50) NOT NULL,
    setting character varying(255),
    notes character varying(255),
    testsetting character varying(255),
    userupdateable boolean DEFAULT false,
    CONSTRAINT pk_tbl_settings PRIMARY KEY (id)
);
