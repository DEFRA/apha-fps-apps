USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vProjectMonthFinal    Script Date: 3/4/00 1:48:17 PM ******/
CREATE VIEW [dbo].[vProjectMonthFinal] AS
SELECT	*
FROM	ProjectMonthFinal 
WHERE ProjectMonthFinal.Project IN (SELECT ParentProject FROM vtlkpProject)

WITH CHECK OPTION

GO
