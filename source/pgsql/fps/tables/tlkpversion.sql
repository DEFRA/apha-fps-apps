CREATE TABLE IF NOT EXISTS fps.tlkpversion (
    version integer,
    x bytea,
    islive integer,
    CONSTRAINT version_pk UNIQUE (version)
);
