USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblEUGrade_Conversion](
    [VLAGrade] [varchar](50) NOT NULL,
    [EUGrade] [varchar](50) NULL
,    CONSTRAINT [PK_tblEUGrade_Conversion] PRIMARY KEY NONCLUSTERED
    (
        VLAGrade
    )
) ON [PRIMARY]
GO
