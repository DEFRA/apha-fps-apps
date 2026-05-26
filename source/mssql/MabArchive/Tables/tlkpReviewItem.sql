USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpReviewItem](
    [ItemID] [int] NOT NULL,
    [Item] [varchar](50) NULL
,    CONSTRAINT [PK_tlkpReviewItem] PRIMARY KEY CLUSTERED
    (
        ItemID
    )
) ON [PRIMARY]
GO
