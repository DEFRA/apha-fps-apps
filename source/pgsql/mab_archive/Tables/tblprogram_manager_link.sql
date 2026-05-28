CREATE TABLE IF NOT EXISTS mabarchive.tblprogram_manager_link (
    program character varying(50) NOT NULL,
    manager character varying(50) NOT NULL,
    CONSTRAINT pk_tblprogram_manager_link PRIMARY KEY (program, manager)
);
