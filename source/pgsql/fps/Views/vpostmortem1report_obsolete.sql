CREATE OR REPLACE VIEW fps.vpostmortem1report_obsolete AS
 SELECT testcode,
    itemdescription,
    totvol,
    ltunitcharge,
    sdunitcharge,
    round(ltfee::numeric)::integer AS ltfee,
    round(sdfee::numeric)::integer AS sdfee,
    round(ltfee::numeric)::integer + round(sdfee::numeric)::integer AS total_fee,
    round(feecharged::numeric)::integer AS fee_charged,
    round((feecharged - ltfee - sdfee)::numeric)::integer AS profit_loss,
    workgroup
   FROM fps.vpostmort1;
