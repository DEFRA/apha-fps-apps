USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[upUpdatePM1Report]

/*
Ilkka Salminen, October 2005

empties tblPostMortem1Report

called by upPM1Report

for FPS PostMortem reporting

*/


AS

insert into tblPostMortem1Report(TestCode, ItemDescription, TotVol, LTUnitCharge, SDUnitCharge,  
LTFee, SDFee,[Total Fee],[Fee Charged],[Profit/Loss], WorkGroup)  

SELECT TestCode, ItemDescription, TotVol, LTUnitCharge, SDUnitCharge, 
LTfee, SDfee,  LTfee + SDfee  AS [Total Fee], FeeCharged AS [Fee Charged], FeeCharged - LTfee - SDfee AS [Profit/Loss], WorkGroup FROM vPostMort1

GO
