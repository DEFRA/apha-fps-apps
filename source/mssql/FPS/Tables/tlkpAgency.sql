USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpAgency](
    [AgencyID] [int] IDENTITY(1,1) NOT NULL,
    [AgencyName] [varchar](18) NOT NULL
,    CONSTRAINT [PK__tlkpAgency__089551D8] PRIMARY KEY CLUSTERED
    (
        AgencyID
    )
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [AgencyName] ON [dbo].[tlkpAgency]
(
    AgencyName
)
GO
