USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_ProfitCentreGrade](
    [Year] [int] NOT NULL,
    [PCGrade] [varchar](20) NOT NULL,
    [DivisionGrade] [varchar](10) NOT NULL,
    [GradeCode] [varchar](10) NOT NULL,
    [ProfitCentre] [varchar](50) NOT NULL,
    [ChargeRate] [money] NULL,
    [DirectRate] [money] NULL,
    [PayRate] [money] NULL,
    [NPR] [money] NULL,
    [OHR] [money] NULL
,    CONSTRAINT [PK__MY ProfitCentreGrad__2BDE8E15] PRIMARY KEY CLUSTERED
    (
        Year, PCGrade
    )
) ON [PRIMARY]
GO
