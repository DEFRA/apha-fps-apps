USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Stored Procedure dbo.sp_Delete_tr    Script Date: 3/4/00 1:48:22 PM ******/
/****** Object:  Stored Procedure dbo.sp_Delete_tr    Script Date: 7/22/99 12:07:57 PM ******/
CREATE procEDURE [dbo].[sp_Delete_tr] 
@OldCode VarChar(20)
AS
DELETE FROM tlkpTestReqmt
WHERE ProjectBuyerCode = @OldCode

GO
