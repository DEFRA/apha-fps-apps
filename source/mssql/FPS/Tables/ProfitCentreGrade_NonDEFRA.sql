USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfitCentreGrade_NonDEFRA](
    [PCGrade] [varchar](20) NOT NULL,
    [DivisionGrade] [varchar](10) NOT NULL,
    [GradeCode] [varchar](10) NOT NULL,
    [ProfitCentre] [varchar](50) NOT NULL,
    [ChargeRate] [money] NULL CONSTRAINT [DF__ProfitCen__Charg__666] DEFAULT ((0)),
    [DirectRate] [money] NULL CONSTRAINT [DF__ProfitCen__Direc__666] DEFAULT ((0)),
    [PayRate] [money] NULL CONSTRAINT [DF__ProfitCen__PayRa__666] DEFAULT ((0)),
    [NPR] [money] NULL CONSTRAINT [DF__ProfitCentr__NPR__666] DEFAULT ((0)),
    [OHR] [money] NULL CONSTRAINT [DF__ProfitCentr__OHR__666] DEFAULT ((0)),
    [HrsAvailable] [float] NULL CONSTRAINT [DF__ProfitCen__HrsAv__666] DEFAULT ((0)),
    [OldChargeRate] [money] NULL CONSTRAINT [DF__ProfitCen__OldCh__666] DEFAULT ((0))
,    CONSTRAINT [PK__ProfitCentreGrad__666] PRIMARY KEY CLUSTERED
    (
        PCGrade
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ProfitCentreGrade_NonDEFRA] WITH CHECK ADD CONSTRAINT [FK__ProfitCen__Divis__666] FOREIGN KEY(DivisionGrade)
REFERENCES [dbo].[DivisionGrade] (DivisionGrade)
GO
ALTER TABLE [dbo].[ProfitCentreGrade_NonDEFRA] CHECK CONSTRAINT [FK__ProfitCen__Divis__666]
GO
ALTER TABLE [dbo].[ProfitCentreGrade_NonDEFRA] WITH CHECK ADD CONSTRAINT [FK__ProfitCen__Grade__666] FOREIGN KEY(GradeCode)
REFERENCES [dbo].[Grade] (GradeCode)
GO
ALTER TABLE [dbo].[ProfitCentreGrade_NonDEFRA] CHECK CONSTRAINT [FK__ProfitCen__Grade__666]
GO
ALTER TABLE [dbo].[ProfitCentreGrade_NonDEFRA] WITH CHECK ADD CONSTRAINT [FK__ProfitCen__Profi__666] FOREIGN KEY(ProfitCentre)
REFERENCES [dbo].[tblkpProfitCentre] (ProfitCentre)
GO
ALTER TABLE [dbo].[ProfitCentreGrade_NonDEFRA] CHECK CONSTRAINT [FK__ProfitCen__Profi__666]
GO
