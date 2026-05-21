USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPublication](
    [UID] [int] IDENTITY(1,1) NOT NULL,
    [Identifier] [varchar](50) NOT NULL,
    [Type] [varchar](3) NOT NULL,
    [Program] [varchar](10) NOT NULL,
    [Subject] [varchar](500) NULL,
    [LeadAuthor] [varchar](50) NULL,
    [OtherAuthors] [varchar](255) NULL,
    [TargetDate] [smalldatetime] NULL,
    [Submitted] [smalldatetime] NULL,
    [Published] [bit] NOT NULL,
    [Comments] [text] NULL
,    CONSTRAINT [IX_tblPublication] UNIQUE NONCLUSTERED
    (
        Identifier
    )
,    CONSTRAINT [PK_tblPublication] PRIMARY KEY CLUSTERED
    (
        UID
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblPublication] WITH CHECK ADD CONSTRAINT [FK_tblPublication_tlkpPublicationType] FOREIGN KEY(Type)
REFERENCES [dbo].[tlkpPublicationType] (Type)
GO
ALTER TABLE [dbo].[tblPublication] CHECK CONSTRAINT [FK_tblPublication_tlkpPublicationType]
GO
