CREATE TABLE IF NOT EXISTS mabarchive.tblprofitcentre_manager_link (
    profitcentre character varying(50) NOT NULL,
    manager character varying(50) NOT NULL,
    CONSTRAINT pk_tblprofitcentre_manager_link PRIMARY KEY (profitcentre, manager)
);
