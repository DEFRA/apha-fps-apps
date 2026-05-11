-- Table: fps.tbluser_program

CREATE TABLE fps.tbluser_program (
    user_id integer NOT NULL,
    programno character varying(10) NOT NULL,
    fpsyear integer,
    CONSTRAINT pk__tbluser_program__26afc4a4 PRIMARY KEY (programno, user_id)
);

