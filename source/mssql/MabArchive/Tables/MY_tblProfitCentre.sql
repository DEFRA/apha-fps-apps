USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_tblProfitCentre](
    [Year] [smallint] NOT NULL,
    [ProfitCentre] [varchar](50) NOT NULL,
    [ProfitCentreName] [varchar](40) NOT NULL,
    [Division] [varchar](10) NOT NULL,
    [CONTTARGET] [money] NULL,
    [ProfitCentreHead] [varchar](50) NULL,
    [DivisionID] [int] NULL
,    CONSTRAINT [PK__tblkpProfitCentr__1DB06A4F] PRIMARY KEY CLUSTERED
    (
        Year, ProfitCentre
    )
) ON [PRIMARY]
GO
