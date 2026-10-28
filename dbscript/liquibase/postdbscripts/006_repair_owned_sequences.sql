--liquibase formatted sql

--changeset migration-fix:006-repair-owned-sequences labels:dml context:all splitStatements:false runInTransaction:true
--validCheckSum: ANY
-- Advance owned positive non-cycling sequences after an explicit-ID migration.
-- Writers and all sequence consumers must be stopped before this changeset.

SET LOCAL lock_timeout = '10s';
SET LOCAL statement_timeout = '30min';

DO $repair$
DECLARE
    counter RECORD;
    maximum_id bigint;
    last_value bigint;
    is_called boolean;
    increment_by bigint;
    maximum_value bigint;
    is_cycling boolean;
    current_next bigint;
    safe_next bigint;
BEGIN
    FOR counter IN
        SELECT namespace.nspname AS table_schema,
               relation.relname AS table_name,
               attribute.attname AS column_name,
               sequence_namespace.nspname AS sequence_schema,
               sequence_relation.relname AS sequence_name
        FROM pg_class relation
        JOIN pg_namespace namespace ON namespace.oid = relation.relnamespace
        JOIN pg_attribute attribute
          ON attribute.attrelid = relation.oid
         AND attribute.attnum > 0
         AND NOT attribute.attisdropped
        JOIN pg_class sequence_relation
          ON sequence_relation.oid = pg_get_serial_sequence(
                 format('%I.%I', namespace.nspname, relation.relname),
                 attribute.attname
             )::regclass
        JOIN pg_namespace sequence_namespace
          ON sequence_namespace.oid = sequence_relation.relnamespace
        WHERE relation.relkind IN ('r', 'p')
          AND namespace.nspname IN ('fps', 'mabarchive')
        ORDER BY namespace.nspname, relation.relname, attribute.attnum
    LOOP
        EXECUTE format(
            'SELECT seqincrement, seqmax, seqcycle FROM pg_sequence WHERE seqrelid = %L::regclass',
            format('%I.%I', counter.sequence_schema, counter.sequence_name))
            INTO increment_by, maximum_value, is_cycling;

        IF increment_by <= 0 OR is_cycling THEN
            RAISE EXCEPTION 'Unsupported sequence %.%',
                counter.sequence_schema, counter.sequence_name;
        END IF;

        EXECUTE format('SELECT last_value, is_called FROM %I.%I',
                       counter.sequence_schema, counter.sequence_name)
            INTO last_value, is_called;

        EXECUTE format('SELECT max(%I)::bigint FROM %I.%I',
                       counter.column_name, counter.table_schema, counter.table_name)
            INTO maximum_id;

        -- Only lock tables whose sequence is behind; re-read under the lock to avoid races.
        IF maximum_id IS NULL
           OR (CASE WHEN is_called THEN last_value + increment_by ELSE last_value END) > maximum_id THEN
            CONTINUE;
        END IF;

        EXECUTE format('LOCK TABLE %I.%I IN SHARE ROW EXCLUSIVE MODE',
                       counter.table_schema, counter.table_name);

        EXECUTE format('SELECT last_value, is_called FROM %I.%I',
                       counter.sequence_schema, counter.sequence_name)
            INTO last_value, is_called;

        EXECUTE format('SELECT max(%I)::bigint FROM %I.%I',
                       counter.column_name, counter.table_schema, counter.table_name)
            INTO maximum_id;

        current_next := CASE
            WHEN is_called THEN last_value + increment_by
            ELSE last_value
        END;
        safe_next := current_next;

        IF maximum_id IS NOT NULL AND safe_next <= maximum_id THEN
            safe_next := safe_next
                + ((maximum_id - safe_next) / increment_by + 1) * increment_by;
        END IF;

        IF safe_next > maximum_value THEN
            RAISE EXCEPTION 'Sequence %.% has no available value above %.%',
                counter.sequence_schema, counter.sequence_name,
                counter.table_schema, counter.table_name;
        END IF;

        IF safe_next > current_next THEN
            RAISE NOTICE 'Advancing %.% from % to %',
                counter.sequence_schema, counter.sequence_name,
                current_next, safe_next;
            EXECUTE format('ALTER SEQUENCE %I.%I RESTART WITH %s',
                           counter.sequence_schema, counter.sequence_name, safe_next);
        END IF;
    END LOOP;
END
$repair$;
