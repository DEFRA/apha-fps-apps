-- CR024: Resync fps.period_monthlyoutput identity sequence
-- Purpose: prevent duplicate primary key violations when sequence lags table max(id)

BEGIN;

SELECT setval(
  'fps.period_monthlyoutput_id_seq',
  COALESCE((SELECT MAX(id) FROM fps.period_monthlyoutput), 0)
);

COMMIT;
