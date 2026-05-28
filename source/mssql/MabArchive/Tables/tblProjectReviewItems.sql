USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblProjectReviewItems](
    [Project] [varchar](50) NOT NULL,
    [ItemID] [int] NOT NULL,
    [FrequencyID] [int] NULL
,    CONSTRAINT [PK_tblProjectReviewItems] PRIMARY KEY CLUSTERED
    (
        Project, ItemID
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblProjectReviewItems] WITH CHECK ADD CONSTRAINT [FK_tblProjectReviewItems_tlkpFrequency] FOREIGN KEY(FrequencyID)
REFERENCES [dbo].[tlkpFrequency] (FrequencyID)
GO
ALTER TABLE [dbo].[tblProjectReviewItems] CHECK CONSTRAINT [FK_tblProjectReviewItems_tlkpFrequency]
GO
ALTER TABLE [dbo].[tblProjectReviewItems] WITH CHECK ADD CONSTRAINT [FK_tblProjectReviewItems_tlkpReviewItem] FOREIGN KEY(ItemID)
REFERENCES [dbo].[tlkpReviewItem] (ItemID)
GO
ALTER TABLE [dbo].[tblProjectReviewItems] CHECK CONSTRAINT [FK_tblProjectReviewItems_tlkpReviewItem]
GO
