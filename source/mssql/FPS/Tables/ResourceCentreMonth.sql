USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResourceCentreMonth](
    [ResourceCentre] [varchar](50) NOT NULL,
    [MonthNo] [int] NOT NULL,
    [PaySpent] [money] NULL,
    [NonPaySpent] [money] NULL,
    [PayBudget] [money] NULL,
    [NonPayBudget] [money] NULL,
    [Spare1] [money] NULL,
    [Spare2] [money] NULL,
    [Spare3] [money] NULL,
    [Spare4] [money] NULL,
    [Spare5] [money] NULL,
    [Spare6] [money] NULL
,    CONSTRAINT [PK_ResourceCentreMonth] PRIMARY KEY NONCLUSTERED
    (
        ResourceCentre, MonthNo
    )
) ON [PRIMARY]
GO
