-- Table: mabarchive.my_proj_invoice

CREATE TABLE mabarchive.my_proj_invoice (
    year smallint NOT NULL,
    projectparent character varying(20) NOT NULL,
    month integer,
    amount money,
    costofwork money,
    wip money,
    profitloss money,
    detail character varying(100),
    invoicecounter integer NOT NULL,
    type character varying(10),
    CONSTRAINT pk_my_proj_invoice PRIMARY KEY (year, projectparent, invoicecounter)
);

