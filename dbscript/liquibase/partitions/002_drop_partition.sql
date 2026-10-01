--liquibase formatted sql

--changeset repo-admin:002_drop_partition labels:ddl context:all runAlways:true splitStatements:false

DO $$
DECLARE
    r           RECORD;
    p_schema    TEXT := current_setting('custom.p_schema', true);
    p_value     TEXT := current_setting('custom.p_value', true);
    v_count     INTEGER := 0;
BEGIN
    -- Validate that inputs were passed correctly
    IF p_schema IS NULL OR p_value IS NULL THEN
        RAISE EXCEPTION 'Input parameters custom.p_schema or custom.p_value are missing!';
    END IF;

    -- Ensure p_value is a valid 4-digit year
    IF btrim(p_value) !~ '^[0-9]{4}$' THEN
        RAISE EXCEPTION 'Input parameter custom.p_value must be a 4-digit year (YYYY). Got: %', p_value;
    END IF;

    p_value := btrim(p_value);

    FOR r IN
        -- Match partitions by their bound VALUE, not by name
        SELECT c.relname AS part_name
        FROM pg_inherits i
        JOIN pg_class c ON i.inhrelid = c.oid
        JOIN pg_class p ON i.inhparent = p.oid
        JOIN pg_namespace nm ON nm.oid = c.relnamespace
        WHERE nm.nspname = p_schema
          AND pg_get_expr(c.relpartbound, c.oid) ~ ('(^|[^0-9])' || p_value || '([^0-9]|$)')
    LOOP
        RAISE NOTICE 'Dropping partition table: %.%', p_schema, r.part_name;
        
        -- Safely drop partition table with CASCADE to handle dependent constraints
        EXECUTE format(
            'DROP TABLE %I.%I CASCADE',
            p_schema,
            r.part_name
        );
        
        v_count := v_count + 1;
    END LOOP;
    
    -- Log the completion result
    RAISE NOTICE 'Successfully dropped % partition table(s)', v_count;
    
    -- If no partitions were found, log a warning
    IF v_count = 0 THEN
        RAISE WARNING 'No partition tables found matching pattern %%_y%% for year %', p_value;
    END IF;
END $$;

--rollback empty ;