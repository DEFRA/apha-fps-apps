CREATE TABLE IF NOT EXISTS fps.resourcecentremonth (
    resourcecentre character varying(50) NOT NULL,
    monthno integer NOT NULL,
    payspent money,
    nonpayspent money,
    paybudget money,
    nonpaybudget money,
    spare1 money,
    spare2 money,
    spare3 money,
    spare4 money,
    spare5 money,
    spare6 money,
    CONSTRAINT pk_resourcecentremonth PRIMARY KEY (resourcecentre, monthno)
);
