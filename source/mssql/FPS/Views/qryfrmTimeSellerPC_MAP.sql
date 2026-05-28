USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryfrmTimeSellerPC_MAP    Script Date: 3/4/00 1:48:20 PM ******/
CREATE VIEW [dbo].[qryfrmTimeSellerPC_MAP]  AS

SELECT tblkpProfitCentre.CONTTARGET, ProfitCentreGrade.ProfitCentre AS SellingPC, ProfitCentreGrade.ChargeRate, ProfitCentreGrade.OHR, vqryTBidSum.SumOfGenBid, WorkGroupGrade.WorkGroup, WorkGroupGrade.ProfitCentreGrade, WorkGroupGrade.WGGrade, vApphours.SumOfplannedhours AS AppHours, Sum(vstaffjobhours.plannedhours) AS Hrs, Sum(tblWGEmployee.HrsAvail) AS AvHrs, Sum(vStaffJobHours.plannedhours)*chargerate AS FEC, vApphours.sumofplannedhours*chargerate AS AppFEC, ohr*Sum(vStaffJobHours.plannedhours) AS Contribution
FROM (vApphours RIGHT JOIN (((tblkpProfitCentre INNER JOIN (ProfitCentreGrade LEFT JOIN vqryTBidSum ON ProfitCentreGrade.ProfitCentre = vqryTBidSum.ProfitCentre) ON tblkpProfitCentre.ProfitCentre = ProfitCentreGrade.ProfitCentre) INNER JOIN WorkGroupGrade ON ProfitCentreGrade.PCGrade = WorkGroupGrade.ProfitCentreGrade) INNER JOIN tblWGEmployee ON WorkGroupGrade.WGGrade = tblWGEmployee.WorkGroupGrade) ON vApphours.WorkGroupGrade = WorkGroupGrade.WGGrade) LEFT JOIN vstaffjobhours ON tblWGEmployee.PACTid = vstaffjobhours.StaffID

GROUP BY tblkpProfitCentre.CONTTARGET, ProfitCentreGrade.ProfitCentre, ProfitCentreGrade.ChargeRate, ProfitCentreGrade.OHR, vqryTBidSum.SumOfGenBid, WorkGroupGrade.WorkGroup, WorkGroupGrade.ProfitCentreGrade, WorkGroupGrade.WGGrade, vApphours.SumOfplannedhours

GO
