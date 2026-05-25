USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBid](
    [WorkGroup] [varchar](50) NOT NULL,
    [Account] [varchar](50) NOT NULL,
    [GenBid] [money] NOT NULL CONSTRAINT [DF__tblBid__GenBid__660B2FAA] DEFAULT (0),
    [SysTimeStamp] [timestamp] NULL
,    CONSTRAINT [PK__tblBid__66FF53E3] PRIMARY KEY CLUSTERED
    (
        WorkGroup, Account
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblBid] WITH CHECK ADD CONSTRAINT [FK__tblBid__Account__10F58DAF] FOREIGN KEY(Account)
REFERENCES [dbo].[tblkpAccountCategory] (AccShortName)
GO
ALTER TABLE [dbo].[tblBid] CHECK CONSTRAINT [FK__tblBid__Account__10F58DAF]
GO
ALTER TABLE [dbo].[tblBid] WITH CHECK ADD CONSTRAINT [FK__tblBid__WorkGrou__3493CFA7] FOREIGN KEY(WorkGroup)
REFERENCES [dbo].[WorkGroup] (WorkGroup)
GO
ALTER TABLE [dbo].[tblBid] CHECK CONSTRAINT [FK__tblBid__WorkGrou__3493CFA7]
GO
