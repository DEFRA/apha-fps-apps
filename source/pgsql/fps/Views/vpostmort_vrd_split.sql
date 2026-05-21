-- View: fps.vpostmort_vrd_split
CREATE OR REPLACE VIEW "fps"."vpostmort_vrd_split" AS
SELECT "right"((monthlyoutput.workgroup)::text, 2) AS location,
