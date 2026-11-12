--liquibase formatted sql

--changeset migration-fix:002-master-data-gaps labels:dml context:all
--comment: Parent values referenced by existing child rows but missing from the lookup tables; must run before 003_key_normalization.sql, whose FK verification fails otherwise.

INSERT INTO fps.tbldirectorate (directorate)
SELECT v.directorate
FROM (VALUES
    ('CSG'), ('PAS'), ('Lab Services'), ('Nat Bord')
) AS v(directorate)
WHERE NOT EXISTS (SELECT 1 FROM fps.tbldirectorate d WHERE d.directorate = v.directorate);

INSERT INTO fps.tlkpcustomer (customer)
SELECT v.customer
FROM (VALUES
    ('Elanco'),
    ('eu'),
    ('various'),
    ('Wildlife and Wetlands Trust'),
    ('NOT Specified'),
    ('ANSES'),
    ('Bioreliance')
) AS v(customer)
WHERE NOT EXISTS (SELECT 1 FROM fps.tlkpcustomer c WHERE c.customer = v.customer);

INSERT INTO fps.tbldisease (disease)
SELECT v.disease
FROM (VALUES
    ('PPR peste des petits ruminants virus'),
    ('Mastitis')
) AS v(disease)
WHERE NOT EXISTS (SELECT 1 FROM fps.tbldisease d WHERE d.disease = v.disease);

-- '120404' is '1020404' (Internal) with a dropped zero; not a valid tlkpsubaccount code.
UPDATE fps.tlkpproject
SET subaccountcode = '1020404'
WHERE subaccountcode = '120404';

--rollback DELETE FROM fps.tbldisease WHERE disease IN ('PPR peste des petits ruminants virus', 'Mastitis');
--rollback DELETE FROM fps.tlkpcustomer WHERE customer IN ('Elanco', 'eu', 'various', 'Wildlife and Wetlands Trust', 'NOT Specified', 'ANSES', 'Bioreliance');
--rollback DELETE FROM fps.tbldirectorate WHERE directorate IN ('CSG', 'PAS', 'Lab Services', 'Nat Bord');
