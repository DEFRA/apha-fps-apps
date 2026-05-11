-- Table: mabarchive.tblprofitcentre_manager_link

CREATE TABLE mabarchive.tblprofitcentre_manager_link (
    profitcentre character varying(50) NOT NULL,
    manager character varying(50) NOT NULL,
    CONSTRAINT pk_tblprofitcentre_manager PRIMARY KEY (profitcentre, manager)
);

