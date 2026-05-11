-- Table: fps.tbluser_testowner

CREATE TABLE fps.tbluser_testowner (
    user_id integer NOT NULL,
    test_owner character varying(2) NOT NULL,
    fpsyear integer,
    CONSTRAINT pk___1__25 PRIMARY KEY (test_owner, user_id)
);

