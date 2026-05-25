USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpRisk](
    [RiskID] [int] NOT NULL,
    [RiskRating] [varchar](15) NOT NULL
,    CONSTRAINT [PK_tlkpRisk] PRIMARY KEY CLUSTERED
    (
        RiskID
    )
) ON [PRIMARY]
GO
