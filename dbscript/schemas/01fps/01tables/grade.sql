-- Table: fps.grade

CREATE TABLE fps.grade (
    gradecode citext NOT NULL,
    desc_long character varying(30),
    avsalary money DEFAULT 0,
    pactcode character varying(50),
    avleavehrs double precision DEFAULT 0,
    avsickhrs double precision DEFAULT 0,
    fpsyear integer,
    CONSTRAINT pk__grade__1ab40213 PRIMARY KEY (gradecode)
);

