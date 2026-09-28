--liquibase formatted sql

--changeset repo-admin:CR049 labels:ddl context:all splitStatements:false

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'fps'
          AND table_name = 'job_cancellation_request'
    ) THEN
        IF EXISTS (
            SELECT 1
            FROM information_schema.referential_constraints rc
            JOIN information_schema.table_constraints tc
                ON tc.constraint_name = rc.unique_constraint_name
            WHERE tc.table_schema = 'fps'
              AND tc.table_name = 'job_cancellation_request'
        ) THEN
            RAISE EXCEPTION
                'fps.job_cancellation_request has dependent FK constraints. Resolve before applying CR049.';
        END IF;
    END IF;
END $$;

DROP TABLE IF EXISTS fps.job_cancellation_request;

--rollback --Not Applicable
