USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vContractAdditionalCosts    Script Date: 3/4/00 1:48:17 PM ******/
/****** Object:  View dbo.vContractAdditionalCosts    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vContractAdditionalCosts] AS
SELECT	Jobcode, Account, Description, ItemCost
FROM	tblAdditionalCosts
WHERE	tblAdditionalCosts.JobCode IN (SELECT vContractProject.ParentProject FROM vContractProject)

GO
