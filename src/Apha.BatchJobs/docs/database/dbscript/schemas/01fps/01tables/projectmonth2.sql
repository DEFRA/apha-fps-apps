-- Table: fps.projectmonth2

CREATE TABLE fps.projectmonth2 (
    project character varying(20) NOT NULL,
    monthno double precision NOT NULL,
    costprofile money,
    subcontracts money,
    animals money,
    nonanimal money,
    timecosts double precision,
    transfercosts double precision,
    totalcost money,
    invoices money,
    coiw money,
    sumofcostprofile money,
    portsales double precision,
    mstonedue integer,
    due__done double precision,
    ontime double precision,
    totalhours double precision,
    paycosts double precision,
    fpsyear integer,
    CONSTRAINT aaaaaprojectmonth2_pk PRIMARY KEY (project, monthno)
);

