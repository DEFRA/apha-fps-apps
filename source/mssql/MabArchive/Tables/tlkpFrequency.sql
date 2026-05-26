USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpFrequency](
    [FrequencyID] [int] NOT NULL,
    [Frequency] [varchar](50) NULL
,    CONSTRAINT [PK_tlkpFrequency] PRIMARY KEY CLUSTERED
    (
        FrequencyID
    )
) ON [PRIMARY]
GO
