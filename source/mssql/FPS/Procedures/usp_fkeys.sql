USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.usp_fkeys    Script Date: 3/4/00 1:48:20 PM ******/
/****** Object:  Stored Procedure dbo.usp_fkeys    Script Date: 1/12/99 12:14:26 PM ******/
CREATE procEDURE [dbo].[usp_fkeys] 
as
	DECLARE @pktable_id			int
	DECLARE @pkfull_table_name	char(70)
	DECLARE @fktable_id			int
	DECLARE @fkfull_table_name	char(70)
	declare	@order_by_pk		int
	DECLARE @TableName varchar(50)
	DECLARE cur_Tables INSENSITIVE CURSOR FOR
	SELECT	o.name
	FROM	sysobjects o
	WHERE	o.type = 'U'
	ORDER BY o.Name
	create table #fkeys(
			 pkdb_id		int NOT NULL,
			 pktable_id 	int NOT NULL,
			 pkcolid		int NOT NULL,
			 fkdb_id		int NOT NULL,
			 fktable_id		int NOT NULL,
			 fkcolid		int NOT NULL,
			 KEY_SEQ		smallint NOT NULL,
			 fk_id			int NOT NULL,
			 pk_id			int NOT NULL)
	/*	SQL Server supports upto 16 PK/FK relationships between 2 tables */
	/*	Process syskeys for each relationship */
	/*	The inserts below adds a row to the temp table for each of the
		16 possible relationships */
OPEN cur_Tables
FETCH NEXT FROM cur_Tables INTO @TableName
SELECT @pktable_id = object_id(@TableName)
WHILE (@@FETCH_STATUS <> -1)
BEGIN
	
    insert into #fkeys
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey1,
			r.fkeydbid,
			r.fkeyid,
			r.fkey1,
			1,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey2,
			r.fkeydbid,
			r.fkeyid,
			r.fkey2,
			2,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey3,
			r.fkeydbid,
			r.fkeyid,
			r.fkey3,
			3,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey4,
			r.fkeydbid,
			r.fkeyid,
			r.fkey4,
			4,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey5,
			r.fkeydbid,
			r.fkeyid,
			r.fkey5,
			5,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey6,
			r.fkeydbid,
			r.fkeyid,
			r.fkey6,
			6,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey7,
			r.fkeydbid,
			r.fkeyid,
			r.fkey7,
			7,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey8,
			r.fkeydbid,
			r.fkeyid,
			r.fkey8,
			8,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey9,
			r.fkeydbid,
			r.fkeyid,
			r.fkey9,
			9,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey10,
			r.fkeydbid,
			r.fkeyid,
			r.fkey10,
			10,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey11,
			r.fkeydbid,
			r.fkeyid,
			r.fkey11,
			11,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey12,
			r.fkeydbid,
			r.fkeyid,
			r.fkey12,
			12,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey13,
			r.fkeydbid,
			r.fkeyid,
			r.fkey13,
			13,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey14,
			r.fkeydbid,
			r.fkeyid,
			r.fkey14,
			14,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey15,
			r.fkeydbid,
			r.fkeyid,
			r.fkey15,
			15,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	  union all
		select
			r.rkeydbid,
			r.rkeyid,
			r.rkey16,
			r.fkeydbid,
			r.fkeyid,
			r.fkey16,
			16,
			r.constid,
			s.constid
		from
			sysreferences r, sysconstraints s
		where	r.rkeyid = s.id
			AND (s.status & 0xf) = 1
			AND r.rkeyid between isnull(@pktable_id, 0) and isnull(@pktable_id, 0x7fffffff)
			AND r.fkeyid between isnull(@fktable_id, 0) and isnull(@fktable_id, 0x7fffffff)
	FETCH NEXT FROM cur_Tables INTO @TableName
	SELECT @pktable_id = object_id(@TableName)
	END
CLOSE cur_Tables
DEALLOCATE cur_Tables
		select
			/*PKTABLE_QUALIFIER = convert(varchar(32),DB_NAME(f.pkdb_id)),
			PKTABLE_OWNER = convert(varchar(32),USER_NAME(o1.uid)),*/
			PKTABLE_NAME = convert(varchar(32),o1.name),
			PKCOLUMN_NAME = convert(varchar(32),c1.name),
			/*FKTABLE_QUALIFIER = convert(varchar(32),DB_NAME(f.fkdb_id)),
			FKTABLE_OWNER = convert(varchar(32),USER_NAME(o2.uid)),*/
			FKTABLE_NAME = convert(varchar(32),o2.name),
			FKCOLUMN_NAME = convert(varchar(32),c2.name),
			/*KEY_SEQ,
			UPDATE_RULE = convert(smallint,1),
			DELETE_RULE = convert(smallint,1),*/
			FK_NAME = convert(varchar(128),OBJECT_NAME(fk_id)),
			PK_NAME = convert(varchar(128),OBJECT_NAME(pk_id))
		from #fkeys f,
			sysobjects o1, sysobjects o2,
			syscolumns c1, syscolumns c2
		where	o1.id = f.pktable_id
			AND o2.id = f.fktable_id
			AND c1.id = f.pktable_id
			AND c2.id = f.fktable_id
			AND c1.colid = f.pkcolid
			AND c2.colid = f.fkcolid
		order by 1,2,3,4

GO
