USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.usp_IWorkGroupGrade    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Stored Procedure dbo.usp_IWorkGroupGrade    Script Date: 1/12/99 12:14:26 PM ******/
CREATE proc [dbo].[usp_IWorkGroupGrade] 
	@WGGrade varchar(50),
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
INSERT INTO vWorkGroupGrade
VALUES (@WGGrade,
	@ProfitCentreGrade,
	@GradeCode,
	@WorkGroup,
	@ChargeRateWG,
	@DirectRateWG,
	@PayRateWG,
	@NPRWG,
	@OHRWG,
	@AvSalary,
	@HrsChangedBy)

GO
