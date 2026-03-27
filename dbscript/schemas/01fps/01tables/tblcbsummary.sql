-- Table: fps.tblcbsummary

CREATE TABLE fps.tblcbsummary (
    cbproject character varying(50) NOT NULL,
    financialyear smallint NOT NULL,
    cbprojecttitle character varying(100),
    startdate date NOT NULL,
    animalcost money,
    testcost money,
    staffcost money,
    linecost money,
    CONSTRAINT pk_tblcbsummary_1__10 PRIMARY KEY (cbproject, financialyear)
);

COMMENT ON COLUMN fps.tblcbsummary.startdate IS $$Converted from DATETIME in MSSQL to DATE in PostgreSQL$$;
