--liquibase formatted sql

--changeset migration-fix:005-encoding-normalization labels:dml context:all splitStatements:false runInTransaction:true
-- Correct only the three source-encoding values approved in the manual repair.
-- The changeset aborts unless each value is either entirely pending or complete.

SET LOCAL lock_timeout = '10s';
SET LOCAL statement_timeout = '30min';

CREATE TEMP TABLE corrupt_key_fix (
    parent_schema text NOT NULL,
    parent_table text NOT NULL,
    parent_column text NOT NULL,
    old_value text NOT NULL,
    new_value text NOT NULL,
    PRIMARY KEY (parent_schema, parent_table, parent_column, old_value)
) ON COMMIT DROP;

INSERT INTO corrupt_key_fix VALUES
    ('fps', 'tlkpcustomer', 'customer',
     'People' || chr(146) || 's Trust for Endangered Species',
     'People' || chr(8217) || 's Trust for Endangered Species'),
    ('fps', 'tlkpcustomer', 'customer',
     'QMUL ' || chr(150) || ' Queen Mary, University of London',
     'QMUL ' || chr(8211) || ' Queen Mary, University of London'),
    ('mabarchive', 'g_tlkpproject', 'parentproject',
     chr(133), chr(8230));

DO $check$
DECLARE
    fix record;
    old_count bigint;
    new_count bigint;
BEGIN
    FOR fix IN SELECT * FROM corrupt_key_fix LOOP
        EXECUTE format('SELECT count(*) FROM %I.%I WHERE %I = $1',
                       fix.parent_schema, fix.parent_table, fix.parent_column)
            INTO old_count USING fix.old_value;
        EXECUTE format('SELECT count(*) FROM %I.%I WHERE %I = $1',
                       fix.parent_schema, fix.parent_table, fix.parent_column)
            INTO new_count USING fix.new_value;
        IF NOT ((old_count = 1 AND new_count = 0) OR (old_count = 0 AND new_count = 1)) THEN
            RAISE EXCEPTION 'Unexpected old/new counts in %.%.%; old=% new=%',
                fix.parent_schema, fix.parent_table, fix.parent_column, old_count, new_count;
        END IF;
    END LOOP;
END
$check$;

-- Add replacement customer parents before changing their FK children.
INSERT INTO fps.tlkpcustomer (customer)
SELECT new_value
FROM corrupt_key_fix fix
WHERE fix.parent_schema = 'fps'
  AND fix.parent_table = 'tlkpcustomer'
  AND EXISTS (SELECT 1 FROM fps.tlkpcustomer p WHERE p.customer = fix.old_value)
  AND NOT EXISTS (SELECT 1 FROM fps.tlkpcustomer p WHERE p.customer = fix.new_value);

-- Update every declared FK child of fps.tlkpcustomer.customer.
DO $children$
DECLARE
    child record;
    fix record;
BEGIN
    FOR fix IN
        SELECT * FROM corrupt_key_fix
        WHERE parent_schema = 'fps'
          AND parent_table = 'tlkpcustomer'
    LOOP
        FOR child IN
            SELECT DISTINCT child_ns.nspname AS child_schema,
                            child_rel.relname AS child_table,
                            child_attr.attname AS child_column
            FROM pg_constraint con
            JOIN pg_class child_rel ON child_rel.oid = con.conrelid
            JOIN pg_namespace child_ns ON child_ns.oid = child_rel.relnamespace
            JOIN pg_class parent_rel ON parent_rel.oid = con.confrelid
            JOIN pg_namespace parent_ns ON parent_ns.oid = parent_rel.relnamespace
            JOIN unnest(con.conkey) WITH ORDINALITY child_key(attnum, ord) ON true
            JOIN unnest(con.confkey) WITH ORDINALITY parent_key(attnum, ord)
              ON parent_key.ord = child_key.ord
            JOIN pg_attribute child_attr
              ON child_attr.attrelid = child_rel.oid
             AND child_attr.attnum = child_key.attnum
            JOIN pg_attribute parent_attr
              ON parent_attr.attrelid = parent_rel.oid
             AND parent_attr.attnum = parent_key.attnum
            WHERE con.contype = 'f'
              AND parent_ns.nspname = fix.parent_schema
              AND parent_rel.relname = fix.parent_table
              AND parent_attr.attname = fix.parent_column
        LOOP
            EXECUTE format('UPDATE %I.%I SET %I = $1 WHERE %I = $2',
                           child.child_schema, child.child_table,
                           child.child_column, child.child_column)
                USING fix.new_value, fix.old_value;
        END LOOP;
    END LOOP;
END
$children$;

DELETE FROM fps.tlkpcustomer parent
USING corrupt_key_fix fix
WHERE fix.parent_schema = 'fps'
  AND fix.parent_table = 'tlkpcustomer'
  AND parent.customer = fix.old_value;

UPDATE mabarchive.g_tlkpproject
SET parentproject = chr(8230)
WHERE parentproject = chr(133);

DO $verify$
DECLARE
    fix record;
    old_count bigint;
    new_count bigint;
BEGIN
    FOR fix IN SELECT * FROM corrupt_key_fix LOOP
        EXECUTE format('SELECT count(*) FROM %I.%I WHERE %I = $1',
                       fix.parent_schema, fix.parent_table, fix.parent_column)
            INTO old_count USING fix.old_value;
        EXECUTE format('SELECT count(*) FROM %I.%I WHERE %I = $1',
                       fix.parent_schema, fix.parent_table, fix.parent_column)
            INTO new_count USING fix.new_value;
        IF old_count <> 0 OR new_count <> 1 THEN
            RAISE EXCEPTION 'Verification failed for %.%.%; old=% new=%',
                fix.parent_schema, fix.parent_table, fix.parent_column, old_count, new_count;
        END IF;
    END LOOP;
END
$verify$;
