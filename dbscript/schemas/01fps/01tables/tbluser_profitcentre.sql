-- Table: fps.tbluser_profitcentre

CREATE TABLE fps.tbluser_profitcentre (
    profitcentre character varying(50) NOT NULL,
    user_id integer NOT NULL,
    fpsyear integer,
    CONSTRAINT pk__tbluser_profitce__77bfcb91 PRIMARY KEY (profitcentre, user_id)
);

