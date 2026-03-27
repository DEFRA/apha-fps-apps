-- Table: fps.tbluser_category

CREATE TABLE fps.tbluser_category (
    user_id integer NOT NULL,
    category character varying(20) NOT NULL,
    fpsyear integer,
    CONSTRAINT pk___6__10 PRIMARY KEY (user_id, category)
);

