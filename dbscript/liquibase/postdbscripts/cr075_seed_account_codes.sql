--liquibase formatted sql

--changeset vishal:CR075 labels:dml context:all

INSERT INTO fps.tlkpaccountcode (code, description)
VALUES ('1', 'Not Specified')
ON CONFLICT (code) DO NOTHING;

INSERT INTO fps.tlkpsubaccount (subaccountcode, subaccount)
VALUES ('1', 'Not Specified')
ON CONFLICT (subaccountcode) DO NOTHING;

--ROLLBACK
--DELETE FROM fps.tlkpaccountcode
--WHERE code = '1' AND description = 'Not Specified';
--DELETE FROM fps.tlkpsubaccount
--WHERE subaccountcode = '1' AND subaccount = 'Not Specified';
