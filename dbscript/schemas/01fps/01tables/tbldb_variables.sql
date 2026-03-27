-- Table: fps.tbldb_variables

CREATE TABLE fps.tbldb_variables (
    db_var_name character varying(20) NOT NULL,
    db_var_value character varying(20),
    CONSTRAINT pk_tbldb_variables PRIMARY KEY (db_var_name)
);

