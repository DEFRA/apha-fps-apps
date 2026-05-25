USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Trigger dbo.tI_tlkpProject    Script Date: 3/4/00 1:48:24 PM ******/
/****** Object:  Trigger dbo.tI_tlkpProject    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tI_tlkpProject] on [dbo].[tlkpProject] for INSERT as
/* ERwin Builtin Wed Jan 07 11:44:32 1998 */
/* INSERT trigger on tlkpProject */
begin
  declare  @numrows int,
           @nullcnt int,
           @validcnt int,
           @errno   int,
           @errmsg  varchar(255)
  select @numrows = @@rowcount
  /* ERwin Builtin Wed Jan 07 11:44:32 1998 */
  /* tlkpProgram R/58 tlkpProject ON CHILD INSERT RESTRICT */
  if
    /* %ChildFK(" or",update) */
    update(Program)
  begin
    select @nullcnt = 0
    select @validcnt = count(*)
      from inserted,tlkpProgram
        where
          /* %JoinFKPK(inserted,tlkpProgram) */
          inserted.Program = tlkpProgram.ProgramNo
    /* %NotnullFK(inserted," is null","select @nullcnt = count(*) from inserted where"," and") */
    select @nullcnt = count(*) from inserted where
      inserted.Program is null
    if @validcnt + @nullcnt != @numrows
    begin
      select @errno  = 30002,
             @errmsg = 'Cannot INSERT "tlkpProject" because "tlkpProgram" does not exist.'
      goto error
    end
  end
  /* ERwin Builtin Wed Jan 07 11:44:32 1998 */
  return
error:
    raiserror (@errmsg, 16, 1)
    rollback transaction
end

GO
