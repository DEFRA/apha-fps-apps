CREATE OR REPLACE VIEW fps.vfthours AS
 SELECT 1.0 * (( SELECT tblsettings.setting::numeric AS setting
           FROM fps.tblsettings
          WHERE tblsettings.id::text = 'HoursInDay'::text)) AS fthoursperday,
    5.0 * (( SELECT tblsettings_1.setting::numeric AS setting
           FROM fps.tblsettings tblsettings_1
          WHERE tblsettings_1.id::text = 'HoursInDay'::text)) AS fthoursperweek,
    ( SELECT sum(tlkpmonthhours.cvlhours) AS fthourspaid
           FROM fps.tlkpmonthhours
          WHERE NOT tlkpmonthhours.year::text = (( SELECT "right"(tbldb_variables.db_var_value::text, 4) AS expr1
                   FROM fps.tbldb_variables
                  WHERE tbldb_variables.db_var_name::text = 'DB_Name'::text)) OR NOT tlkpmonthhours.month < 4) AS fthourspaidperyear;
