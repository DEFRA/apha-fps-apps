-- Table: fps.tblstagingmonthlyoutput
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblstagingmonthlyoutput; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblstagingmonthlyoutput (
    testcode public.citext NOT NULL,
    buyer public.citext NOT NULL,
    month double precision NOT NULL,
    workgroup public.citext NOT NULL,
    volume double precision,
    failurecomments public.citext,
    passed boolean
);
