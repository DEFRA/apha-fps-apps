USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblkpProfitCentre](
    [ProfitCentre] [varchar](50) NOT NULL,
    [ProfitCentreName] [varchar](40) NOT NULL,
    [Division] [varchar](10) NOT NULL,
    [CONTTARGET] [money] NULL CONSTRAINT [DF__tblkpProf__CONTT__1BC821DD] DEFAULT (0),
    [ProfitCentreHead] [varchar](50) NULL,
    [DivisionID] [int] NULL CONSTRAINT [DF__tblkpProf__Divis__1CBC4616] DEFAULT (0),
    [Email_Recipient] [varchar](50) NULL,
    [TimeSheetLayout] [tinyint] NULL,
    [TimeSheet] [int] NULL,
    [OutputSheet] [int] NULL,
    [PACTCoordinatorEmailName] [varchar](50) NULL,
    [HighLevelSummary] [image] NULL
,    CONSTRAINT [PK__tblkpProfitCentr__1DB06A4F] PRIMARY KEY CLUSTERED
    (
        ProfitCentre
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblkpProfitCentre] WITH CHECK ADD CONSTRAINT [FK_tblkpProfitCentre_1__18] FOREIGN KEY(Division)
REFERENCES [dbo].[tlkpDivision] (DivName)
GO
ALTER TABLE [dbo].[tblkpProfitCentre] CHECK CONSTRAINT [FK_tblkpProfitCentre_1__18]
GO
CREATE NONCLUSTERED INDEX [Division] ON [dbo].[tblkpProfitCentre]
(
    Division
)
GO
