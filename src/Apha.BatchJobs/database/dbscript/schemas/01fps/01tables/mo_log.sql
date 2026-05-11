-- Table: fps.mo_log

CREATE TABLE fps.mo_log (
    sequenceno integer GENERATED ALWAYS AS IDENTITY NOT NULL,
    testcode character varying(20),
    buyer character varying(20),
    month double precision,
    workgroup character varying(50),
    volume double precision,
    wgbuyer character varying(50),
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    fpsyear integer
);

COMMENT ON TABLE fps.mo_log IS $$Note: PostgreSQL does not support column-level collations. The original SQL Server collation was Latin1_General_CI_AS.$$;

COMMENT ON COLUMN fps.mo_log.sequenceno IS $$This column uses GENERATED ALWAYS AS IDENTITY, which is equivalent to IDENTITY in SQL Server.$$;
