USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblCSG7_AccountGroups](
    [CSG7Group] [nvarchar](15) NOT NULL,
    [UseInflation] [bit] NULL CONSTRAINT [DF__Temporary__UseIn__1B0907CE] DEFAULT (1)
,    CONSTRAINT [aaaaatblCSG7_AccountGroups_PK] PRIMARY KEY NONCLUSTERED
    (
        CSG7Group
    )
) ON [PRIMARY]
GO
