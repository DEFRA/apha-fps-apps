USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vLatestProjectYear]
AS
SELECT ParentProject,  MAX(Year) 
    AS Year
FROM dbo.MY_tlkpProject
GROUP BY ParentProject


GO
