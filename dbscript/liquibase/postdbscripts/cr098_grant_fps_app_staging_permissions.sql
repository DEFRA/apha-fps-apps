--liquibase formatted sql

--changeset Arihant:CR098 labels:ddl context:all

GRANT TRUNCATE ON
    fps.proj_subcontract_staging,
    fps.tblstagingmonthlyoutput,
    fps.tblstagingmonthlytime,
    fps.proj_invoice_staging
TO fps_app;

ALTER TABLE fps.proj_subcontract_staging
    OWNER TO fps_app;

ALTER TABLE fps.tblstagingmonthlyoutput
    OWNER TO fps_app;

ALTER TABLE fps.tblstagingmonthlytime
    OWNER TO fps_app;

ALTER TABLE fps.proj_invoice_staging
    OWNER TO fps_app;