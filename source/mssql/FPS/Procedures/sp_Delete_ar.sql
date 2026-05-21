USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[sp_Delete_ar]
@OldCode VarChar(20)
AS
DELETE FROM tblanimalreq
WHERE jobcode = @OldCode

GO
