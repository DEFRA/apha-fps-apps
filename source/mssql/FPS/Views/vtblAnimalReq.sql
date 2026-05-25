USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vtblAnimalReq    Script Date: 3/4/00 1:48:17 PM ******/
/****** Object:  View dbo.vtblAnimalReq    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vtblAnimalReq] AS
SELECT	*
FROM	tblAnimalReq
WHERE	tblAnimalReq.JobCode IN (SELECT vtlkpProject.ParentProject FROM vtlkpProject)
WITH CHECK OPTION

GO
