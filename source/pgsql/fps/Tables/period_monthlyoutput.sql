-- Table: fps.period_monthlyoutput
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: period_monthlyoutput; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.period_monthlyoutput (
    id integer NOT NULL,
    period integer NOT NULL,
    project character varying(20) NOT NULL,
    oracleprojectcode character varying(50),
    subaccountcode character varying(50),
    isdefraproject character varying(3) NOT NULL,
    opc character varying(50),
    occ double precision,
    month double precision NOT NULL,
    spc character varying(50) NOT NULL,
    workgroup character varying(50) NOT NULL,
    scc double precision,
    testcode character varying(20) NOT NULL,
    volume double precision,
    testprice money,
    totalcost money
);
-- Name: COLUMN period_monthlyoutput.id; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.period_monthlyoutput.id IS 'Converted from IDENTITY(1,1) to SERIAL';
-- Name: COLUMN period_monthlyoutput.project; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.period_monthlyoutput.project IS 'Original collation: Latin1_General_CI_AS';
-- Name: COLUMN period_monthlyoutput.oracleprojectcode; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.period_monthlyoutput.oracleprojectcode IS 'Original collation: Latin1_General_CI_AS';
-- Name: COLUMN period_monthlyoutput.subaccountcode; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.period_monthlyoutput.subaccountcode IS 'Original collation: Latin1_General_CI_AS';
-- Name: COLUMN period_monthlyoutput.isdefraproject; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.period_monthlyoutput.isdefraproject IS 'Original collation: Latin1_General_CI_AS';
-- Name: COLUMN period_monthlyoutput.opc; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.period_monthlyoutput.opc IS 'Original collation: Latin1_General_CI_AS';
-- Name: COLUMN period_monthlyoutput.spc; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.period_monthlyoutput.spc IS 'Original collation: Latin1_General_CI_AS';
-- Name: COLUMN period_monthlyoutput.workgroup; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.period_monthlyoutput.workgroup IS 'Original collation: Latin1_General_CI_AS';
-- Name: COLUMN period_monthlyoutput.testcode; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.period_monthlyoutput.testcode IS 'Original collation: Latin1_General_CI_AS';
-- Name: period_monthlyoutput_id_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.period_monthlyoutput_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: period_monthlyoutput_id_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.period_monthlyoutput_id_seq OWNED BY fps.period_monthlyoutput.id;
-- Name: period_monthlyoutput id; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.period_monthlyoutput ALTER COLUMN id SET DEFAULT nextval('fps.period_monthlyoutput_id_seq'::regclass);
-- Name: period_monthlyoutput pk_period_monthlyoutput_1; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.period_monthlyoutput
    ADD CONSTRAINT pk_period_monthlyoutput_1 PRIMARY KEY (id);
