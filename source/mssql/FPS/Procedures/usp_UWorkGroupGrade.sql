USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.usp_UWorkGroupGrade    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Stored Procedure dbo.usp_UWorkGroupGrade    Script Date: 1/12/99 12:14:26 PM ******/
CREATE proc [dbo].[usp_UWorkGroupGrade] 
	@WGGrade_old varchar(50),
	@WGGrade_new varchar(50),
	@ProfitCentreGrade varchar(20),
	@GradeCode varchar(10),
	@WorkGroup varchar(50),
	@ChargeRateWG float = NULL,
	@DirectRateWG money = NULL,
	@PayRateWG money = NULL,
	@NPRWG money = NULL,
	@OHRWG money = NULL,
	@AvSalary money = NULL,
	@HrsChangedBy varchar(50) = 'None'
AS
UPDATE	vWorkGroupGrade
SET	WGGrade = @WGGrade_new,
	ProfitCentreGrade = @ProfitCentreGrade,
	GradeCode = @GradeCode,
	WorkGroup = @WorkGroup,
	ChargeRateWG = @ChargeRateWG,
	DirectRateWG = @DirectRateWG,
	PayRateWG = @PayRateWG,
	NPRWG = @NPRWG,
	OHRWG = @OHRWG,
	AvSalary = @AvSalary,
	HrsChangedBy = @HrsChangedBy
WHERE	WGGrade = @WGGrade_old

GO
