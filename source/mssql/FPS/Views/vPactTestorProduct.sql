USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.vPactTestorProduct    Script Date: 3/4/00 1:48:15 PM *****
***** Object:  View dbo.vPactTestorProduct    Script Date: 1/12/99 12:13:46 PM ******/
CREATE VIEW [dbo].[vPactTestorProduct]
AS
SELECT     ItemCode, ItemDescription, ShortDescription, TestManager, Owner, JobStatus, UnitPriceVLA AS UnitPriceVLAgen, PriceAHVG AS PriceAHVGx, 
                      DefraUnitPrice
FROM         dbo.TestOrProduct

GO
