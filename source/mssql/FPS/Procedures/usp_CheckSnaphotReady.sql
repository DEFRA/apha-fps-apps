USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[usp_CheckSnaphotReady] AS

	declare @R as varchar(5)

	Select @R=RTrim(DB_Var_Value)
	FROM tblDB_Variables
	WHERE DB_Var_Name = 'Snapshot_Ready'

            if @R<>'True'

            	BEGIN 
                        Raiserror ('The FPS is not ready to snapshot.',16,1)
                        Return 0
            	END
	ELSE
		Return 1

GO
