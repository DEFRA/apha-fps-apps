--liquibase formatted sql

--changeset repo-admin:cr038 labels:ddl context:all

-- standardize user/audit columns to varchar(255) for consistency across mabarchive tables.
alter table mabarchive.tblcomments
alter column madeby type varchar(255);

alter table mabarchive.tbllogmilestone
alter column changedby type varchar(255);

-- add missing login email column to project manager table.
alter table mabarchive.tblprojectmanager
add column if not exists loginemail varchar(255);

--rollback alter table mabarchive.tblcomments alter column madeby type varchar(50);
--rollback alter table mabarchive.tbllogmilestone alter column changedby type varchar(10);
--rollback alter table mabarchive.tblprojectmanager drop column if exists loginemail;

