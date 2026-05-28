USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblImages](
    [ImageID] [int] NOT NULL,
    [Image] [image] NULL,
    [Decription] [varchar](50) NULL
,    CONSTRAINT [PK_tblImages] PRIMARY KEY CLUSTERED
    (
        ImageID
    )
) ON [PRIMARY]
GO
