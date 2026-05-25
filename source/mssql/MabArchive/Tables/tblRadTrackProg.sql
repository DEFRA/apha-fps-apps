USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblRadTrackProg](
    [Program] [varchar](10) NOT NULL,
    [RadTrackProg] [bit] NOT NULL CONSTRAINT [DF_tblRadTrackProg_RadTrackProg] DEFAULT ((1)),
    [PublicationPrefix] [varchar](5) NULL
,    CONSTRAINT [PK_tblRadTrackProg] PRIMARY KEY NONCLUSTERED
    (
        Program
    )
) ON [PRIMARY]
GO
