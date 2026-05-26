USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpDivision](
    [DivisionID] [int] NULL,
    [AgencyID] [int] NOT NULL,
    [DivName] [varchar](10) NOT NULL,
    [CentOverhead] [money] NULL CONSTRAINT [DF__tlkpDivis__CentO__0F624AF8] DEFAULT (0)
,    CONSTRAINT [PK__tlkpDivision__10566F31] PRIMARY KEY CLUSTERED
    (
        DivName
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tlkpDivision] WITH CHECK ADD CONSTRAINT [FK_tlkpDivision_1__14] FOREIGN KEY(AgencyID)
REFERENCES [dbo].[tlkpAgency] (AgencyID)
GO
ALTER TABLE [dbo].[tlkpDivision] CHECK CONSTRAINT [FK_tlkpDivision_1__14]
GO
CREATE UNIQUE NONCLUSTERED INDEX [DivName] ON [dbo].[tlkpDivision]
(
    DivName
)
GO
