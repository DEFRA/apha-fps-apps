-- Table: fps.period_timecostcalcs
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: period_timecostcalcs; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.period_timecostcalcs (
    id integer NOT NULL,
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
    "time" double precision,
    totalcost money
);
-- Name: period_timecostcalcs_id_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.period_timecostcalcs_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: period_timecostcalcs_id_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.period_timecostcalcs_id_seq OWNED BY fps.period_timecostcalcs.id;
-- Name: period_timecostcalcs id; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.period_timecostcalcs ALTER COLUMN id SET DEFAULT nextval('fps.period_timecostcalcs_id_seq'::regclass);
-- Name: period_timecostcalcs pk_period_timecostcalcs_1; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.period_timecostcalcs
    ADD CONSTRAINT pk_period_timecostcalcs_1 PRIMARY KEY (id);
