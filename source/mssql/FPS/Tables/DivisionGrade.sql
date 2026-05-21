USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DivisionGrade](
    [DivisionGrade] [varchar](10) NOT NULL,
    [GradeCode] [varchar](10) NOT NULL,
    [Division] [varchar](10) NOT NULL,
    [ChargeRate] [money] NULL CONSTRAINT [DF__DivisionG__Charg__1D906EBE] DEFAULT (0),
    [DirectRate] [money] NULL CONSTRAINT [DF__DivisionG__Direc__1E8492F7] DEFAULT (0),
    [PayRate] [money] NULL CONSTRAINT [DF__DivisionG__PayRa__1F78B730] DEFAULT (0),
    [NPR] [money] NULL CONSTRAINT [DF__DivisionGra__NPR__206CDB69] DEFAULT (0),
    [OHR] [money] NULL CONSTRAINT [DF__DivisionGra__OHR__2160FFA2] DEFAULT (0)
,    CONSTRAINT [PK__DivisionGrade__225523DB] PRIMARY KEY CLUSTERED
    (
        DivisionGrade
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DivisionGrade] WITH CHECK ADD CONSTRAINT [FK__DivisionG__Divis__123EB7A3] FOREIGN KEY(Division)
REFERENCES [dbo].[tlkpDivision] (DivName)
GO
ALTER TABLE [dbo].[DivisionGrade] CHECK CONSTRAINT [FK__DivisionG__Divis__123EB7A3]
GO
ALTER TABLE [dbo].[DivisionGrade] WITH CHECK ADD CONSTRAINT [FK__DivisionG__Grade__7B064C90] FOREIGN KEY(GradeCode)
REFERENCES [dbo].[Grade] (GradeCode)
GO
ALTER TABLE [dbo].[DivisionGrade] CHECK CONSTRAINT [FK__DivisionG__Grade__7B064C90]
GO
