USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vContractProject]
AS
SELECT *
FROM dbo.tlkpProject
WHERE (Contract IN
        (SELECT vtblContract.contractno
      FROM vtblContract))
WITH CHECK OPTION

GO
