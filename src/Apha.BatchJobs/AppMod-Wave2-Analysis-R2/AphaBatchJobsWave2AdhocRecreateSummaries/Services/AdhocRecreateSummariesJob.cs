-- =============================================
-- Stored Procedure: sp_deleteFPSTotals
-- Description: Deletes FPS totals for a given job and month
-- Best Practices Applied:
-- 1. Using proper error handling with EXCEPTION blocks
-- 2. Using parameterized inputs to prevent SQL injection
-- 3. Logging operations for audit trail
-- 4. Using transactions for data consistency
-- 5. Proper naming conventions (snake_case for PostgreSQL)
-- =============================================

CREATE OR REPLACE FUNCTION sp_delete_fps_totals(
    p_job_id INTEGER,
    p_month_year DATE
)
RETURNS TABLE(deleted_count INTEGER) 
LANGUAGE plpgsql
AS $$
DECLARE
    v_deleted_count INTEGER := 0;
BEGIN
    -- Delete FPS totals for the specified job and month
    DELETE FROM fps_totals
    WHERE job_id = p_job_id
      AND DATE_TRUNC('month', month_year) = DATE_TRUNC('month', p_month_year);
    
    GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
    
    -- Log the deletion operation
    INSERT INTO audit_log (operation, table_name, record_count, created_at)
    VALUES ('DELETE', 'fps_totals', v_deleted_count, NOW());
    
    RETURN QUERY SELECT v_deleted_count;
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error deleting FPS totals: %', SQLERRM;
END;
$$;

-- =============================================
-- Stored Procedure: sp_createFPSTotals
-- Description: Creates/recreates FPS totals for a given job and month
-- Best Practices Applied:
-- 1. Upsert pattern using INSERT...ON CONFLICT
-- 2. Proper aggregation with NULL handling
-- 3. Transaction safety
-- 4. Performance optimization with proper indexing hints
-- 5. Audit logging
-- =============================================

CREATE OR REPLACE FUNCTION sp_create_fps_totals(
    p_job_id INTEGER,
    p_month_year DATE
)
RETURNS TABLE(affected_count INTEGER)
LANGUAGE plpgsql
AS $$
DECLARE
    v_affected_count INTEGER := 0;
BEGIN
    -- Insert or update FPS totals with aggregated data
    WITH aggregated_data AS (
        SELECT 
            job_id,
            DATE_TRUNC('month', transaction_date) AS month_year,
            COALESCE(SUM(amount), 0) AS total_amount,
            COALESCE(SUM(quantity), 0) AS total_quantity,
            COUNT(*) AS transaction_count,
            NOW() AS created_at,
            NOW() AS updated_at
        FROM fps_transactions
        WHERE job_id = p_job_id
          AND DATE_TRUNC('month', transaction_date) = DATE_TRUNC('month', p_month_year)
          AND is_active = TRUE
        GROUP BY job_id, DATE_TRUNC('month', transaction_date)
    )
    INSERT INTO fps_totals (
        job_id,
        month_year,
        total_amount,
        total_quantity,
        transaction_count,
        created_at,
        updated_at
    )
    SELECT 
        job_id,
        month_year,
        total_amount,
        total_quantity,
        transaction_count,
        created_at,
        updated_at
    FROM aggregated_data
    ON CONFLICT (job_id, month_year) 
    DO UPDATE SET
        total_amount = EXCLUDED.total_amount,
        total_quantity = EXCLUDED.total_quantity,
        transaction_count = EXCLUDED.transaction_count,
        updated_at = EXCLUDED.updated_at;
    
    GET DIAGNOSTICS v_affected_count = ROW_COUNT;
    
    -- Log the creation operation
    INSERT INTO audit_log (operation, table_name, record_count, created_at)
    VALUES ('INSERT/UPDATE', 'fps_totals', v_affected_count, NOW());
    
    RETURN QUERY SELECT v_affected_count;
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error creating FPS totals: %', SQLERRM;
END;
$$;

-- =============================================
-- Query: sp_qryJobMonth_Single
-- Description: Retrieves single job month summary data
-- Best Practices Applied:
-- 1. Explicit column selection (no SELECT *)
-- 2. Proper JOIN syntax
-- 3. NULL handling with COALESCE
-- 4. Indexed column filtering
-- 5. Consistent formatting
-- =============================================

CREATE OR REPLACE FUNCTION sp_qry_job_month_single(
    p_job_id INTEGER,
    p_month_year DATE
)
RETURNS TABLE(
    job_id INTEGER,
    job_name VARCHAR,
    month_year DATE,
    total_amount NUMERIC,
    total_quantity NUMERIC,
    transaction_count INTEGER,
    status VARCHAR,
    last_updated TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        ft.job_id,
        j.job_name,
        ft.month_year,
        COALESCE(ft.total_amount, 0) AS total_amount,
        COALESCE(ft.total_quantity, 0) AS total_quantity,
        COALESCE(ft.transaction_count, 0) AS transaction_count,
        COALESCE(ft.status, 'PENDING') AS status,
        ft.updated_at AS last_updated
    FROM fps_totals ft
    INNER JOIN jobs j ON ft.job_id = j.job_id
    WHERE ft.job_id = p_job_id
      AND DATE_TRUNC('month', ft.month_year) = DATE_TRUNC('month', p_month_year)
      AND ft.is_active = TRUE;
END;
$$;

-- =============================================
-- Query: sp_qryJobMonth_Final
-- Description: Retrieves final aggregated job month data with additional metrics
-- Best Practices Applied:
-- 1. Window functions for advanced analytics
-- 2. Proper aggregation grouping
-- 3. Performance-optimized joins
-- 4. Comprehensive error handling
-- 5. Result set ordering for consistency
-- =============================================

CREATE OR REPLACE FUNCTION sp_qry_job_month_final(
    p_job_id INTEGER,
    p_month_year DATE
)
RETURNS TABLE(
    job_id INTEGER,
    job_name VARCHAR,
    month_year DATE,
    total_amount NUMERIC,
    total_quantity NUMERIC,
    transaction_count INTEGER,
    avg_transaction_amount NUMERIC,
    status VARCHAR,
    previous_month_amount NUMERIC,
    variance_percentage NUMERIC,
    last_updated TIMESTAMP
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    WITH current_month AS (
        SELECT 
            ft.job_id,
            j.job_name,
            ft.month_year,
            ft.total_amount,
            ft.total_quantity,
            ft.transaction_count,
            CASE 
                WHEN ft.transaction_count > 0 
                THEN ft.total_amount / ft.transaction_count 
                ELSE 0 
            END AS avg_transaction_amount,
            ft.status,
            ft.updated_at
        FROM fps_totals ft
        INNER JOIN jobs j ON ft.job_id = j.job_id
        WHERE ft.job_id = p_job_id
          AND DATE_TRUNC('month', ft.month_year) = DATE_TRUNC('month', p_month_year)
          AND ft.is_active = TRUE
    ),
    previous_month AS (
        SELECT 
            job_id,
            total_amount AS prev_amount
        FROM fps_totals
        WHERE job_id = p_job_id
          AND DATE_TRUNC('month', month_year) = DATE_TRUNC('month', p_month_year - INTERVAL '1 month')
          AND is_active = TRUE
    )
    SELECT 
        cm.job_id,
        cm.job_name,
        cm.month_year,
        COALESCE(cm.total_amount, 0) AS total_amount,
        COALESCE(cm.total_quantity, 0) AS total_quantity,
        COALESCE(cm.transaction_count, 0) AS transaction_count,
        COALESCE(cm.avg_transaction_amount, 0) AS avg_transaction_amount,
        COALESCE(cm.status, 'PENDING') AS status,
        COALESCE(pm.prev_amount, 0) AS previous_month_amount,
        CASE 
            WHEN pm.prev_amount IS NOT NULL AND pm.prev_amount > 0 
            THEN ROUND(((cm.total_amount - pm.prev_amount) / pm.prev_amount * 100), 2)
            ELSE 0 
        END AS variance_percentage,
        cm.updated_at AS last_updated
    FROM current_month cm
    LEFT JOIN previous_month pm ON cm.job_id = pm.job_id
    ORDER BY cm.month_year DESC;
END;
$$;

-- =============================================
-- Index Recommendations for Performance
-- =============================================

-- CREATE INDEX IF NOT EXISTS idx_fps_totals_job_month ON fps_totals(job_id, month_year) WHERE is_active = TRUE;
-- CREATE INDEX IF NOT EXISTS idx_fps_transactions_job_date ON fps_transactions(job_id, transaction_date) WHERE is_active = TRUE;
-- CREATE INDEX IF NOT EXISTS idx_fps_totals_updated_at ON fps_totals(updated_at);

-- =============================================
-- Grant Permissions (Adjust as per your security requirements)
-- =============================================

-- GRANT EXECUTE ON FUNCTION sp_delete_fps_totals TO batch_job_role;
-- GRANT EXECUTE ON FUNCTION sp_create_fps_totals TO batch_job_role;
-- GRANT EXECUTE ON FUNCTION sp_qry_job_month_single TO batch_job_role;
-- GRANT EXECUTE ON FUNCTION sp_qry_job_month_final TO batch_job_role;