--liquibase formatted sql

--changeset repo-admin:001_add_partition labels:ddl context:all runAlways:true splitStatements:false

DO $$
DECLARE
    v_part_name  TEXT;
    r            RECORD;
    p_schema     TEXT := current_setting('custom.p_schema', true);
    p_value      TEXT := current_setting('custom.p_value', true);
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
        SELECT
            n.nspname AS schema_name,
            c.relname AS table_name,
            a.attname AS column_name
        FROM pg_partitioned_table pt
        JOIN pg_class c ON c.oid = pt.partrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        JOIN pg_attribute a 
            ON a.attrelid = c.oid 
           AND a.attnum = ANY(pt.partattrs)
        WHERE n.nspname = p_schema 
          AND pt.partstrat = 'l' -- List partitioning
          AND a.attname = 'fpsyear'
    LOOP
         -- Generate safe partition name
         v_part_name := r.table_name || '_y' || p_value;
             
         -- Skip if a partition already covers this value (regardless of its name)
         IF EXISTS (
             SELECT 1
             FROM pg_inherits i
             JOIN pg_class child  ON child.oid  = i.inhrelid
             JOIN pg_class parent ON parent.oid = i.inhparent
             JOIN pg_namespace n  ON n.oid = parent.relnamespace
             WHERE parent.relname = r.table_name
               AND n.nspname = r.schema_name
               AND pg_get_expr(child.relpartbound, child.oid) ~ ('(^|[^0-9])' || p_value || '([^0-9]|$)')
         ) THEN
             RAISE NOTICE 'A partition for %.% value % already exists, skipping.', r.schema_name, r.table_name, p_value;
         ELSE   
             -- Create partition
             EXECUTE format(
                 'CREATE TABLE %I.%I PARTITION OF %I.%I FOR VALUES IN (%L)',
                 r.schema_name,
                 v_part_name,
                 r.schema_name,
                 r.table_name,
                 p_value
             );
             RAISE NOTICE 'Partition %.% successfully created.', r.schema_name, v_part_name;
         END IF;
    END LOOP;
END $$;

--rollback empty ;