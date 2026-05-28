USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblkpPlanningCategory](
    [PlanningCategory] [varchar](50) NOT NULL,
    [PlanCategoryDesc] [varchar](50) NULL,
    [CustomerGroup] [varchar](50) NULL,
    [Corporate] [varchar](50) NULL,
    [Divisional] [varchar](50) NULL
,    CONSTRAINT [PK__tblkpPlanningCat__05B8E52D] PRIMARY KEY CLUSTERED
    (
        PlanningCategory
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [CustomerGroup] ON [dbo].[tblkpPlanningCategory]
(
    CustomerGroup
)
GO
