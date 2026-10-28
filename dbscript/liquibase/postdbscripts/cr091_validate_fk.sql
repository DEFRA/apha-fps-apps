--liquibase formatted sql

--changeset migration-fix:004-fk-validation labels:dml context:all runAlways:true runOnChange:true splitStatements:false runInTransaction:true
-- Phase 5 - Migration gate. Run this after every data migration.
--
-- Rebuilds the FK inventory from the catalog, re-runs the orphan checks, and fails
-- loudly if anything is broken. Intended to be wired into the migration job so a bad
-- load is caught immediately instead of surfacing months later during a restore.
--
-- This file is plain SQL and can be run directly in pgAdmin.

DO $guard$
DECLARE
    unvalidated_count bigint;
BEGIN
    IF current_setting('session_replication_role') <> 'origin' THEN
        RAISE EXCEPTION 'FAIL - foreign-key enforcement is not enabled for this session. session_replication_role=%',
            current_setting('session_replication_role');
    END IF;

    SELECT count(*)
      INTO unvalidated_count
      FROM pg_constraint c
      JOIN pg_class r ON r.oid = c.conrelid
      JOIN pg_namespace n ON n.oid = r.relnamespace
     WHERE c.contype = 'f'
       AND c.conparentid = 0
       AND n.nspname IN ('fps', 'mabarchive')
       AND NOT c.convalidated;

    IF unvalidated_count > 0 THEN
        RAISE EXCEPTION 'FAIL - % foreign-key constraint(s) are NOT VALIDATED.', unvalidated_count;
    END IF;

    RAISE NOTICE 'PASS - foreign-key enforcement is enabled: session_replication_role=origin.';
    RAISE NOTICE 'PASS - all foreign-key constraints are validated.';
END
$guard$;

CREATE SCHEMA IF NOT EXISTS audit;

DROP TABLE IF EXISTS audit.orphan_report CASCADE;
DROP TABLE IF EXISTS audit.fk_inventory CASCADE;

CREATE TABLE audit.fk_inventory (
    fk_id          integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    conname        text NOT NULL,
    child_schema   text NOT NULL,
    child_table    text NOT NULL,
    child_cols     text[] NOT NULL,
    parent_schema  text NOT NULL,
    parent_table   text NOT NULL,
    parent_cols    text[] NOT NULL,
    convalidated   boolean NOT NULL,
    condeferrable  boolean NOT NULL,
    condeferred    boolean NOT NULL,
    confmatchtype  "char" NOT NULL,
    on_update      "char" NOT NULL,
    on_delete      "char" NOT NULL,
    UNIQUE (conname, child_schema, child_table)
);

INSERT INTO audit.fk_inventory (
    conname, child_schema, child_table, child_cols,
    parent_schema, parent_table, parent_cols,
    convalidated, condeferrable, condeferred, confmatchtype, on_update, on_delete
)
SELECT c.conname, cn.nspname, cr.relname,
       (SELECT array_agg(a.attname ORDER BY k.ord)
          FROM unnest(c.conkey) WITH ORDINALITY AS k(attnum, ord)
          JOIN pg_attribute a ON a.attrelid = c.conrelid AND a.attnum = k.attnum),
       pn.nspname, pr.relname,
       (SELECT array_agg(a.attname ORDER BY k.ord)
          FROM unnest(c.confkey) WITH ORDINALITY AS k(attnum, ord)
          JOIN pg_attribute a ON a.attrelid = c.confrelid AND a.attnum = k.attnum),
       c.convalidated, c.condeferrable, c.condeferred,
       c.confmatchtype, c.confupdtype, c.confdeltype
FROM pg_constraint c
JOIN pg_class cr ON cr.oid = c.conrelid
JOIN pg_namespace cn ON cn.oid = cr.relnamespace
JOIN pg_class pr ON pr.oid = c.confrelid
JOIN pg_namespace pn ON pn.oid = pr.relnamespace
WHERE c.contype = 'f'
  AND cn.nspname IN ('fps', 'mabarchive')
  AND c.conparentid = 0;

CREATE TABLE audit.orphan_report (
    fk_id         integer NOT NULL REFERENCES audit.fk_inventory (fk_id),
    conname       text NOT NULL,
    child_schema  text NOT NULL,
    child_table   text NOT NULL,
    child_key     text[] NOT NULL,
    parent_key    text[],
    row_count     bigint NOT NULL,
    match_count   integer,
    category      text NOT NULL
);

CREATE INDEX ON audit.orphan_report (category);
CREATE INDEX ON audit.orphan_report (child_schema, child_table);

DO $do$
DECLARE
    fk record;
    n integer;
    i integer;
    sep_c text;
    sep_a text;
    sel_child text;
    pred_notnull text;
    grp text;
    pred_exact text;
    pred_norm text;
    sel_parent text;
    arr_child text;
    arr_parent text;
    pred_ws text;
    pred_case text;
    pred_empty text;
    stmt text;
    n_failed integer := 0;
BEGIN
    FOR fk IN SELECT * FROM audit.fk_inventory ORDER BY fk_id LOOP
        n := array_length(fk.child_cols, 1);
        sel_child := ''; pred_notnull := ''; grp := ''; pred_exact := ''; pred_norm := '';
        sel_parent := ''; arr_child := ''; arr_parent := ''; pred_ws := ''; pred_case := '';
        pred_empty := '';

        FOR i IN 1..n LOOP
            sep_c := CASE WHEN i > 1 THEN ', ' ELSE '' END;
            sep_a := CASE WHEN i > 1 THEN ' AND ' ELSE '' END;
            sel_child := sel_child || format('%sc.%I AS k%s', sep_c, fk.child_cols[i], i);
            pred_notnull := pred_notnull || format('%sc.%I IS NOT NULL', sep_a, fk.child_cols[i]);
            grp := grp || format('%s%s', sep_c, i);
            pred_exact := pred_exact || format('%sp.%I = o.k%s', sep_a, fk.parent_cols[i], i);
            pred_norm := pred_norm || format('%slower(btrim(p.%I::text)) = lower(btrim(o.k%s::text))', sep_a, fk.parent_cols[i], i);
            sel_parent := sel_parent || format('%sp.%I::text AS p%s', sep_c, fk.parent_cols[i], i);
            arr_child := arr_child || format('%so.k%s::text', sep_c, i);
            arr_parent := arr_parent || format('%sm.p%s', sep_c, i);
            pred_ws := pred_ws || format('%sbtrim(o.k%s::text) = btrim(m.p%s)', sep_a, i, i);
            pred_case := pred_case || format('%slower(o.k%s::text) = lower(m.p%s)', sep_a, i, i);
            pred_empty := pred_empty || format('%sbtrim(o.k%s::text) = %L', CASE WHEN i > 1 THEN ' OR ' ELSE '' END, i, '');
        END LOOP;

        stmt := 'INSERT INTO audit.orphan_report '
             || '(fk_id, conname, child_schema, child_table, child_key, parent_key, row_count, match_count, category) '
             || 'WITH ck AS (SELECT ' || sel_child || ', count(*) AS row_count FROM '
             || format('%I.%I', fk.child_schema, fk.child_table) || ' c WHERE ' || pred_notnull || ' GROUP BY ' || grp || '), '
             || 'orph AS (SELECT * FROM ck o WHERE NOT EXISTS (SELECT 1 FROM '
             || format('%I.%I', fk.parent_schema, fk.parent_table) || ' p WHERE ' || pred_exact || ')) '
             || 'SELECT ' || fk.fk_id || ', ' || quote_literal(fk.conname) || ', '
             || quote_literal(fk.child_schema) || ', ' || quote_literal(fk.child_table) || ', ARRAY[' || arr_child || '], '
             || 'CASE WHEN m.matched IS NULL THEN NULL ELSE ARRAY[' || arr_parent || '] END, '
             || 'o.row_count, m.n, CASE WHEN ' || pred_empty || ' THEN ''EMPTY_KEY'' '
             || 'WHEN m.matched IS NULL THEN ''MISSING_MASTER'' WHEN m.n > 1 THEN ''AMBIGUOUS'' '
             || 'WHEN ' || pred_ws || ' THEN ''WS_ONLY'' WHEN ' || pred_case || ' THEN ''CASE_ONLY'' '
             || 'ELSE ''CASE_AND_WS'' END FROM orph o LEFT JOIN LATERAL (SELECT dp.*, count(*) OVER ()::int AS n, 1 AS matched '
             || 'FROM (SELECT DISTINCT ' || sel_parent || ' FROM ' || format('%I.%I', fk.parent_schema, fk.parent_table)
             || ' p WHERE ' || pred_norm || ') dp LIMIT 1) m ON true';

        BEGIN
            EXECUTE stmt;
        EXCEPTION WHEN others THEN
            n_failed := n_failed + 1;
            RAISE WARNING 'FK % on %.% could not be checked: %', fk.conname, fk.child_schema, fk.child_table, SQLERRM;
        END;
    END LOOP;

    IF n_failed > 0 THEN
        RAISE EXCEPTION 'FAIL - % constraint(s) could not be checked; validation is incomplete.', n_failed;
    END IF;
END
$do$;

CREATE OR REPLACE VIEW audit.v_fk_summary AS
SELECT i.conname,
       i.child_schema || '.' || i.child_table AS child,
       i.parent_schema || '.' || i.parent_table AS parent,
       coalesce(sum(r.row_count), 0) AS orphan_rows,
       count(r.*) AS orphan_keys,
       count(*) FILTER (WHERE r.category = 'MISSING_MASTER') AS missing_master,
       count(*) FILTER (WHERE r.category = 'AMBIGUOUS') AS ambiguous,
       count(*) FILTER (WHERE r.category = 'EMPTY_KEY') AS empty_key,
       count(*) FILTER (WHERE r.category IN ('CASE_ONLY', 'WS_ONLY', 'CASE_AND_WS')) AS normalisable
FROM audit.fk_inventory i
LEFT JOIN audit.orphan_report r ON r.fk_id = i.fk_id
GROUP BY i.fk_id, i.conname, i.child_schema, i.child_table, i.parent_schema, i.parent_table;

CREATE OR REPLACE VIEW audit.v_orphan_detail AS
SELECT r.category,
      r.conname,
      r.child_schema || '.' || r.child_table AS child,
      '[' || array_to_string(r.child_key, '][') || ']' AS child_key,
      CASE WHEN r.parent_key IS NULL THEN NULL
          ELSE '[' || array_to_string(r.parent_key, '][') || ']' END AS parent_key_candidate,
      r.match_count,
      r.row_count,
      i.parent_schema || '.' || i.parent_table AS parent
FROM audit.orphan_report r
JOIN audit.fk_inventory i ON i.fk_id = r.fk_id
ORDER BY r.row_count DESC, child, r.child_key;

DO $do$
DECLARE
    n_keys  bigint;
    n_rows  bigint;
    n_cons  bigint;
    detail  text;
BEGIN
    SELECT count(*), coalesce(sum(row_count), 0), count(DISTINCT conname)
      INTO n_keys, n_rows, n_cons
      FROM audit.orphan_report;

    IF n_keys = 0 THEN
        RAISE NOTICE 'PASS - no foreign key violations.';
        RETURN;
    END IF;

    SELECT string_agg(format('  %s: %s -> %s (%s rows, %s)', conname, child, parent, orphan_rows,
                             CASE WHEN missing_master > 0 THEN 'missing master rows'
                                  ELSE 'case/whitespace mismatch' END),
                      E'\n' ORDER BY orphan_rows DESC)
      INTO detail
      FROM audit.v_fk_summary
     WHERE orphan_rows > 0;

    RAISE EXCEPTION E'FAIL - % foreign key(s) violated: % offending key(s) across % row(s).\n%\nSee audit.v_orphan_detail for the full list.',
        n_cons, n_keys, n_rows, detail;
END
$do$;
