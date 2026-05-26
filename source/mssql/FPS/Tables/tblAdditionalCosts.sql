USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAdditionalCosts](
    [JobCode] [varchar](20) NOT NULL,
    [Account] [varchar](50) NOT NULL,
    [Description] [varchar](20) NOT NULL,
    [ItemCost] [money] NOT NULL CONSTRAINT [DF__tblAdditi__ItemC__151B244E] DEFAULT (0),
    [Freq] [varchar](5) NULL,
    [Supplier] [varchar](50) NULL
,    CONSTRAINT [PK__tblAdditionalCos__160F4887] PRIMARY KEY CLUSTERED
    (
        JobCode, Account, Description
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblAdditionalCosts] WITH CHECK ADD CONSTRAINT [FK_tblAdditionalCosts_1__18] FOREIGN KEY(JobCode)
REFERENCES [dbo].[tlkpProject] (ParentProject)
GO
ALTER TABLE [dbo].[tblAdditionalCosts] CHECK CONSTRAINT [FK_tblAdditionalCosts_1__18]
GO
ALTER TABLE [dbo].[tblAdditionalCosts] WITH CHECK ADD CONSTRAINT [FK_tblAdditionalCosts_2__18] FOREIGN KEY(Account)
REFERENCES [dbo].[tblkpAccountCategory] (AccShortName)
GO
ALTER TABLE [dbo].[tblAdditionalCosts] CHECK CONSTRAINT [FK_tblAdditionalCosts_2__18]
GO
