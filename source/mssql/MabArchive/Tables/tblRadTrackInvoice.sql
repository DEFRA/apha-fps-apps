USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblRadTrackInvoice](
    [InvoiceCounter] [int] IDENTITY(1,1) NOT NULL,
    [Project] [varchar](20) NULL,
    [PlannedAmount] [float] NULL,
    [DueAmount] [float] NULL,
    [DueDate] [datetime] NULL,
    [ActualAmount] [float] NULL,
    [DateInvoiced] [datetime] NULL,
    [Contract] [varchar](10) NULL,
    [DateJobsheetRaised] [datetime] NULL,
    [InvoiceRef] [varchar](50) NULL,
    [InvoicePaid] [smallint] NOT NULL CONSTRAINT [DF__tblRadTra__Invoi__11D4A34F] DEFAULT (0)
,    CONSTRAINT [PK_tblRadTrackInvoice] PRIMARY KEY NONCLUSTERED
    (
        InvoiceCounter
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblRadTrackInvoice] WITH CHECK ADD CONSTRAINT [FK_tblRadTrackInvoice_G_tlkpProject_RadTrackData] FOREIGN KEY(Project)
REFERENCES [dbo].[G_tlkpProject_RadTrackData] (ParentProject)
GO
ALTER TABLE [dbo].[tblRadTrackInvoice] CHECK CONSTRAINT [FK_tblRadTrackInvoice_G_tlkpProject_RadTrackData]
GO
ALTER TABLE [dbo].[tblRadTrackInvoice] WITH CHECK ADD CONSTRAINT [FK_tblRadTrackInvoice_tblRadtrackContract] FOREIGN KEY(Contract)
REFERENCES [dbo].[tblRadtrackContract] (Contract)
GO
ALTER TABLE [dbo].[tblRadTrackInvoice] CHECK CONSTRAINT [FK_tblRadTrackInvoice_tblRadtrackContract]
GO
