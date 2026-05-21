USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vContractAnimalReq    Script Date: 3/4/00 1:48:17 PM ******/
/****** Object:  View dbo.vContractAnimalReq    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vContractAnimalReq] AS
SELECT	*
FROM	tblAnimalReq
WHERE	tblAnimalReq.JobCode IN (SELECT vContractProject.ParentProject FROM vContractProject)
	
WITH CHECK OPTION

GO
