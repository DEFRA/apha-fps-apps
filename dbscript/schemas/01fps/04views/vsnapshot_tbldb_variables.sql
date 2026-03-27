-- View: fps.vsnapshot_tbldb_variables

CREATE OR REPLACE VIEW fps.vsnapshot_tbldb_variables AS
 SELECT db_var_name,
    db_var_value
   FROM fps.tbldb_variables;
