USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.All_WG_Project    Script Date: 3/4/00 1:48:16 PM ******/
CREATE VIEW [dbo].[All_WG_Project] AS
SELECT Workgroup, ParentProject as Project FROM Workgroup, tlkpProject

GO
