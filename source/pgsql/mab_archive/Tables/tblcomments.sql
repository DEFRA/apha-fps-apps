-- Table: mabarchive.tblcomments
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblcomments; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblcomments (
    commentno integer NOT NULL,
    project character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    year smallint NOT NULL,
    dateentered timestamp without time zone,
    topic character varying(25) NOT NULL COLLATE pg_catalog."und-x-icu",
    comment text COLLATE pg_catalog."und-x-icu",
    madeby character(20) COLLATE pg_catalog."und-x-icu"
);
-- Name: tblcomments_commentno_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.tblcomments_commentno_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tblcomments_commentno_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.tblcomments_commentno_seq OWNED BY mabarchive.tblcomments.commentno;
-- Name: tblcomments commentno; Type: DEFAULT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblcomments ALTER COLUMN commentno SET DEFAULT nextval('mabarchive.tblcomments_commentno_seq'::regclass);
-- Name: tblcomments ix_tblcomments; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblcomments
    ADD CONSTRAINT ix_tblcomments UNIQUE (project, year, topic);
-- Name: tblcomments pk_tblcomments; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblcomments
    ADD CONSTRAINT pk_tblcomments PRIMARY KEY (commentno);
-- Name: tblcomments_commentno_idx; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tblcomments_commentno_idx ON mabarchive.tblcomments USING btree (commentno);
