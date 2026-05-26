USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jackie Carter>
-- Create date: <17/11/08>
-- Description:	<Gets all projects >
-- =============================================
CREATE PROCEDURE [dbo].[spProjectGetAll]

 @Year smallint 

AS
BEGIN
	SET NOCOUNT ON;

   SELECT  MY_tlkpProject.Year as projectYear,
           MY_tlkpProject.ParentProject,
           MY_tlkpProject.Manager,
           tblProjectManager.mNumber
	from  tblProjectManager inner join MY_tlkpProject on tblProjectManager.projectManager = MY_tlkpProject.Manager
	Where MY_tlkpProject.Year = @Year
	Order by MY_tlkpProject.ParentProject
END

GO
