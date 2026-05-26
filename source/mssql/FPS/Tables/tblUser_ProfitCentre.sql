USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUser_ProfitCentre](
    [ProfitCentre] [varchar](50) NOT NULL,
    [User_ID] [int] NOT NULL
,    CONSTRAINT [PK__tblUser_ProfitCe__77BFCB91] PRIMARY KEY CLUSTERED
    (
        ProfitCentre, User_ID
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [XIF89tblUser_ProfitCentre] ON [dbo].[tblUser_ProfitCentre]
(
    User_ID
)
GO
CREATE NONCLUSTERED INDEX [XIF90tblUser_ProfitCentre] ON [dbo].[tblUser_ProfitCentre]
(
    ProfitCentre
)
GO
