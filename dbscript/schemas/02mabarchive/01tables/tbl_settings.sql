-- Table: mabarchive.tbl_settings

CREATE TABLE mabarchive.tbl_settings (
    id character varying(50) NOT NULL,
    setting character varying(255),
    notes character varying(255),
    testsetting character varying(255),
    userupdateable boolean DEFAULT false,
    CONSTRAINT aaaaatbl_settings_pk PRIMARY KEY (id)
);

