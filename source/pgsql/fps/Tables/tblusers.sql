-- Table: fps.tblusers
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblusers; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblusers (
    user_id integer NOT NULL,
    username character varying(50) COLLATE public.latin1_general_ci_as,
    agencyid integer,
    frmwarning boolean DEFAULT false NOT NULL,
    comments character varying(255) COLLATE public.latin1_general_ci_as,
    dt2username character varying(50) COLLATE public.latin1_general_ci_as,
    useremail character varying(255) COLLATE public.latin1_general_ci_as
);
-- Name: tblusers_user_id_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.tblusers_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tblusers_user_id_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.tblusers_user_id_seq OWNED BY fps.tblusers.user_id;
-- Name: tblusers user_id; Type: DEFAULT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblusers ALTER COLUMN user_id SET DEFAULT nextval('fps.tblusers_user_id_seq'::regclass);
-- Name: tblusers pk__tblusers__1367e606; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblusers
    ADD CONSTRAINT pk__tblusers__1367e606 PRIMARY KEY (user_id);
-- Name: tblusers username; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblusers
    ADD CONSTRAINT username UNIQUE (username);
