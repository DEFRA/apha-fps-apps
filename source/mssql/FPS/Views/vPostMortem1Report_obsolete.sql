USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/* SELECT     NULL AS TestCode, NULL AS ItemDescription, SUM(TotVol) AS TotVol, NULL AS LTUnitCharge, NULL AS SDUnitCHarge, CAST(ROUND(SUM(LTFee), 0) 
                      AS int) AS [LTfee], CAST(ROUND(SUM(SDFee), 0) AS int) AS [SDfee], CAST(ROUND(SUM(LTFee), 0) AS int) + CAST(ROUND(SUM(SDFee), 0) AS int) 
                      AS [Total Fee], CAST(ROUND(SUM(FeeCharged), 0) AS int) AS [Fee Charged], CAST(ROUND(SUM(FeeCharged) - SUM(LTFee) - SUM(SDFee), 0) AS int) 
                      AS [Profit/Loss]
 FROM         vPostMort1
 UNION*/
CREATE VIEW [dbo].[vPostMortem1Report_obsolete]
AS
SELECT     TestCode, ItemDescription, TotVol, LTUnitCharge, SDUnitCharge, CAST(ROUND(LTfee, 0) AS int) AS LTfee, CAST(ROUND(SDfee, 0) AS int) AS SDfee, 
                      CAST(ROUND(LTfee, 0) AS int) + CAST(ROUND(SDfee, 0) AS int) AS [Total Fee], CAST(ROUND(FeeCharged, 0) AS int) AS [Fee Charged], 
                      CAST(ROUND(FeeCharged - LTfee - SDfee, 0) AS int) AS [Profit/Loss], WorkGroup
FROM         dbo.vPostMort1

GO
