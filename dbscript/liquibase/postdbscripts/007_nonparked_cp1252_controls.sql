--liquibase formatted sql

--changeset migration-fix:007-nonparked-cp1252-controls labels:dml context:all splitStatements:false runInTransaction:true
-- Repair the 758 Windows-1252 C1 control-character rows confirmed by the
-- non-parked source/target comparison. The changeset accepts only the fully
-- pending or already-complete state for each table and verifies the result.

SET LOCAL lock_timeout = '10s';
SET LOCAL statement_timeout = '10min';

CREATE FUNCTION pg_temp.has_cp1252_controls(value text)
RETURNS boolean
LANGUAGE sql
IMMUTABLE
RETURNS NULL ON NULL INPUT
AS $function$
    SELECT strpos(value, chr(128)) > 0
        OR strpos(value, chr(133)) > 0
        OR strpos(value, chr(145)) > 0
        OR strpos(value, chr(146)) > 0
        OR strpos(value, chr(147)) > 0
        OR strpos(value, chr(148)) > 0
        OR strpos(value, chr(149)) > 0
        OR strpos(value, chr(150)) > 0
        OR strpos(value, chr(156)) > 0;
$function$;

CREATE FUNCTION pg_temp.fix_cp1252_controls(value text)
RETURNS text
LANGUAGE sql
IMMUTABLE
RETURNS NULL ON NULL INPUT
AS $function$
    SELECT translate(
        value,
        chr(128) || chr(133) || chr(145) || chr(146) || chr(147)
            || chr(148) || chr(149) || chr(150) || chr(156),
        chr(8364) || chr(8230) || chr(8216) || chr(8217) || chr(8220)
            || chr(8221) || chr(8226) || chr(8211) || chr(339)
    );
$function$;

DO $repair$
DECLARE
    affected integer;
BEGIN
    SELECT COUNT(*) INTO affected
    FROM fps.project_log
    WHERE fpsyear = 2026
      AND pg_temp.has_cp1252_controls(projecttitle);
    IF affected NOT IN (0, 19) THEN
        RAISE EXCEPTION 'fps.project_log expected 19 pending rows or 0 complete rows, found %', affected;
    END IF;
    UPDATE fps.project_log
    SET projecttitle = pg_temp.fix_cp1252_controls(projecttitle)
    WHERE fpsyear = 2026
      AND pg_temp.has_cp1252_controls(projecttitle);
    GET DIAGNOSTICS affected = ROW_COUNT;
    RAISE NOTICE 'fps.project_log: repaired % rows', affected;

    SELECT COUNT(*) INTO affected
    FROM mabarchive.tbllogmilestone
    WHERE pg_temp.has_cp1252_controls(description)
       OR pg_temp.has_cp1252_controls(projectleadercomment)
       OR pg_temp.has_cp1252_controls(capscomment);
    IF affected NOT IN (0, 679) THEN
        RAISE EXCEPTION 'mabarchive.tbllogmilestone expected 679 pending rows or 0 complete rows, found %', affected;
    END IF;
    UPDATE mabarchive.tbllogmilestone
    SET description = pg_temp.fix_cp1252_controls(description),
        projectleadercomment = pg_temp.fix_cp1252_controls(projectleadercomment),
        capscomment = pg_temp.fix_cp1252_controls(capscomment)
    WHERE pg_temp.has_cp1252_controls(description)
       OR pg_temp.has_cp1252_controls(projectleadercomment)
       OR pg_temp.has_cp1252_controls(capscomment);
    GET DIAGNOSTICS affected = ROW_COUNT;
    RAISE NOTICE 'mabarchive.tbllogmilestone: repaired % rows', affected;

    SELECT COUNT(*) INTO affected
    FROM mabarchive.my_tbladditionalcosts
    WHERE pg_temp.has_cp1252_controls(description);
    IF affected NOT IN (0, 7) THEN
        RAISE EXCEPTION 'mabarchive.my_tbladditionalcosts expected 7 pending rows or 0 complete rows, found %', affected;
    END IF;
    UPDATE mabarchive.my_tbladditionalcosts
    SET description = pg_temp.fix_cp1252_controls(description)
    WHERE pg_temp.has_cp1252_controls(description);
    GET DIAGNOSTICS affected = ROW_COUNT;
    RAISE NOTICE 'mabarchive.my_tbladditionalcosts: repaired % rows', affected;

    SELECT COUNT(*) INTO affected
    FROM mabarchive.tbladditionalcosts
    WHERE pg_temp.has_cp1252_controls(description);
    IF affected NOT IN (0, 13) THEN
        RAISE EXCEPTION 'mabarchive.tbladditionalcosts expected 13 pending rows or 0 complete rows, found %', affected;
    END IF;
    UPDATE mabarchive.tbladditionalcosts
    SET description = pg_temp.fix_cp1252_controls(description)
    WHERE pg_temp.has_cp1252_controls(description);
    GET DIAGNOSTICS affected = ROW_COUNT;
    RAISE NOTICE 'mabarchive.tbladditionalcosts: repaired % rows', affected;

    SELECT COUNT(*) INTO affected
    FROM mabarchive.tblpublication
    WHERE pg_temp.has_cp1252_controls(subject)
       OR pg_temp.has_cp1252_controls(comments);
    IF affected NOT IN (0, 24) THEN
        RAISE EXCEPTION 'mabarchive.tblpublication expected 24 pending rows or 0 complete rows, found %', affected;
    END IF;
    UPDATE mabarchive.tblpublication
    SET subject = pg_temp.fix_cp1252_controls(subject),
        comments = pg_temp.fix_cp1252_controls(comments)
    WHERE pg_temp.has_cp1252_controls(subject)
       OR pg_temp.has_cp1252_controls(comments);
    GET DIAGNOSTICS affected = ROW_COUNT;
    RAISE NOTICE 'mabarchive.tblpublication: repaired % rows', affected;

    SELECT COUNT(*) INTO affected
    FROM mabarchive.tblcomments
    WHERE pg_temp.has_cp1252_controls(comment);
    IF affected NOT IN (0, 16) THEN
        RAISE EXCEPTION 'mabarchive.tblcomments expected 16 pending rows or 0 complete rows, found %', affected;
    END IF;
    UPDATE mabarchive.tblcomments
    SET comment = pg_temp.fix_cp1252_controls(comment)
    WHERE pg_temp.has_cp1252_controls(comment);
    GET DIAGNOSTICS affected = ROW_COUNT;
    RAISE NOTICE 'mabarchive.tblcomments: repaired % rows', affected;
END;
$repair$;

DO $verify$
BEGIN
    IF EXISTS (
        SELECT 1 FROM fps.project_log
        WHERE fpsyear = 2026
          AND pg_temp.has_cp1252_controls(projecttitle)
    ) OR EXISTS (
        SELECT 1 FROM mabarchive.tbllogmilestone
        WHERE pg_temp.has_cp1252_controls(description)
           OR pg_temp.has_cp1252_controls(projectleadercomment)
           OR pg_temp.has_cp1252_controls(capscomment)
    ) OR EXISTS (
        SELECT 1 FROM mabarchive.my_tbladditionalcosts
        WHERE pg_temp.has_cp1252_controls(description)
    ) OR EXISTS (
        SELECT 1 FROM mabarchive.tbladditionalcosts
        WHERE pg_temp.has_cp1252_controls(description)
    ) OR EXISTS (
        SELECT 1 FROM mabarchive.tblpublication
        WHERE pg_temp.has_cp1252_controls(subject)
           OR pg_temp.has_cp1252_controls(comments)
    ) OR EXISTS (
        SELECT 1 FROM mabarchive.tblcomments
        WHERE pg_temp.has_cp1252_controls(comment)
    ) THEN
        RAISE EXCEPTION 'CP1252 control-character verification failed';
    END IF;
END;
$verify$;