USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPurchase](
    [WorkGroup] [varchar](50) NOT NULL,
    [Account] [varchar](50) NOT NULL,
    [ItemDescription] [varchar](50) NOT NULL,
    [Amount] [money] NOT NULL CONSTRAINT [DF__tblPurcha__Amoun__69DBC08E] DEFAULT (0),
    [SysTimeStamp] [timestamp] NULL
,    CONSTRAINT [PK__tblPurchase__6ACFE4C7] PRIMARY KEY CLUSTERED
    (
        WorkGroup, Account, ItemDescription
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblPurchase] WITH CHECK ADD CONSTRAINT [FK__tblPurchase__12DDD621] FOREIGN KEY(WorkGroup, Account)
REFERENCES [dbo].[tblBid] (WorkGroup, Account)
GO
ALTER TABLE [dbo].[tblPurchase] CHECK CONSTRAINT [FK__tblPurchase__12DDD621]
GO
