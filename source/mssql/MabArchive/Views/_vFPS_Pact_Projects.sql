USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[_vFPS_Pact_Projects]
AS
SELECT 1998 AS year, CASE WHEN P.ParentProject IS NULL 
    THEN F.parentproject ELSE P.parentProject END AS parentproject,
     CASE WHEN p.program IS NULL 
    THEN f.program ELSE p.program END AS program, 
    CASE WHEN p.customer IS NULL 
    THEN f.customer ELSE p.customer END AS customer, 
    CASE WHEN p.manager IS NULL 
    THEN f.manager ELSE p.manager END AS manager, 
    f.transferincome, f.custincome, p.wip_eoy, p.wip_limit, 
    P.wip_current, CASE WHEN p.projectstatus IS NULL 
    THEN f.projectstatus ELSE p.projectstatus END AS projectstatus,
     f.datecreated, f.feccost, f.profit, p.budget_cvl, 
    CASE WHEN p.parentproject IS NULL 
    THEN 'FPS' ELSE 'Pact' END AS source
FROM FPS.dbo.tlkpProject F FULL OUTER JOIN
    PACT.dbo.tlkpProject P ON F.ParentProject = P.ParentProject

GO
