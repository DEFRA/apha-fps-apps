-- Table: fps.tblstagingpurchaselocal
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblstagingpurchaselocal; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblstagingpurchaselocal (
    workgroup public.citext,
    account public.citext,
    itemdescription character varying(50),
    amount money
);
