USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_GetOutPutRecord    Script Date: 3/4/00 1:48:22 PM ******/
/****** Object:  Stored Procedure dbo.sp_GetOutPutRecord    Script Date: 1/12/99 12:14:26 PM ******/
CREATE procedure [dbo].[sp_GetOutPutRecord]
@@TestCode varchar(20), 
@@Buyer varchar(20),
@@Month int, 
@@WorkGroup varchar(20) 
as
select * from MonthlyOutput
where TestCode = @@TestCode 
and Buyer = @@Buyer 
and Month = @@Month
and WorkGroup = @@WorkGroup

GO
