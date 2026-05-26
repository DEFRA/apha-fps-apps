USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[usp_Delete_Project] 
	@OldCode Varchar(20)
AS
declare @errstr Varchar(200)
set @errstr =' '
IF (SELECT Count(*) FROM MonthlyOutput where Buyer =@Oldcode) != 0 
	 set @errstr=@errstr + 'Monthly Tests, '
IF (SELECT Count(*) FROM MonthlyTime where parentproject=@Oldcode) != 0 
	set @errstr=@errstr + 'Monthly Time, '
IF (SELECT Count(*) FROM Proj_Invoice where projectparent=@Oldcode) != 0 
	set @errstr=@errstr + 'Invoice, '	
IF (SELECT Count(*) FROM Proj_Subcontract where project=@Oldcode) != 0 
	set @errstr=@errstr +'Subcontracts, '
If @errstr<>' ' 
	begin
		set @errstr='This project cannot be delted, there are records in ' + @errstr
		RAISERROR(@errstr, 16, 1)
	end
else

	BEGIN TRANSACTION
	EXECUTE sp_Delete_tcv @oldcode
	EXECUTE sp_Delete_JC @oldcode

	EXECUTE sp_delete_tr @oldcode
	EXECUTE sp_Delete_ar @oldcode
	EXECUTE sp_Delete_sj @oldcode
	EXECUTE sp_Delete_ac @oldcode

	EXECUTE sp_Delete_pp @oldcode
	COMMIT TRANSACTION

GO
