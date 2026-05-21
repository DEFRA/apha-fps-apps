USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblCapsStaff](
    [mNumber] [varchar](50) NOT NULL,
    [Name] [varchar](50) NOT NULL,
    [DT2Number] [varchar](50) NULL
,    CONSTRAINT [PK_tblCapsStaff] PRIMARY KEY CLUSTERED
    (
        mNumber
    )
) ON [PRIMARY]
GO
