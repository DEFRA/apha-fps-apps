CREATE TABLE IF NOT EXISTS mabarchive.tlkpprojectstatus (
    projectstatus character varying(50) NOT NULL,
    is_fps boolean NOT NULL,
    is_pims boolean NOT NULL,
    CONSTRAINT pk_tlkpprojectstatus PRIMARY KEY (projectstatus)
);
