USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryTCC_Union    Script Date: 3/4/00 1:48:17 PM ******/
CREATE VIEW [dbo].[qryTCC_Union] AS 
SELECT Workgroup, ParentProject as Project FROM Workgroup, tlkpProject
WHERE Workgroup like 'SV__'
UNION
SELECT Workgroup, Project from TimeCostCalcs

GO
