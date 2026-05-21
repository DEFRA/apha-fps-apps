CREATE TABLE IF NOT EXISTS mabarchive.tblaccesssystems (
    systemid integer NOT NULL,
    systemname character varying(50) NOT NULL,
    CONSTRAINT pk_tblaccesssystems PRIMARY KEY (systemid)
);
