USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfitCentreGrade](
    [PCGrade] [varchar](20) NOT NULL,
    [DivisionGrade] [varchar](10) NOT NULL,
    [GradeCode] [varchar](10) NOT NULL,
    [ProfitCentre] [varchar](50) NOT NULL,
    [ChargeRate] AS (([PayRate]+[NPR])+[OHR]),
    [DirectRate] [money] NULL CONSTRAINT [DF__ProfitCen__Direc__2625B4BF] DEFAULT ((0)),
    [PayRate] [money] NULL CONSTRAINT [DF__ProfitCen__PayRa__2719D8F8] DEFAULT ((0)),
    [NPR] [money] NULL CONSTRAINT [DF__ProfitCentr__NPR__280DFD31] DEFAULT ((0)),
    [OHR] [money] NULL CONSTRAINT [DF__ProfitCentr__OHR__2902216A] DEFAULT ((0)),
    [HrsAvailable] [float] NULL CONSTRAINT [DF__ProfitCen__HrsAv__29F645A3] DEFAULT ((0)),
    [OldChargeRate] [money] NULL CONSTRAINT [DF__ProfitCen__OldCh__2AEA69DC] DEFAULT ((0)),
    [DefraChargeRate] AS ([PayRate]+[NPR])
,    CONSTRAINT [PK__ProfitCentreGrad__2BDE8E15] PRIMARY KEY CLUSTERED
    (
        PCGrade
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ProfitCentreGrade] WITH CHECK ADD CONSTRAINT [FK__ProfitCen__Divis__7BFA70C9] FOREIGN KEY(DivisionGrade)
REFERENCES [dbo].[DivisionGrade] (DivisionGrade)
GO
ALTER TABLE [dbo].[ProfitCentreGrade] CHECK CONSTRAINT [FK__ProfitCen__Divis__7BFA70C9]
GO
ALTER TABLE [dbo].[ProfitCentreGrade] WITH CHECK ADD CONSTRAINT [FK__ProfitCen__Grade__7CEE9502] FOREIGN KEY(GradeCode)
REFERENCES [dbo].[Grade] (GradeCode)
GO
ALTER TABLE [dbo].[ProfitCentreGrade] CHECK CONSTRAINT [FK__ProfitCen__Grade__7CEE9502]
GO
ALTER TABLE [dbo].[ProfitCentreGrade] WITH CHECK ADD CONSTRAINT [FK__ProfitCen__Profi__30C33EC3] FOREIGN KEY(ProfitCentre)
REFERENCES [dbo].[tblkpProfitCentre] (ProfitCentre)
GO
ALTER TABLE [dbo].[ProfitCentreGrade] CHECK CONSTRAINT [FK__ProfitCen__Profi__30C33EC3]
GO
CREATE NONCLUSTERED INDEX [ProfitCentre] ON [dbo].[ProfitCentreGrade]
(
    ProfitCentre
)
GO
