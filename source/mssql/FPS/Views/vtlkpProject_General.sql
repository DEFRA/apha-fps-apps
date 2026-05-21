USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vtlkpProject_General    Script Date: 3/4/00 1:48:16 PM *****
***** Object:  View dbo.vtlkpProject_General    Script Date: 1/12/99 12:13:47 PM ******/
CREATE VIEW [dbo].[vtlkpProject_General]
AS
SELECT     ParentProject, ProjectTitle, ShortTitle, Program, ProjectStatus, Customer, Manager, Disease, Contract, DateCreated, IsDefraProject, ProjectGroup
FROM         dbo.tlkpProject

GO
