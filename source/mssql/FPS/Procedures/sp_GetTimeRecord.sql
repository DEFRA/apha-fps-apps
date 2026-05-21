USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_GetTimeRecord    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Stored Procedure dbo.sp_GetTimeRecord    Script Date: 1/12/99 12:14:27 PM ******/
CREATE procedure [dbo].[sp_GetTimeRecord]
@@PACTStaffID varchar (50), 
@@TimeCode varchar(20), 
@@ParentProject varchar(20), 
@@WorkGroup varchar(20), 
@@Month int 
as
select * from MonthlyTime
where PACTStaffID = @@PACTStaffID
and TimeCode = @@TimeCode
and ParentProject = @@ParentProject
and WorkGroup = @@WorkGroup
and Month = @@Month

GO
