-- Table: fps.tlkpversion

CREATE TABLE fps.tlkpversion (
    version integer,
    x bytea,
    islive integer,
    CONSTRAINT version_pk UNIQUE (version)
);

