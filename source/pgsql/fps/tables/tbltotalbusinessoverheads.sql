CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads (
    totalbusinessoverheads money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbltotalbusinessoverheads PRIMARY KEY (fpsyear),
    CONSTRAINT tb_pk UNIQUE (totalbusinessoverheads, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_default PARTITION OF fps.tbltotalbusinessoverheads
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2016 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2017 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2018 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2019 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2020 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2021 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2022 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2023 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2024 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2025 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tbltotalbusinessoverheads_y2026 PARTITION OF fps.tbltotalbusinessoverheads
    FOR VALUES IN (2026);

-- Foreign keys for fps.tbltotalbusinessoverheads
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltotalbusinessoverheads_fpsyear'
          AND conrelid = 'fps.tbltotalbusinessoverheads'::regclass
    ) THEN
        ALTER TABLE fps.tbltotalbusinessoverheads
            ADD CONSTRAINT fk_tbltotalbusinessoverheads_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
