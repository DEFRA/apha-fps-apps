USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPaymentSchedule](
    [Contract] [varchar](10) NOT NULL,
    [DueDate] [datetime] NOT NULL,
    [Paid] [tinyint] NOT NULL
,    CONSTRAINT [PK___1__10] PRIMARY KEY CLUSTERED
    (
        Contract, DueDate
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblPaymentSchedule] WITH CHECK ADD CONSTRAINT [FK_tblPaymentSchedule_1__10] FOREIGN KEY(Contract)
REFERENCES [dbo].[tblContract] (ContractNo)
GO
ALTER TABLE [dbo].[tblPaymentSchedule] CHECK CONSTRAINT [FK_tblPaymentSchedule_1__10]
GO
