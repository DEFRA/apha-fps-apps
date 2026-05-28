USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Trigger dbo.tlkpProgram_UTrig    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tlkpProgram_UTrig    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tlkpProgram_UTrig] on [dbo].[tlkpProgram]
  for UPDATE
  as
/* ERwin Builtin Wed Jan 28 15:21:55 1998 */
/* UPDATE trigger on tlkpProgram */
/* default body for tlkpProgram_UTrig */
begin
  declare  @numrows int,
           @nullcnt int,
           @validcnt int,
           @insProgramNo varchar(10),
           @errno   int,
           @errmsg  varchar(255)
  select @numrows = @@rowcount
/* ERwin Builtin Wed Jan 28 15:21:55 1998 */
  /* tlkpProgram R/84 tblUser_Program ON PARENT UPDATE CASCADE */
  if
    /* %ParentPK(" or",update) */
    update(ProgramNo)
  begin
    if @numrows = 1
    begin
      select @insProgramNo = inserted.ProgramNo
        from inserted
      update tblUser_Program
      set
        /*  %JoinFKPK(tblUser_Program,@ins," = ",",") */
        tblUser_Program.ProgramNo = @insProgramNo
      from tblUser_Program,inserted,deleted
      where
        /*  %JoinFKPK(tblUser_Program,deleted," = "," and") */
        tblUser_Program.ProgramNo = deleted.ProgramNo
    end
    else
    begin
      select @errno = 30006,
             @errmsg = 'Cannot cascade "tlkpProgram" UPDATE because more than one row has been affected.'
      goto error
    end
  end
/* ERwin Builtin Wed Jan 28 15:21:55 1998 */
  /* tlkpProgram R/58 tlkpProject ON PARENT UPDATE CASCADE */
  if
    /* %ParentPK(" or",update) */
    update(ProgramNo)
  begin
    if @numrows = 1
    begin
      select @insProgramNo = inserted.ProgramNo
        from inserted
      update tlkpProject
      set
        /*  %JoinFKPK(tlkpProject,@ins," = ",",") */
        tlkpProject.Program = @insProgramNo
      from tlkpProject,inserted,deleted
      where
        /*  %JoinFKPK(tlkpProject,deleted," = "," and") */
        tlkpProject.Program = deleted.ProgramNo
    end
    else
    begin
      select @errno = 30006,
             @errmsg = 'Cannot cascade "tlkpProgram" UPDATE because more than one row has been affected.'
      goto error
    end
  end
  return
error:
    raiserror (@errmsg, @errno, 1)
    rollback transaction
end


GO
