USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpPublicationType](
    [Type] [varchar](3) NOT NULL,
    [Description] [varchar](50) NULL
,    CONSTRAINT [PK_tlkpPublicationType] PRIMARY KEY CLUSTERED
    (
        Type
    )
) ON [PRIMARY]
GO
