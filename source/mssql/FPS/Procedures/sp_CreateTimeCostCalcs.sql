USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[sp_CreateTimeCostCalcs] AS
/*Based on Create-TimeCostCals from Access FE*/
INSERT 	TimeCostCalcs 	(WorkGroup,
			JobCode,
			Project, 
			Month, 
			StaffID, 
			GradeCode, 
			Name,
			ChargeRate, 
			Class, 
			Time, 
			Cost, 
			Division,
			Pay,
			NonPay,
			OverHead)

SELECT DISTINCT 	WorkGroupGrade.WorkGroup,
			MonthlyTime.TimeCode AS JobCode, 
			TimeCodeValid.ParentProject AS Project, 
			MonthlyTime.Month, 
			vPacttblStaff.PACTid AS StaffID, 
			WorkGroupGrade.GradeCode, 
			vPacttblStaff.Name, 
			case tlkpProject.isdefraproject
				when 0 then
					ProfitCentreGrade.ChargeRate
				else
					ProfitCentreGrade.DefraChargeRate
			end as chargerate,
			CASE 
				WHEN tlkpProgram.sector_name='Charge' THEN
					'Charge'
				ELSE
					'Free'
			END AS Class,
			MonthlyTime.Hours AS Time, 
			CASE 
				WHEN tlkpProgram.sector_name='Charge' THEN
					hours  
				ELSE
					0
			END *
			case tlkpProject.isdefraproject
				when 0 then
					ProfitCentreGrade.ChargeRate
				else
					ProfitCentreGrade.DefraChargeRate
			end AS Cost, 
			tblkpProfitCentre.Division,
			MonthlyTime.Hours * ProfitCentreGrade.PayRate AS Pay,
			MonthlyTime.Hours * ProfitCentreGrade.NPR  AS NonPay,
			MonthlyTime.Hours *ProfitCentreGrade.OHR AS OverHead

FROM (((tblkpProfitCentre 
	INNER JOIN ProfitCentreGrade ON tblkpProfitCentre.ProfitCentre = ProfitCentreGrade.ProfitCentre)
	INNER JOIN WorkGroupGrade ON ProfitCentreGrade.PCGrade = WorkGroupGrade.ProfitCentreGrade) 
	INNER JOIN (TimeCodeValid 
	INNER JOIN (vPACTtblStaff 
	INNER JOIN MonthlyTime ON VPACTtblStaff.PACTid = MonthlyTime.PactStaffID) 
		ON (TimeCodeValid.WorkGroup = MonthlyTime.WorkGroup) 
		AND (TimeCodeValid.TimeCode = MonthlyTime.TimeCode) 
		AND (TimeCodeValid.ParentProject = MonthlyTime.ParentProject)) 
		ON WorkGroupGrade.WGGrade = vPACTtblStaff.WorkGroupGrade) 
	INNER JOIN tlkpProject ON TimeCodeValid.ParentProject = tlkpProject.ParentProject
INNER JOIN tlkpProgram ON tlkpProgram.ProgramNo = tlkpProject.Program

GO
