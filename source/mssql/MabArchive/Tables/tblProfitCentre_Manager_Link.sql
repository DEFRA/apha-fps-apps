USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblProfitCentre_Manager_Link](
    [ProfitCentre] [varchar](50) NOT NULL,
    [Manager] [varchar](50) NOT NULL
,    CONSTRAINT [PK_tblProfitCentre_Manager] PRIMARY KEY CLUSTERED
    (
        ProfitCentre, Manager
    )
) ON [PRIMARY]
GO
