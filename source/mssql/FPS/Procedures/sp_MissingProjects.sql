USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_MissingProjects    Script Date: 3/4/00 1:48:22 PM ******/
/****** Object:  Stored Procedure dbo.sp_MissingProjects    Script Date: 7/22/99 12:07:57 PM ******/
/****** Object:  Stored Procedure dbo.sp_MissingProjects    Script Date: 1/12/99 12:14:26 PM ******/
CREATE procedure [dbo].[sp_MissingProjects] as 
SELECT DISTINCT tlkpProject.ParentProject
FROM tlkpProject LEFT JOIN ProjectMonth ON tlkpProject.ParentProject = ProjectMonth.Project
WHERE ((ProjectMonth.Project Is Null))
ORDER BY ParentProject

GO
