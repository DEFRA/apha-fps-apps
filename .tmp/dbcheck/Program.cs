using Npgsql;

var targetDb = args.Length > 0 ? args[0] : "dbmig";
string cs = $"Host=fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com;Port=5432;Database={targetDb};Username=fpsdev;Password=ijZFiEr5BnKoiLXxD1g7Zg;SSL Mode=Require;Trust Server Certificate=true";

await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();
Console.WriteLine($"CONNECTED_DB={targetDb}");

async Task<List<string>> GetOneColAsync(string sql)
{
        var items = new List<string>();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
                items.Add(rdr.GetValue(0)?.ToString() ?? string.Empty);
        return items;
}

var missingViews = await GetOneColAsync(@"WITH required(view_name) AS (
    VALUES ('vpacttblstaff'),('vpacttlkptestcapability'),('qrymilestone1'),('qryjobmonthmilestone'),
                 ('qryprojectmonthcw'),('qryjobmonth_subcontracts'),('qryjobmonth_invoices'),('qryjobmonth_transferstotal')
)
SELECT r.view_name
FROM required r
LEFT JOIN information_schema.views v
    ON v.table_schema='fps' AND v.table_name=r.view_name
WHERE v.table_name IS NULL
ORDER BY 1;");
Console.WriteLine("MISSING_RECREATESUMMARIES_VIEWS=" + (missingViews.Count == 0 ? "none" : string.Join(",", missingViews)));

var missingConstraints = await GetOneColAsync(@"WITH required(conname) AS (
    VALUES ('pk_milestone_1__12'),('fk_milestone_project'),('aaaaatimecodevalid_pk'),('fk_timecodevalid_parentproject'),
                 ('pk__tlkptestcapabili__4e53a1aa'),('fk_tlkptestcapability_1__15'),('fk_tlkptestcapability_1__18'),('fk_tlkptestcapability_2__18')
)
SELECT r.conname
FROM required r
LEFT JOIN pg_constraint c ON c.conname=r.conname
LEFT JOIN pg_class t ON t.oid=c.conrelid
LEFT JOIN pg_namespace n ON n.oid=t.relnamespace AND n.nspname='fps'
WHERE c.conname IS NULL OR n.nspname IS NULL
ORDER BY 1;");
Console.WriteLine("MISSING_RECREATESUMMARIES_CONSTRAINTS=" + (missingConstraints.Count == 0 ? "none" : string.Join(",", missingConstraints)));

var tblyear = await GetOneColAsync(@"SELECT column_name || ':' || data_type || ':' || is_nullable
FROM information_schema.columns
WHERE table_schema='fps' AND table_name='tblyearmaster'
ORDER BY ordinal_position;");
Console.WriteLine("TBLYEARMASTER_COLUMNS=" + (tblyear.Count == 0 ? "missing" : string.Join("|", tblyear)));

var nullableFpsYear = await GetOneColAsync(@"SELECT table_name
FROM information_schema.columns
WHERE table_schema='fps'
    AND column_name='fpsyear'
    AND is_nullable='YES'
    AND table_name IN (
        'fpsyeartotals','monthlyoutput','monthlytime','profitcentregrade','proj_invoice',
        'proj_subcontract','projectmonthfinal','tbladditionalcosts','tblanimalreq','tblanimals',
        'tblcontract','tblemployee','tblstaffjob','tblwgemployee','testorproduct','timecostcalcs',
        'tlkpprogram','tlkpproject','tlkptestreqmt','workgroup','workgroupgrade'
    )
ORDER BY table_name;");
Console.WriteLine("NULLABLE_FPSYEAR_TABLES=" + (nullableFpsYear.Count == 0 ? "none" : string.Join(",", nullableFpsYear)));

var arCounterDefault = await GetOneColAsync(@"SELECT pg_get_expr(ad.adbin, ad.adrelid)
FROM pg_attrdef ad
JOIN pg_class c ON c.oid=ad.adrelid
JOIN pg_namespace n ON n.oid=c.relnamespace
JOIN pg_attribute a ON a.attrelid=c.oid AND a.attnum=ad.adnum
WHERE n.nspname='mabarchive' AND c.relname='my_tblanimalreq' AND a.attname='ar_counter';");
Console.WriteLine("MABARCHIVE_AR_COUNTER_DEFAULT=" + (arCounterDefault.Count == 0 ? "missing" : string.Join(";", arCounterDefault)));
