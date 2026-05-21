USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  View dbo.vtlkpProjectNoZT    Script Date: 3/4/00 1:48:16 PM ******/
/****** Object:  View dbo.vtlkpProjectNoZT    Script Date: 1/12/99 12:13:47 PM ******/
/****** Object:  View dbo.vtlkpProjectNoZT    Script Date: 4/23/98 10:23:01 AM ******/
CREATE VIEW [dbo].[vtlkpProjectNoZT] AS
SELECT	*
FROM 	tlkpProject
WHERE	Program Not Like 'ZT%'


GO
