using Npgsql;

var dbName = args.Length > 0 ? args[0] : "postgres";
var cs = $"Host=fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com;Port=5432;Database={dbName};Username=fpsdev;Password=ijZFiEr5BnKoiLXxD1g7Zg;SSL Mode=Require;Trust Server Certificate=true;Timeout=15;Command Timeout=60";

await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();

async Task PrintAsync(string title, string sql)
{
    Console.WriteLine($"\n=== {title} ===");
    await using var cmd = new NpgsqlCommand(sql, conn);
    await using var reader = await cmd.ExecuteReaderAsync();
    for (var i = 0; i < reader.FieldCount; i++)
    {
        Console.Write(reader.GetName(i));
        if (i < reader.FieldCount - 1) Console.Write(" | ");
    }
    Console.WriteLine();

    while (await reader.ReadAsync())
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            Console.Write(reader.IsDBNull(i) ? "NULL" : reader.GetValue(i));
            if (i < reader.FieldCount - 1) Console.Write(" | ");
        }
        Console.WriteLine();
    }
}

await PrintAsync("context", "select current_database() as db, current_user as usr, now() as ts");

if (string.Equals(dbName, "postgres", StringComparison.OrdinalIgnoreCase))
{
        await PrintAsync("available databases", @"
select datname
from pg_database
where datistemplate = false
order by datname;");

    Console.WriteLine("\n=== database contains fps.job_master ===");
    await using (var dbCmd = new NpgsqlCommand("select datname from pg_database where datistemplate=false order by datname;", conn))
    await using (var dbReader = await dbCmd.ExecuteReaderAsync())
    {
        var dbs = new List<string>();
        while (await dbReader.ReadAsync())
            dbs.Add(dbReader.GetString(0));

        await dbReader.CloseAsync();

        foreach (var db in dbs)
        {
            try
            {
                var dbCs = $"Host=fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com;Port=5432;Database={db};Username=fpsdev;Password=ijZFiEr5BnKoiLXxD1g7Zg;SSL Mode=Require;Trust Server Certificate=true;Timeout=10;Command Timeout=20";
                await using var c = new NpgsqlConnection(dbCs);
                await c.OpenAsync();
                await using var ccmd = new NpgsqlCommand("select to_regclass('fps.job_master')::text as obj", c);
                var obj = (await ccmd.ExecuteScalarAsync())?.ToString();
                Console.WriteLine($"{db} | {(string.IsNullOrWhiteSpace(obj) ? "missing" : obj)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{db} | error: {ex.GetType().Name}");
            }
        }
    }
        return;
}

await PrintAsync("tables", @"
select table_name
from information_schema.tables
where table_schema='fps'
    and table_name in ('job_master','job_status','job_queue','job_queue_log','job_lock','job_cancellation_request')
order by table_name;");

await PrintAsync("schemas", @"
select schema_name
from information_schema.schemata
order by schema_name;");

await PrintAsync("tables like job% (all schemas)", @"
select table_schema, table_name
from information_schema.tables
where table_type='BASE TABLE'
    and table_name ilike '%job%'
order by table_schema, table_name;");

await PrintAsync("job_lock columns", @"
select column_name, data_type, is_nullable
from information_schema.columns
where table_schema='fps' and table_name='job_lock'
order by ordinal_position;");

await PrintAsync("job_queue columns", @"
select column_name, data_type, is_nullable
from information_schema.columns
where table_schema='fps' and table_name='job_queue'
order by ordinal_position;");

await PrintAsync("job_cancellation_request columns", @"
select column_name, data_type, is_nullable
from information_schema.columns
where table_schema='fps' and table_name='job_cancellation_request'
order by ordinal_position;");

await PrintAsync("critical indexes", @"
select tablename, indexname
from pg_indexes
where schemaname='fps'
    and indexname in (
        'uq_job_lock_job_name_active',
        'uq_job_queue_jobexecutionid',
        'idx_job_queue_requested_at_utc',
        'idx_job_cancel_requested_at',
        'idx_job_cancel_status')
order by tablename, indexname;");

try
{
        await PrintAsync("missing required statuses", @"
select jm.jobname, req.status
from fps.job_master jm
cross join (values
    ('Pending'),('Running'),('Retry'),('Completed'),('Failed'),('Cancelled'),('Skipped'),('CancelRequested')
) as req(status)
left join fps.job_status js on js.jobid = jm.jobid and js.status = req.status
where js.statusid is null
order by jm.jobname, req.status;");

        await PrintAsync("pending cancel count", "select count(*) as pending_count from fps.job_cancellation_request where status='Pending';");
        await PrintAsync("active lock count", "select count(*) as active_lock_count from fps.job_lock where is_active = true;");
}
catch (PostgresException ex)
{
        Console.WriteLine($"\nSchema-specific checks skipped: {ex.SqlState} {ex.MessageText}");
}
