-- Table: mabarchive.tblprogram_manager_link

CREATE TABLE mabarchive.tblprogram_manager_link (
    program character varying(50) NOT NULL,
    manager character varying(50) NOT NULL,
    CONSTRAINT pk_tblprogram_manager PRIMARY KEY (program, manager)
);

