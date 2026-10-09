--liquibase formatted sql

--changeset migration-fix:008-cp1252-controls-all-columns labels:dml context:all splitStatements:false runInTransaction:true
-- Repair Windows-1252 characters that the ISO-8859-1 migration stored as C1 control
-- characters (U+0080-U+009F), in every non-key text column of fps and mabarchive.
-- Covers the tables 007 does not and all 27 cp1252 characters (007 maps 9).
-- Safe to re-run: columns without control characters are not touched.
-- Primary/foreign/unique key columns are skipped and reported; they need the
-- parent-then-children handling used in 005.

SET LOCAL lock_timeout = '10s';
SET LOCAL statement_timeout = '30min';

DO $repair$
DECLARE
    -- cp1252 bytes 0x80-0x9F and the Unicode characters they stand for; 0x81, 0x8D, 0x8F, 0x90, 0x9D are undefined
    src_codes int[] := ARRAY[128, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 142,
                             145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 158, 159];
    dst_codes int[] := ARRAY[8364, 8218, 402, 8222, 8230, 8224, 8225, 710, 8240, 352, 8249, 338, 381,
                             8216, 8217, 8220, 8221, 8226, 8211, 8212, 732, 8482, 353, 8250, 339, 382, 376];
    from_chars text := '';
    to_chars text := '';
    pattern text;
    tbl record;
    fix_cond text;
    set_list text;
    key_counts text;
    fix_rows bigint;
    key_rows bigint[];
    affected bigint;
    total_rows bigint := 0;
    total_tables int := 0;
    skipped_keys int := 0;
    i int;
BEGIN
    FOR i IN 1 .. array_length(src_codes, 1) LOOP
        from_chars := from_chars || chr(src_codes[i]);
        to_chars := to_chars || chr(dst_codes[i]);
    END LOOP;
    pattern := '[' || from_chars || ']';

    FOR tbl IN
        SELECT col.schema_name, col.table_name,
               array_agg(col.column_name ORDER BY col.attnum) FILTER (WHERE NOT col.is_key) AS fix_cols,
               array_agg(col.column_name ORDER BY col.attnum) FILTER (WHERE col.is_key) AS key_cols
        FROM (
            SELECT n.nspname AS schema_name, c.relname AS table_name, a.attname AS column_name, a.attnum,
                   EXISTS (
                       SELECT 1 FROM pg_constraint k
                       WHERE k.conrelid = c.oid AND k.contype IN ('p', 'f', 'u') AND a.attnum = ANY (k.conkey)
                   ) AS is_key
            FROM pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            JOIN pg_attribute a ON a.attrelid = c.oid
            WHERE n.nspname IN ('fps', 'mabarchive')
              AND c.relkind IN ('r', 'p')
              AND NOT c.relispartition
              AND a.attnum > 0
              AND NOT a.attisdropped
              AND a.attgenerated = ''
              AND a.atttypid IN ('text'::regtype, 'varchar'::regtype, 'bpchar'::regtype)
        ) col
        GROUP BY col.schema_name, col.table_name
        ORDER BY 1, 2
    LOOP
        SELECT string_agg(format('%I ~ $1', c), ' OR '),
               string_agg(format('%I = translate(%I, $2, $3)', c, c), ', ')
        INTO fix_cond, set_list
        FROM unnest(tbl.fix_cols) AS c;

        SELECT 'ARRAY[' || string_agg(format('count(*) FILTER (WHERE %I ~ $1)', k), ', ') || ']::bigint[]'
        INTO key_counts
        FROM unnest(tbl.key_cols) AS k;

        -- One read per table: rows to repair plus per-key-column counts
        EXECUTE format('SELECT count(*) FILTER (WHERE %s), %s FROM %I.%I',
                       coalesce(fix_cond, 'false'), coalesce(key_counts, 'ARRAY[]::bigint[]'),
                       tbl.schema_name, tbl.table_name)
            INTO fix_rows, key_rows USING pattern;

        FOR i IN 1 .. coalesce(array_length(tbl.key_cols, 1), 0) LOOP
            IF key_rows[i] > 0 THEN
                skipped_keys := skipped_keys + 1;
                RAISE WARNING 'SKIPPED key column %.%.% has % row(s) with cp1252 control characters',
                    tbl.schema_name, tbl.table_name, tbl.key_cols[i], key_rows[i];
            END IF;
        END LOOP;

        CONTINUE WHEN fix_rows = 0;

        EXECUTE format('UPDATE %I.%I SET %s WHERE %s', tbl.schema_name, tbl.table_name, set_list, fix_cond)
            USING pattern, from_chars, to_chars;
        GET DIAGNOSTICS affected = ROW_COUNT;

        -- translate() removes every pattern character, so a matching row count proves the table is clean
        IF affected <> fix_rows THEN
            RAISE EXCEPTION 'Verification failed: %.% expected % row(s) to repair, updated %',
                tbl.schema_name, tbl.table_name, fix_rows, affected;
        END IF;

        total_rows := total_rows + affected;
        total_tables := total_tables + 1;
        RAISE NOTICE '%.%: repaired % row(s)', tbl.schema_name, tbl.table_name, affected;
    END LOOP;

    RAISE NOTICE 'PASS - repaired % row(s) across % table(s); % key column(s) skipped',
        total_rows, total_tables, skipped_keys;
END
$repair$;

--ROLLBACK
--Not reversible: the original control characters are not kept. Restore from backup if needed.
