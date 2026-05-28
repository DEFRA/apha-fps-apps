USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTotalBusinessOverheads](
    [TotalBusinessOverheads] [money] NULL
) ON [PRIMARY]
GO
CREATE UNIQUE CLUSTERED INDEX [TB_PK] ON [dbo].[tblTotalBusinessOverheads]
(
    TotalBusinessOverheads
)
GO
