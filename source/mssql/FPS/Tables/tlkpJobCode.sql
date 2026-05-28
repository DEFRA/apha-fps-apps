USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tlkpJobCode](
    [JobCode] [varchar](50) NOT NULL,
    [ParentProject] [varchar](20) NULL,
    [JobCodeWorkGroup] [varchar](50) NULL,
    [NewProg] [varchar](20) NULL,
    [Type] [varchar](15) NULL,
    [JobCodeName] [varchar](255) NULL
,    CONSTRAINT [PK_tlkpJobCode_new_1__15] PRIMARY KEY CLUSTERED
    (
        JobCode
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tlkpJobCode] WITH CHECK ADD CONSTRAINT [FK_tlkpJobCode_1__11] FOREIGN KEY(ParentProject)
REFERENCES [dbo].[tlkpProject] (ParentProject)
GO
ALTER TABLE [dbo].[tlkpJobCode] CHECK CONSTRAINT [FK_tlkpJobCode_1__11]
GO
ALTER TABLE [dbo].[tlkpJobCode] WITH CHECK ADD CONSTRAINT [CK_tlkpJobCode_1__11] CHECK (NOT [Type] IS NULL)
GO
