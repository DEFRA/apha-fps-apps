-- Table: fps.period_timecostcalcs

CREATE TABLE fps.period_timecostcalcs (
    id integer DEFAULT nextval('fps.period_timecostcalcs_id_seq'::regclass) NOT NULL,
    period integer NOT NULL,
    project character varying(20) NOT NULL,
    oracleprojectcode character varying(50),
    subaccountcode character varying(50),
    month double precision NOT NULL,
    defraproject character varying(3) NOT NULL,
    occ double precision,
    opc character varying(50),
    spc character varying(50) NOT NULL,
    scc double precision,
    name character varying(50),
    gradecode character varying(10),
    spnumber character varying(10) NOT NULL,
    chargerate money,
    pay money,
    nonpay money,
    overhead money,
    time double precision,
    totalcost money,
    CONSTRAINT pk_period_timecostcalcs_1 PRIMARY KEY (id)
);

