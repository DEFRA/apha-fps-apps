-- Table: mabarchive.tblproposedproject

CREATE TABLE mabarchive.tblproposedproject (
    id integer DEFAULT nextval('mabarchive.tblproposedproject_id_seq'::regclass) NOT NULL,
    parentproject character varying(20) NOT NULL,
    projecttitle character varying(200),
    program character varying(10),
    customer character varying(50),
    manager character varying(50),
    projectstatus character varying(50),
    costbookno character varying(50),
    disease character varying(50),
    reason character varying(250),
    CONSTRAINT pk_tblproposedproject PRIMARY KEY (id)
);

