USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblContract](
    [ContractNo] [varchar](10) NOT NULL,
    [Category] [varchar](20) NOT NULL,
    [Manager] [varchar](50) NULL,
    [Customer] [varchar](50) NULL,
    [Title] [varchar](100) NULL,
    [RegisteredDate] [datetime] NULL,
    [StartDate] [datetime] NULL,
    [EndDate] [datetime] NULL,
    [ContractDoc] [image] NULL,
    [Duration] [int] NULL
,    CONSTRAINT [PK___2__10] PRIMARY KEY NONCLUSTERED
    (
        ContractNo
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblContract] WITH CHECK ADD CONSTRAINT [FK_tblContract_1__16] FOREIGN KEY(Customer)
REFERENCES [dbo].[tlkpCustomer] (Customer)
GO
ALTER TABLE [dbo].[tblContract] CHECK CONSTRAINT [FK_tblContract_1__16]
GO
ALTER TABLE [dbo].[tblContract] WITH CHECK ADD CONSTRAINT [FK_tblContract_3__10] FOREIGN KEY(Category)
REFERENCES [dbo].[tblCategory] (Category)
GO
ALTER TABLE [dbo].[tblContract] CHECK CONSTRAINT [FK_tblContract_3__10]
GO
