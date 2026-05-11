-- Table: mabarchive.tblprojectmanager

CREATE TABLE mabarchive.tblprojectmanager (
    projectmanager character varying(50) NOT NULL,
    email character varying(255),
    mnumber character varying(10),
    disable boolean DEFAULT false NOT NULL,
    CONSTRAINT pk_tblprojectmanager PRIMARY KEY (projectmanager)
);

