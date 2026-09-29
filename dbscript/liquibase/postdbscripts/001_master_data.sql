--liquibase formatted sql

--changeset migration-fix:001-master-data labels:dml context:all

-- fpsyear-aware: one row per (profitcentre, fpsyear) so this never seeds a -1 sentinel row (CR089).
-- tblyearmaster is seeded in the changesets phase (cr033), so it is populated before this runs.
INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division, conttarget, profitcentrehead, divisionid, email_recipient, timesheetlayout, timesheet, outputsheet, pactcoordinatoremailname, fpsyear)
SELECT v.profitcentre, v.profitcentrename, v.division, v.conttarget, v.profitcentrehead, v.divisionid, v.email_recipient, v.timesheetlayout, v.timesheet, v.outputsheet, v.pactcoordinatoremailname, y.fpsyear
FROM (VALUES
    ('MVSD', 'Midlands VSD', 'Surv', 0.00::numeric, NULL::text, 5, NULL::text, NULL::integer, NULL::integer, NULL::integer, NULL::text),
    ('Outbreak', 'Outbreak', 'Surv', 0.00::numeric, NULL::text, 5, NULL::text, NULL::integer, NULL::integer, NULL::integer, NULL::text),
    ('PRISM', 'Stores', 'R&D', 0.00::numeric, 'Drinkel, Karie', 2, NULL::text, 2, -1, NULL::integer, NULL::text),
    ('SEVSD', 'South-East VSD', 'Surv', 0.00::numeric, NULL::text, 5, NULL::text, NULL::integer, NULL::integer, NULL::integer, NULL::text)
) AS v(profitcentre, profitcentrename, division, conttarget, profitcentrehead, divisionid, email_recipient, timesheetlayout, timesheet, outputsheet, pactcoordinatoremailname)
CROSS JOIN fps.tblyearmaster y
WHERE NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre p WHERE p.profitcentre = v.profitcentre AND p.fpsyear = y.fpsyear);

INSERT INTO fps.tlkpaccountcode (code, description)
SELECT v.code, v.description
FROM (VALUES
    ('3501', 'Laboratory Services/Diagnostic Testing'),
    ('3502', 'Consultancy'),
    ('3503', 'Research & Development'),
    ('3504', 'Products'),
    ('3505', 'Patents & Royalties'),
    ('3506', 'Surveillance services'),
    ('3507', 'Core Facility Services'),
    ('3508', 'Sundries'),
    ('3510', 'Laboratory Surveillance')
) AS v(code, description)
WHERE NOT EXISTS (SELECT 1 FROM fps.tlkpaccountcode a WHERE a.code = v.code);

INSERT INTO fps.tlkpcustomer (customer)
SELECT v.customer
FROM (VALUES
    ('MSD Animal Health ( Merck Sharpe & Dohne)'),
    ('PSD')
) AS v(customer)
WHERE NOT EXISTS (SELECT 1 FROM fps.tlkpcustomer c WHERE c.customer = v.customer);

INSERT INTO fps.tbldirectorate (directorate)
SELECT v.directorate
FROM (VALUES
    ('BSD'), ('EU Exit'), ('Finance'), ('HNC'), ('IMT'), ('Inspectorates'),
    ('N/A'), ('NATBORD'), ('Operations'), ('Science'), ('SEVBC')
) AS v(directorate)
WHERE NOT EXISTS (SELECT 1 FROM fps.tbldirectorate d WHERE d.directorate = v.directorate);

