USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		IG
-- Create date: 18 08 2011
-- Description:	Run this to send report emails for
-- BO 
-- =============================================
CREATE PROCEDURE [dbo].[spSendReportEmails_Manual]


AS
BEGIN

	SET NOCOUNT ON;
	Update [tbl Settings]
	SET Setting=-1
	WHERE ID='SendEmail'
	exec spSendProgramManagerReportEmail
	exec spSendProjectManagerReportEmail
	exec spSendRCManagerReportEmail
	exec spResetSendEmail
END

GO
