USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanCatWGGrade](
    [PlanCategory] [varchar](50) NOT NULL,
    [WGGrade] [varchar](50) NOT NULL,
    [Hours] [int] NULL CONSTRAINT [DF__PlanCatWG__Hours__375040C1] DEFAULT (0),
    [CreatedBy] [varchar](10) NULL,
    [SellerAgrees] [varchar](10) NULL,
    [BuyerAgrees] [varchar](10) NULL
,    CONSTRAINT [PK__PlanCatWGGrade__384464FA] PRIMARY KEY CLUSTERED
    (
        PlanCategory, WGGrade
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PlanCatWGGrade] WITH CHECK ADD CONSTRAINT [FK__PlanCatWG__PlanC__01B34A1F] FOREIGN KEY(PlanCategory)
REFERENCES [dbo].[tblkpPlanningCategory] (PlanningCategory)
GO
ALTER TABLE [dbo].[PlanCatWGGrade] CHECK CONSTRAINT [FK__PlanCatWG__PlanC__01B34A1F]
GO
ALTER TABLE [dbo].[PlanCatWGGrade] WITH CHECK ADD CONSTRAINT [FK__PlanCatWG__WGGra__3587F3E0] FOREIGN KEY(WGGrade)
REFERENCES [dbo].[WorkGroupGrade] (WGGrade)
GO
ALTER TABLE [dbo].[PlanCatWGGrade] CHECK CONSTRAINT [FK__PlanCatWG__WGGra__3587F3E0]
GO
