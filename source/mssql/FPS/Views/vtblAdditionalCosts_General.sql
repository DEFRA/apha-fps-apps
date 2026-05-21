USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vtblAdditionalCosts_General]
AS
SELECT JobCode, Account, Description, ItemCost, Freq, 
    Supplier
FROM tblAdditionalCosts
WITH CHECK OPTION

GO
