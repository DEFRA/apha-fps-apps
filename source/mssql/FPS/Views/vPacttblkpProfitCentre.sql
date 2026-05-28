USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vPacttblkpProfitCentre    Script Date: 3/4/00 1:48:16 PM ******/
/****** Object:  View dbo.vPacttblkpProfitCentre    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vPacttblkpProfitCentre] AS
SELECT  tblkpProfitCentre.ProfitCentre,
	 tblkpProfitCentre.ProfitCentreName, 
	tblkpProfitCentre.Division, 
	tblkpProfitCentre.CONTTARGET, 
	tblkpProfitCentre.ProfitCentreHead, 
	tblkpProfitCentre.DivisionID, 
	tblkpProfitCentre.Email_Recipient,
	tblkpProfitCentre.PACTCoordinatorEmailName,
	tblkpProfitCentre.Timesheet,
	tblkpProfitCentre.OutputSheet,
	tblkpProfitCentre.TimesheetLayout
FROM tblkpProfitCentre

GO
