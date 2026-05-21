USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[up_Update_tblPostMortem1Report] AS

insert into tblPostMortem1Report(TestCode, ItemDescription, TotVol, LTUnitCharge, SDUnitCharge,  LTFee, SDFee,[Total Fee],[Fee Charged],[Profit/Loss])  
SELECT TestCode, ItemDescription, TotVol, LTUnitCharge, SDUnitCharge, CAST(ROUND(LTfee, 0) AS int) AS LTfee, CAST(ROUND(SDfee, 0) AS int) AS SDfee,  CAST(ROUND(LTfee, 0) AS int) + CAST(ROUND(SDfee, 0) AS int) AS [Total Fee], CAST(ROUND(FeeCharged, 0) AS int) AS [Fee Charged],  CAST(ROUND(FeeCharged - LTfee - SDfee, 0) AS int) AS [Profit/Loss] FROM vPostMort1

GO
