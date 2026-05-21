USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vMilestone] AS
SELECT Milestone.*
                        FROM Milestone INNER JOIN
                            vtlkpProject ON 
                            Milestone.Project = vtlkpProject.ParentProject
                             WITH CHECK OPTION

GO
