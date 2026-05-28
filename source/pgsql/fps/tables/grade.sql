CREATE TABLE IF NOT EXISTS fps.grade (
    gradecode character varying(10) NOT NULL,
    desc_long character varying(30),
    avsalary money DEFAULT 0,
    pactcode character varying(50),
    avleavehrs double precision DEFAULT 0,
    avsickhrs double precision DEFAULT 0,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_grade PRIMARY KEY (gradecode, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.grade_default PARTITION OF fps.grade
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.grade_y2016 PARTITION OF fps.grade
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.grade_y2017 PARTITION OF fps.grade
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.grade_y2018 PARTITION OF fps.grade
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.grade_y2019 PARTITION OF fps.grade
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.grade_y2020 PARTITION OF fps.grade
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.grade_y2021 PARTITION OF fps.grade
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.grade_y2022 PARTITION OF fps.grade
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.grade_y2023 PARTITION OF fps.grade
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.grade_y2024 PARTITION OF fps.grade
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.grade_y2025 PARTITION OF fps.grade
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.grade_y2026 PARTITION OF fps.grade
    FOR VALUES IN (2026);

-- Foreign keys for fps.grade
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_grade_fpsyear'
          AND conrelid = 'fps.grade'::regclass
    ) THEN
        ALTER TABLE fps.grade
            ADD CONSTRAINT fk_grade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
