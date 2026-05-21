USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[_vPact_FPS_TestReq]
AS
SELECT 1998 AS year, CASE WHEN P.TestCode IS NULL 
    THEN F.TestCode ELSE P.Testcode END AS TestCode, 
    CASE WHEN P.Buyer IS NULL 
    THEN F.JobCode ELSE P.Buyer END AS Buyer, 
    CASE WHEN P.unitPrice IS NULL 
    THEN F.testPrice WHEN P.unitPrice = 0 AND 
    F.TestPrice IS NOT NULL 
    THEN F.testprice ELSE P.unitPrice END AS UnitPrice, 
    CASE WHEN f.notests IS NULL 
    THEN p.norequired ELSE f.notests END AS norequired, 
    CASE WHEN f.jobcode IS NULL 
    THEN p.projectbuyercode ELSE f.jobcode END AS projectbuyercode,
     p.testBuyercode, CASE WHEN P.testcode IS NULL 
    THEN 'FPS' ELSE 'Pact' END AS source
FROM PACT.dbo.tlkpTestReqmt P FULL OUTER JOIN
    FPS.dbo.tblTestRequ F ON P.TestCode = F.TestCode AND 
    P.ProjectBuyerCode = F.JobCode

GO
