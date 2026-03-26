-- Table: fps.projectmonth3

CREATE TABLE fps.projectmonth3 (
    endperiod double precision NOT NULL,
    periodname character varying(50),
    project character varying(20) NOT NULL,
    cumcost money,
    cuminvoices money,
    cumcoiw money,
    cumportsales double precision,
    cumprofile money,
    sumofcostprofile money,
    sumofmstonedue double precision,
    sumofdue__done double precision,
    sumofontime double precision,
    cumcwdebit money,
    cumcwcredit money,
    cumtotalhours double precision,
    cumsubcontracts double precision,
    cumtestcosts double precision,
    cumpaycosts double precision,
    fpsyear integer,
    CONSTRAINT aaaaaprojectmonth3_pk PRIMARY KEY (endperiod, project)
);

