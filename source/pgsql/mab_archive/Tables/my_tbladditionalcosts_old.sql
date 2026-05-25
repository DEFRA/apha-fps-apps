-- Table: mabarchive.my_tbladditionalcosts_old
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tbladditionalcosts_old; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tbladditionalcosts_old (
    "Year" smallint NOT NULL,
    "JobCode" character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    "Account" character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    "Description" character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    "ItemCost" money NOT NULL,
    "Freq" character varying(5) COLLATE pg_catalog."und-x-icu",
    "Supplier" character varying(50) COLLATE pg_catalog."und-x-icu",
    "AC_Counter" integer NOT NULL
);
-- Name: my_tbladditionalcosts_AC_Counter_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive."my_tbladditionalcosts_AC_Counter_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: my_tbladditionalcosts_AC_Counter_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive."my_tbladditionalcosts_AC_Counter_seq" OWNED BY mabarchive.my_tbladditionalcosts_old."AC_Counter";
-- Name: my_tbladditionalcosts_old AC_Counter; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tbladditionalcosts_old ALTER COLUMN "AC_Counter" SET DEFAULT nextval('mabarchive."my_tbladditionalcosts_AC_Counter_seq"'::regclass);
