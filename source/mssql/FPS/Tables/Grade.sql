USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Grade](
    [GradeCode] [varchar](10) NOT NULL,
    [DESC_LONG] [varchar](30) NULL,
    [AvSalary] [money] NULL CONSTRAINT [DF__Grade__AvSalary__17D79568] DEFAULT (0),
    [PACTcode] [varchar](50) NULL,
    [AvLeaveHrs] [float] NULL CONSTRAINT [DF__Grade__AvLeaveHr__18CBB9A1] DEFAULT (0),
    [AvSickHrs] [float] NULL CONSTRAINT [DF__Grade__AvSickHrs__19BFDDDA] DEFAULT (0)
,    CONSTRAINT [PK__Grade__1AB40213] PRIMARY KEY CLUSTERED
    (
        GradeCode
    )
) ON [PRIMARY]
GO
