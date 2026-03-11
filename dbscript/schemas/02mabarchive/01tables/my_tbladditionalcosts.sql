-- Table: mabarchive.my_tbladditionalcosts

CREATE TABLE mabarchive.my_tbladditionalcosts (
    year smallint NOT NULL,
    jobcode character varying(20) NOT NULL,
    account character varying(50) NOT NULL,
    description character varying(20) NOT NULL,
    itemcost money NOT NULL,
    freq character varying(5),
    supplier character varying(50),
    ac_counter integer NOT NULL,
    CONSTRAINT pk_my_tbladditionalcosts PRIMARY KEY (ac_counter)
);

