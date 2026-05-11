-- Table: fps.workgroup

CREATE TABLE fps.workgroup (
    workgroup citext NOT NULL,
    profitcentre citext NOT NULL,
    costcentre double precision,
    owner character varying(50),
    description character varying(45),
    centraloverhead money DEFAULT 0,
    sendemail smallint,
    cos90 smallint,
    costcentreold double precision,
    email_recipient character varying(50),
    fpsyear integer,
    CONSTRAINT pk__workgroup__25518c17 PRIMARY KEY (workgroup),
    CONSTRAINT fk_workgroup_costcentre FOREIGN KEY (costcentre) REFERENCES fps.costcentre(costcentre),
    CONSTRAINT fk_workgroup_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre)
);

