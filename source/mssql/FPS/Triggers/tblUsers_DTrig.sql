USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  Trigger dbo.tblUsers_DTrig    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Trigger dbo.tblUsers_DTrig    Script Date: 1/11/99 3:48:25 PM ******/
CREATE trigger[dbo].[tblUsers_DTrig] on [dbo].[tblUsers]
  for DELETE
  as
/* ERwin Builtin Mon Jan 05 17:39:22 1998 */
/* DELETE trigger on tblUsers */
/* default body for tblUsers_DTrig */
begin
  declare  @numrows int,
           @nullcnt int,
           @validcnt int,
           @insUser_ID int,
           @errno   int,
           @errmsg  varchar(255)
  select @numrows = @@rowcount
/* ERwin Builtin Mon Jan 05 17:39:22 1998 */
    /* tblUsers R/82 tblUser_Program ON PARENT DELETE CASCADE */
    delete tblUser_Program
      from tblUser_Program,deleted
      where
        /*  %JoinFKPK(tblUser_Program,deleted," = "," and") */
        tblUser_Program.User_ID = deleted.User_ID
  return
error:
    raiserror (@errmsg, @errno, 1)
    rollback transaction
end


GO
