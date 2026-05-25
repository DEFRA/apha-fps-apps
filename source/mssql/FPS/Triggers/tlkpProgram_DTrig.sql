USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Trigger dbo.tlkpProgram_DTrig    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tlkpProgram_DTrig    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tlkpProgram_DTrig] on [dbo].[tlkpProgram]
  for DELETE
  as
/* ERwin Builtin Wed Jan 28 15:21:55 1998 */
/* DELETE trigger on tlkpProgram */
/* default body for tlkpProgram_DTrig */
begin
  declare  @numrows int,
           @nullcnt int,
           @validcnt int,
           @insProgramNo varchar(10),
           @errno   int,
           @errmsg  varchar(255)
  select @numrows = @@rowcount
/* ERwin Builtin Wed Jan 28 15:21:55 1998 */
    /* tlkpProgram R/84 tblUser_Program ON PARENT DELETE CASCADE */
    delete tblUser_Program
      from tblUser_Program,deleted
      where
        /*  %JoinFKPK(tblUser_Program,deleted," = "," and") */
        tblUser_Program.ProgramNo = deleted.ProgramNo
/* ERwin Builtin Wed Jan 28 15:21:55 1998 */
    /* tlkpProgram R/58 tlkpProject ON PARENT DELETE RESTRICT */
    if exists (
      select * from deleted,tlkpProject
      where
        /*  %JoinFKPK(tlkpProject,deleted," = "," and") */
        tlkpProject.Program = deleted.ProgramNo
    )
    begin
      select @errno  = 30001,
             @errmsg = 'Cannot DELETE "tlkpProgram" because "tlkpProject" exists.'
      goto error
    end
  return
error:
    raiserror (@errmsg, @errno, 1)
    rollback transaction
end


GO
