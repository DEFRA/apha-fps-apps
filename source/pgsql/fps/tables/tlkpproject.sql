CREATE TABLE IF NOT EXISTS fps.tlkpproject (
    parentproject character varying(20) NOT NULL,
    projecttitle character varying(200) NOT NULL,
    program character varying(10) NOT NULL,
    customer character varying(50) NOT NULL,
    manager character varying(50),
    transferincome money NOT NULL,
    custincome money NOT NULL,
    wip_eoy money DEFAULT 0,
    wip_limit money,
    wip_current money,
    projectstatus character varying(50) NOT NULL,
    costbookno character varying(50),
    datecreated timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    feccost money DEFAULT 0,
    profit money DEFAULT 0,
    budget_cvl money DEFAULT 0,
    datecosted timestamp without time zone,
    disease character varying(50) NOT NULL,
    contract character varying(10) DEFAULT 0 NOT NULL,
    projectparent character varying(50),
    shorttitle character varying(30),
    caseworksub numeric(5,4),
    pvsincome money,
    plancaseworkdebit money,
    finished smallint DEFAULT 0,
    owningrc character varying(50),
    comments text,
    carryover money,
    carryoverseed money,
    isdefraproject smallint NOT NULL,
    costcentre double precision,
    oracleprojectcode character varying(50),
    subaccountcode character varying(50),
    projectgroup character varying(50),
    incomeaccountcode character varying(50) NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tlkpproject PRIMARY KEY (parentproject, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_default PARTITION OF fps.tlkpproject
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2016 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2017 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2018 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2019 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2020 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2021 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2022 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2023 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2024 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2025 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tlkpproject_y2026 PARTITION OF fps.tlkpproject
    FOR VALUES IN (2026);

-- Foreign keys for fps.tlkpproject
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_1__10'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_1__10 FOREIGN KEY (projectstatus) REFERENCES fps.tblstatus(status);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_1__16'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_1__16 FOREIGN KEY (customer) REFERENCES fps.tlkpcustomer(customer);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_1'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_1 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2016(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_10'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_10 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2025(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_11'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_11 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2026(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_12'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_12 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_default(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_2'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_2 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2017(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_3'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_3 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2018(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_4'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_4 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2019(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_5'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_5 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2020(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_6'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_6 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2021(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_7'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_7 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2022(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_8'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_8 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2023(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_contract_9'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_contract_9 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2024(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_disease'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_disease FOREIGN KEY (disease) REFERENCES fps.tbldisease(disease);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_fpsyear'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_incomeaccountcode'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_incomeaccountcode FOREIGN KEY (incomeaccountcode) REFERENCES fps.tlkpaccountcode(code);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_1'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_1 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2016(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_10'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_10 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2025(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_11'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_11 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2026(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_12'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_12 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_default(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_2'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_2 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2017(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_3'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_3 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2018(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_4'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_4 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2019(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_5'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_5 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2020(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_6'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_6 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2021(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_7'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_7 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2022(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_8'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_8 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2023(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_program_9'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_program_9 FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram_y2024(programno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_1'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_1 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2016(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_10'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_10 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2025(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_11'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_11 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2026(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_12'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_12 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_default(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_2'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_2 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2017(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_3'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_3 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2018(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_4'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_4 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2019(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_5'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_5 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2020(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_6'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_6 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2021(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_7'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_7 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2022(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_8'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_8 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2023(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_projectgroup_9'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_projectgroup_9 FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup_y2024(projectgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpproject_subaccountcode'
          AND conrelid = 'fps.tlkpproject'::regclass
    ) THEN
        ALTER TABLE fps.tlkpproject
            ADD CONSTRAINT fk_tlkpproject_subaccountcode FOREIGN KEY (subaccountcode) REFERENCES fps.tlkpsubaccount(subaccountcode);
    END IF;
END $$;
