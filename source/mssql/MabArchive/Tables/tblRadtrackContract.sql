USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblRadtrackContract](
    [Contract] [varchar](10) NOT NULL
,    CONSTRAINT [PK_tblRadtrackContract] PRIMARY KEY CLUSTERED
    (
        Contract
    )
) ON [PRIMARY]
GO
