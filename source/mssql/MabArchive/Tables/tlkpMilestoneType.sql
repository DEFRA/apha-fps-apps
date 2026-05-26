USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpMilestoneType](
    [IDType] [char](1) NOT NULL,
    [Type] [varchar](50) NULL,
    [MilestoneDeliverable] [char](1) NULL
,    CONSTRAINT [PK_tlkpMilestoneType] PRIMARY KEY CLUSTERED
    (
        IDType
    )
) ON [PRIMARY]
GO
