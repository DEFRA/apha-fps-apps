-- Table: fps.project_log
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: project_log; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.project_log (
    sequenceno integer NOT NULL,
    parentproject character varying(20) NOT NULL,
    projecttitle character varying(200) NOT NULL,
    program character varying(10) NOT NULL,
    customer character varying(50) NOT NULL,
    manager character varying(50),
    transferincome money NOT NULL,
    custincome money NOT NULL,
    wip_eoy money,
    wip_limit money,
    wip_current money,
    projectstatus character varying(50) NOT NULL,
    costbookno character varying(50),
    datecreated timestamp without time zone,
    feccost money,
    profit money,
    budget_cvl money,
    datecosted timestamp without time zone,
    disease character varying(50) NOT NULL,
    contract character varying(10) NOT NULL,
    projectparent character varying(50),
    shorttitle character varying(30),
    caseworksub numeric(5,4),
    pvsincome money,
    plancaseworkdebit money,
    finished smallint,
    owningrc character varying(50),
    comments text,
    carryover money,
    carryoverseed money,
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    jobcode character varying(20) NOT NULL,
    isdefraproject smallint,
    costcentre double precision,
    oracleprojectcode character varying(50),
    subaccountcode character varying(50),
    projectgroup character varying(50),
    incomeaccountcode character varying(50),
    fpsyear integer NOT NULL
);
-- Name: project_log_sequenceno_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.project_log_sequenceno_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: project_log_sequenceno_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.project_log_sequenceno_seq OWNED BY fps.project_log.sequenceno;
-- Name: project_log sequenceno; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.project_log ALTER COLUMN sequenceno SET DEFAULT nextval('fps.project_log_sequenceno_seq'::regclass);
-- Name: project_log pk_project_log; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.project_log
    ADD CONSTRAINT pk_project_log PRIMARY KEY (sequenceno, fpsyear);
-- Name: project_log_ind_dt; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX project_log_ind_dt ON fps.project_log USING btree (date_time);
-- Name: project_log_ind_jc; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX project_log_ind_jc ON fps.project_log USING btree (jobcode);
-- Name: project_log fk_project_log_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.project_log
    ADD CONSTRAINT fk_project_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
