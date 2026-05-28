CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory (
    accshortname character varying(50) NOT NULL,
    accountdescription character varying(50),
    constituentaccountcodes character varying(100),
    accounttype character varying(10) NOT NULL,
    projectspecific integer,
    rcspecific integer,
    csg7_group character(15),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblkpaccountcategory PRIMARY KEY (accshortname, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_default PARTITION OF fps.tblkpaccountcategory
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2016 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2017 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2018 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2019 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2020 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2021 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2022 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2023 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2024 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2025 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblkpaccountcategory_y2026 PARTITION OF fps.tblkpaccountcategory
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblkpaccountcategory
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblkpaccountcategory_fpsyear'
          AND conrelid = 'fps.tblkpaccountcategory'::regclass
    ) THEN
        ALTER TABLE fps.tblkpaccountcategory
            ADD CONSTRAINT fk_tblkpaccountcategory_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
