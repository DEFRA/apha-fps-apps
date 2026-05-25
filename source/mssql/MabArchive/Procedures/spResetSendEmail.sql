USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		IG
-- Create date: 12/05/09
-- Description:	Resets the Send Email flag to stop project and program managers
--				getting the monthly report emails.
-- =============================================
CREATE PROCEDURE [dbo].[spResetSendEmail] 
AS
BEGIN
	SET NOCOUNT ON;
	Update [tbl Settings]
	SET Setting=0
	WHERE ID='SendEmail'
END

GO
