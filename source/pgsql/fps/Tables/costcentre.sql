CREATE TABLE IF NOT EXISTS fps.costcentre (
    costcentre double precision NOT NULL,
    profitcentre character varying(50) NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_costcentre PRIMARY KEY (costcentre, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.costcentre_default PARTITION OF fps.costcentre
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.costcentre_y2016 PARTITION OF fps.costcentre
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.costcentre_y2017 PARTITION OF fps.costcentre
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.costcentre_y2018 PARTITION OF fps.costcentre
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.costcentre_y2019 PARTITION OF fps.costcentre
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.costcentre_y2020 PARTITION OF fps.costcentre
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.costcentre_y2021 PARTITION OF fps.costcentre
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.costcentre_y2022 PARTITION OF fps.costcentre
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.costcentre_y2023 PARTITION OF fps.costcentre
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.costcentre_y2024 PARTITION OF fps.costcentre
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.costcentre_y2025 PARTITION OF fps.costcentre
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.costcentre_y2026 PARTITION OF fps.costcentre
    FOR VALUES IN (2026);

-- Foreign keys for fps.costcentre
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_costcentre_fpsyear'
          AND conrelid = 'fps.costcentre'::regclass
    ) THEN
        ALTER TABLE fps.costcentre
            ADD CONSTRAINT fk_costcentre_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_costcentre_profitcentre'
          AND conrelid = 'fps.costcentre'::regclass
    ) THEN
        ALTER TABLE fps.costcentre
            ADD CONSTRAINT fk_costcentre_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre);
    END IF;
END $$;
