-- Table: fps.tblkpprofitcentre

CREATE TABLE fps.tblkpprofitcentre (
    profitcentre citext NOT NULL,
    profitcentrename character varying(40) NOT NULL,
    division citext DEFAULT 0 NOT NULL,
    conttarget money DEFAULT 0,
    profitcentrehead character varying(50),
    divisionid integer DEFAULT 0,
    email_recipient character varying(50),
    timesheetlayout smallint,
    timesheet integer,
    outputsheet integer,
    pactcoordinatoremailname character varying(50),
    highlevelsummary bytea,
    CONSTRAINT pk__tblkpprofitcentr__1db06a4f PRIMARY KEY (profitcentre),
    CONSTRAINT fk_tblkpprofitcentre_division FOREIGN KEY (division) REFERENCES fps.tlkpdivision(divname)
);

