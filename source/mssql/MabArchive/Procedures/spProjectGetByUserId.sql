USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jackie Carter>
-- Create date: <13/11/08>
-- Description:	<Gets all projects for a specific User Name>
-- =============================================
CREATE PROCEDURE [dbo].[spProjectGetByUserId] 
	
	@userId varchar(10),
	@Year smallint 
AS
BEGIN
	SET NOCOUNT ON;

   SELECT	MY_tlkpProject.Year as projectYear,
			MY_tlkpProject.ParentProject,
			MY_tlkpProject.Manager,
			tblProjectManager.mNumber
	from	tblProjectManager inner join MY_tlkpProject on tblProjectManager.projectManager = MY_tlkpProject.Manager
	where	tblProjectManager.mNumber = @UserId
	and		MY_tlkpProject.Year = @Year
	Order by MY_tlkpProject.ParentProject
END

GO
