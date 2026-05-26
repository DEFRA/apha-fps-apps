-- Table: fps.tblstagingmonthlyTime
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblstagingmonthlyTime; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps."tblstagingmonthlyTime" (
    pactstaffid public.citext,
    timecode public.citext,
    parentproject public.citext,
    month double precision,
    workgroup public.citext,
    hours public.citext,
    failurecomments public.citext,
    passed boolean,
    pactid public.citext,
    newworkgroup public.citext,
    oldtestcode public.citext,
    name public.citext
);
