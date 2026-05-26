USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vWGJobcodes] AS
SELECT DISTINCT tlkpProject.ParentProject, 
TimeCodeValid.TimeCode, 
TimeCodeValid.WorkGroup, 
shorttitle + ': ' + JobcodeName  + ItemDescription AS Descript
FROM ((tlkpProject INNER JOIN TimeCodeValid ON tlkpProject.ParentProject = TimeCodeValid.ParentProject) LEFT JOIN tlkpJobCode ON (TimeCodeValid.ParentProject = tlkpJobCode.ParentProject) AND (TimeCodeValid.JobCode = tlkpJobCode.JobCode)) LEFT JOIN TestOrProduct ON TimeCodeValid.TestCode = TestOrProduct.ItemCode

GO
