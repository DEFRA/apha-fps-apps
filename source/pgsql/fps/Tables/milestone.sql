-- Table: fps.milestone
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: milestone; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.milestone (
    project public.citext NOT NULL,
    milestoneref character varying(4) NOT NULL COLLATE public.latin1_general_ci_as,
    objectiveref character varying(50) NOT NULL COLLATE public.latin1_general_ci_as,
    milsetonetitle character varying(120) COLLATE public.latin1_general_ci_as,
    plandate date,
    actualdate date,
    comment text,
    monthnofin double precision,
    year character varying(50) COLLATE public.latin1_general_ci_as,
    fpsyear integer NOT NULL
);
-- Name: TABLE milestone; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON TABLE fps.milestone IS 'Milestone information';
-- Name: COLUMN milestone.project; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.milestone.project IS 'Project identifier';
-- Name: COLUMN milestone.milestoneref; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.milestone.milestoneref IS 'Milestone reference';
-- Name: COLUMN milestone.objectiveref; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.milestone.objectiveref IS 'Objective reference';
-- Name: COLUMN milestone.milsetonetitle; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.milestone.milsetonetitle IS 'Milestone title';
-- Name: COLUMN milestone.plandate; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.milestone.plandate IS 'Planned date';
-- Name: COLUMN milestone.actualdate; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.milestone.actualdate IS 'Actual date';
-- Name: COLUMN milestone.comment; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.milestone.comment IS 'Additional comments';
-- Name: COLUMN milestone.monthnofin; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.milestone.monthnofin IS 'Month number (financial)';
-- Name: COLUMN milestone.year; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.milestone.year IS 'Year';
-- Name: milestone pk_milestone; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.milestone
    ADD CONSTRAINT pk_milestone PRIMARY KEY (project, milestoneref, objectiveref, fpsyear);
-- Name: milestone fk_milestone_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.milestone
    ADD CONSTRAINT fk_milestone_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: milestone fk_milestone_project; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.milestone
    ADD CONSTRAINT fk_milestone_project FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
