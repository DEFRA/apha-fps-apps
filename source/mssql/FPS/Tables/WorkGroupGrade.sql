USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkGroupGrade](
    [WGGrade] [varchar](50) NOT NULL,
    [ProfitCentreGrade] [varchar](20) NOT NULL,
    [GradeCode] [varchar](10) NOT NULL,
    [WorkGroup] [varchar](50) NOT NULL,
    [ChargeRateWG] [money] NULL,
    [DirectRateWG] [money] NULL CONSTRAINT [DF__WorkGroup__Direc__282DF8C2] DEFAULT (0),
    [PayRateWG] [money] NULL CONSTRAINT [DF__WorkGroup__PayRa__29221CFB] DEFAULT (0),
    [NPRWG] [money] NULL CONSTRAINT [DF__WorkGroup__NPRWG__2A164134] DEFAULT (0),
    [OHRWG] [money] NULL CONSTRAINT [DF__WorkGroup__OHRWG__2B0A656D] DEFAULT (0),
    [AvSalary] [money] NULL CONSTRAINT [DF__WorkGroup__AvSal__2CF2ADDF] DEFAULT (0),
    [HrsChangedBy] [varchar](50) NULL
,    CONSTRAINT [PK__WorkGroupGrade__2DE6D218] PRIMARY KEY CLUSTERED
    (
        WGGrade
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[WorkGroupGrade] WITH CHECK ADD CONSTRAINT [FK_WorkGroupGrade_1__10] FOREIGN KEY(WorkGroup)
REFERENCES [dbo].[WorkGroup] (WorkGroup)
GO
ALTER TABLE [dbo].[WorkGroupGrade] CHECK CONSTRAINT [FK_WorkGroupGrade_1__10]
GO
ALTER TABLE [dbo].[WorkGroupGrade] WITH CHECK ADD CONSTRAINT [FK_WorkGroupGrade_2__10] FOREIGN KEY(ProfitCentreGrade)
REFERENCES [dbo].[ProfitCentreGrade] (PCGrade)
GO
ALTER TABLE [dbo].[WorkGroupGrade] CHECK CONSTRAINT [FK_WorkGroupGrade_2__10]
GO
ALTER TABLE [dbo].[WorkGroupGrade] WITH CHECK ADD CONSTRAINT [FK_WorkGroupGrade_5__10] FOREIGN KEY(GradeCode)
REFERENCES [dbo].[Grade] (GradeCode)
GO
ALTER TABLE [dbo].[WorkGroupGrade] CHECK CONSTRAINT [FK_WorkGroupGrade_5__10]
GO
