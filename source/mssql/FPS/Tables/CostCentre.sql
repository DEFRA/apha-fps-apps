USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CostCentre](
    [CostCentre] [float] NOT NULL,
    [ProfitCentre] [varchar](50) NOT NULL
,    CONSTRAINT [PK_CostCentre_1] PRIMARY KEY CLUSTERED
    (
        CostCentre
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CostCentre] WITH CHECK ADD CONSTRAINT [FK_CostCentre_tblkpProfitCentre] FOREIGN KEY(ProfitCentre)
REFERENCES [dbo].[tblkpProfitCentre] (ProfitCentre)
GO
ALTER TABLE [dbo].[CostCentre] CHECK CONSTRAINT [FK_CostCentre_tblkpProfitCentre]
GO
