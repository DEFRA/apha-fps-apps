USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblProposedProject](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [ParentProject] [varchar](20) NOT NULL,
    [ProjectTitle] [varchar](200) NULL,
    [Program] [varchar](10) NULL,
    [Customer] [varchar](50) NULL,
    [Manager] [varchar](50) NULL,
    [ProjectStatus] [varchar](50) NULL,
    [CostBookNo] [varchar](50) NULL,
    [Disease] [varchar](50) NULL,
    [Reason] [varchar](250) NULL
,    CONSTRAINT [PK_tblProposedProject] PRIMARY KEY NONCLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [Project_index] ON [dbo].[tblProposedProject]
(
    ParentProject
)
GO
