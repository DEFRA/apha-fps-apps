CREATE TABLE IF NOT EXISTS mabarchive.g_tlkpproject (
    parentproject character varying(20) NOT NULL,
    projecttitle character varying(200),
    costbookno character varying(50),
    disease character varying(50),
    contract character varying(10),
    shorttitle character varying(30),
    projectstatus character varying(50),
    CONSTRAINT pk_g_tlkpproject PRIMARY KEY (parentproject)
);
