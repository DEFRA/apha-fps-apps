--liquibase formatted sql

--changeset arihant:CR087 labels:ddl context:all splitStatements:false

BEGIN;

-- ============================================================================
-- CR087
--
-- Purpose:
--   Restore automatic ID generation for:
--
--       fps.rate_change_history.ratechangehistoryid
--
-- Background:
--   - ratechangehistoryid was originally defined as bigserial.
--   - CR071 partitioned fps.rate_change_history and expected the existing
--     sequence/default to be preserved.
--   - The current partitioned production table has no DEFAULT/sequence for
--     ratechangehistoryid.
--   - Bulk Rates history inserts do not explicitly supply
--     ratechangehistoryid, so inserts currently fail.
--
-- Correction:
--   1. Ensure the expected sequence exists.
--   2. Seed it from the current maximum ratechangehistoryid.
--   3. Set the sequence as the DEFAULT on the partitioned parent table.
--   4. Set sequence ownership to the production column.
--   5. Validate the corrected schema.
-- ============================================================================


-- ============================================================================
-- 0. Preconditions
-- ============================================================================

DO $$
BEGIN

    IF to_regclass('fps.rate_change_history') IS NULL THEN
        RAISE EXCEPTION
            'CR087 precondition failed: fps.rate_change_history does not exist.';
    END IF;


    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'fps'
          AND table_name = 'rate_change_history'
          AND column_name = 'ratechangehistoryid'
    ) THEN
        RAISE EXCEPTION
            'CR087 precondition failed: fps.rate_change_history.ratechangehistoryid does not exist.';
    END IF;


    -- CR071 should already have converted the table to LIST partitioning.
    IF NOT EXISTS (
        SELECT 1
        FROM pg_partitioned_table
        WHERE partrelid = 'fps.rate_change_history'::regclass
    ) THEN
        RAISE EXCEPTION
            'CR087 precondition failed: fps.rate_change_history is not partitioned.';
    END IF;

END
$$;


-- Prevent concurrent history inserts while the sequence is created/reseeded
-- and the DEFAULT is restored.
LOCK TABLE fps.rate_change_history
    IN ACCESS EXCLUSIVE MODE;


-- ============================================================================
-- 1. Restore sequence
-- ============================================================================

CREATE SEQUENCE IF NOT EXISTS
    fps.rate_change_history_ratechangehistoryid_seq
    AS bigint;


-- ============================================================================
-- 2. Seed sequence from existing data
--
-- If the current maximum ID is:
--
--     36 -> next generated value will be 37
--
-- If the table is empty:
--
--     next generated value will be 1
-- ============================================================================

DO $$
DECLARE
    v_max_id bigint;
BEGIN

    SELECT COALESCE(MAX(ratechangehistoryid), 0)
    INTO v_max_id
    FROM fps.rate_change_history;


    PERFORM setval(
        'fps.rate_change_history_ratechangehistoryid_seq'::regclass,
        GREATEST(v_max_id, 1),
        v_max_id > 0
    );

END
$$;


-- ============================================================================
-- 3. Restore DEFAULT on partitioned parent
-- ============================================================================

ALTER TABLE fps.rate_change_history
    ALTER COLUMN ratechangehistoryid
    SET DEFAULT nextval(
        'fps.rate_change_history_ratechangehistoryid_seq'::regclass
    );


-- ============================================================================
-- 4. Restore sequence ownership
-- ============================================================================

ALTER SEQUENCE
    fps.rate_change_history_ratechangehistoryid_seq
    OWNED BY fps.rate_change_history.ratechangehistoryid;


-- ============================================================================
-- 5. Postconditions
-- ============================================================================

DO $$
DECLARE
    v_default_expression text;
    v_max_id bigint;
    v_sequence_last_value bigint;
BEGIN

    -- Sequence must exist.
    IF to_regclass(
        'fps.rate_change_history_ratechangehistoryid_seq'
    ) IS NULL THEN

        RAISE EXCEPTION
            'CR087 postcondition failed: rate_change_history sequence does not exist.';

    END IF;


    -- Production column must now have a DEFAULT.
    SELECT pg_get_expr(
               d.adbin,
               d.adrelid
           )
    INTO v_default_expression
    FROM pg_attribute a
    JOIN pg_attrdef d
      ON d.adrelid = a.attrelid
     AND d.adnum = a.attnum
    WHERE a.attrelid = 'fps.rate_change_history'::regclass
      AND a.attname = 'ratechangehistoryid'
      AND NOT a.attisdropped;


    IF v_default_expression IS NULL THEN

        RAISE EXCEPTION
            'CR087 postcondition failed: ratechangehistoryid DEFAULT is still missing.';

    END IF;


    IF v_default_expression NOT LIKE
       '%rate_change_history_ratechangehistoryid_seq%' THEN

        RAISE EXCEPTION
            'CR087 postcondition failed: ratechangehistoryid DEFAULT does not reference the expected sequence. Found: %',
            v_default_expression;

    END IF;


    -- Sequence must not be behind existing data.
    SELECT COALESCE(MAX(ratechangehistoryid), 0)
    INTO v_max_id
    FROM fps.rate_change_history;


    SELECT last_value
    INTO v_sequence_last_value
    FROM fps.rate_change_history_ratechangehistoryid_seq;


    IF v_max_id > 0
       AND v_sequence_last_value < v_max_id THEN

        RAISE EXCEPTION
            'CR087 postcondition failed: sequence value % is behind current maximum ratechangehistoryid %.',
            v_sequence_last_value,
            v_max_id;

    END IF;

END
$$;


COMMIT;
