USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Proj_Invoice](
    [ProjectParent] [varchar](20) NOT NULL,
    [Month] [int] NULL,
    [Amount] [money] NULL,
    [CostOfWork] [money] NULL,
    [WIP] [money] NULL,
    [ProfitLoss] [money] NULL,
    [Detail] [varchar](100) NULL,
    [InvoiceCounter] [int] IDENTITY(1,1) NOT NULL,
    [TimeStamp] [timestamp] NOT NULL,
    [x] [varchar](5) NULL,
    [Type] [varchar](10) NULL
,    CONSTRAINT [PK_Proj_Invoice_1__13] PRIMARY KEY CLUSTERED
    (
        InvoiceCounter
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Proj_Invoice] WITH CHECK ADD CONSTRAINT [FK_Proj_Invoice_1__11] FOREIGN KEY(ProjectParent)
REFERENCES [dbo].[tlkpProject] (ParentProject)
GO
ALTER TABLE [dbo].[Proj_Invoice] CHECK CONSTRAINT [FK_Proj_Invoice_1__11]
GO
ALTER TABLE [dbo].[Proj_Invoice] WITH CHECK ADD CONSTRAINT [CK_Proj_Invoice_2__22] CHECK ([Type]='PVSIncome' OR [Type]='CVOGIncome')
GO
