-- Table: fps.period_monthlyoutput

CREATE TABLE fps.period_monthlyoutput (
    id integer DEFAULT nextval('fps.period_monthlyoutput_id_seq'::regclass) NOT NULL,
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
    totalcost money,
    CONSTRAINT pk_period_monthlyoutput_1 PRIMARY KEY (id)
);

COMMENT ON COLUMN fps.period_monthlyoutput.id IS $$Converted from IDENTITY(1,1) to SERIAL$$;
COMMENT ON COLUMN fps.period_monthlyoutput.project IS $$Original collation: Latin1_General_CI_AS$$;
COMMENT ON COLUMN fps.period_monthlyoutput.oracleprojectcode IS $$Original collation: Latin1_General_CI_AS$$;
COMMENT ON COLUMN fps.period_monthlyoutput.subaccountcode IS $$Original collation: Latin1_General_CI_AS$$;
COMMENT ON COLUMN fps.period_monthlyoutput.isdefraproject IS $$Original collation: Latin1_General_CI_AS$$;
COMMENT ON COLUMN fps.period_monthlyoutput.opc IS $$Original collation: Latin1_General_CI_AS$$;
COMMENT ON COLUMN fps.period_monthlyoutput.spc IS $$Original collation: Latin1_General_CI_AS$$;
COMMENT ON COLUMN fps.period_monthlyoutput.workgroup IS $$Original collation: Latin1_General_CI_AS$$;
COMMENT ON COLUMN fps.period_monthlyoutput.testcode IS $$Original collation: Latin1_General_CI_AS$$;
