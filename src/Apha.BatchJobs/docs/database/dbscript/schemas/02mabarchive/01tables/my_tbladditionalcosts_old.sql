-- Table: mabarchive.my_tbladditionalcosts_old

CREATE TABLE mabarchive.my_tbladditionalcosts_old (
    "Year" smallint NOT NULL,
    "JobCode" character varying(20) NOT NULL,
    "Account" character varying(50) NOT NULL,
    "Description" character varying(20) NOT NULL,
    "ItemCost" money NOT NULL,
    "Freq" character varying(5),
    "Supplier" character varying(50),
    "AC_Counter" integer DEFAULT nextval('mabarchive."my_tbladditionalcosts_AC_Counter_seq"'::regclass) NOT NULL
);

