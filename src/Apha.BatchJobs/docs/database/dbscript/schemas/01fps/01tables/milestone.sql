-- Table: fps.milestone

CREATE TABLE fps.milestone (
    project citext NOT NULL,
    milestoneref character varying(4) NOT NULL,
    objectiveref character varying(50) NOT NULL,
    milsetonetitle character varying(120),
    plandate date,
    actualdate date,
    comment text,
    monthnofin double precision,
    year character varying(50),
    fpsyear integer,
    CONSTRAINT pk_milestone_1__12 PRIMARY KEY (project, milestoneref, objectiveref),
    CONSTRAINT fk_milestone_project FOREIGN KEY (project) REFERENCES fps.tlkpproject(parentproject)
);

COMMENT ON TABLE fps.milestone IS $$Milestone information$$;

COMMENT ON COLUMN fps.milestone.project IS $$Project identifier$$;
COMMENT ON COLUMN fps.milestone.milestoneref IS $$Milestone reference$$;
COMMENT ON COLUMN fps.milestone.objectiveref IS $$Objective reference$$;
COMMENT ON COLUMN fps.milestone.milsetonetitle IS $$Milestone title$$;
COMMENT ON COLUMN fps.milestone.plandate IS $$Planned date$$;
COMMENT ON COLUMN fps.milestone.actualdate IS $$Actual date$$;
COMMENT ON COLUMN fps.milestone.comment IS $$Additional comments$$;
COMMENT ON COLUMN fps.milestone.monthnofin IS $$Month number (financial)$$;
COMMENT ON COLUMN fps.milestone.year IS $$Year$$;
