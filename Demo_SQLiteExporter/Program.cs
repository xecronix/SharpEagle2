using Demo_SQLiteExporter;
using System.Reflection.Metadata.Ecma335;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Data.Common;
using SharpEagle2;
using XecronixCursor;

internal class Program
{

    private static bool RunTests()
    {
        if (
            ImporterTests.ImporterFromFilePath() &&
            ImporterTests.ImporterFromHardCodedList()
            )
        {
            Emitter.WriteLine("Tests Pass");
        }
        else { Emitter.WriteLine("Tests Fail"); return false; }
        return true;
    }

    private static int GetUserId(SqliteConnection conn, FlatData data)
    {
        int retval = 0;
        string sql = $"SELECT id FROM contacts WHERE first_name = @FirstName AND last_name = @LastName";
        using var cmd = new SqliteCommand(sql, conn);
        cmd.Parameters.AddWithValue("@FirstName", data.FirstName);
        cmd.Parameters.AddWithValue("@LastName", data.LastName);
        using var reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            retval = reader.GetInt32(0);
        }
        return retval;
    }

    private static void PrintDatabase(SqliteConnection conn) 
    {
        string sql = "SELECT * FROM contacts c JOIN addresses a on c.id=a.contact_id JOIN phones p on c.id=p.contact_id";
        QueryAndPrint(conn, sql);
    }

    private static void PrintContactsTable(SqliteConnection conn)
    {
        string sql = "SELECT * FROM contacts;";
        QueryAndPrint(conn, sql);
    }

    private static void PrintAddressesTable(SqliteConnection conn)
    {
        string sql = "SELECT * FROM Addresses;";
        QueryAndPrint(conn, sql);
    }

    private static void PrintPhonesTable(SqliteConnection conn)
    {
        string sql = "SELECT * FROM Phones;";
        QueryAndPrint(conn, sql);
    }

    private static void QueryAndPrint(SqliteConnection conn, string sql)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using (var reader = cmd.ExecuteReader())
        {
            // FieldCount returns the number of columns in each row
            int columnCount = reader.FieldCount;
            Emitter.WriteLine($"Number of columns: {columnCount}");

            while (reader.Read())
            {
                // You can iterate through columns using this count
                string line = "";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string value = $"{reader.GetValue(i)}";
                    line += $"|{value}|";
                }
                Emitter.WriteLine(line);
            }
        }
    }

    private static bool PopulateDemoDatabase(SqliteConnection conn, List<FlatData> data)
    {
        foreach (FlatData row in data)
        {
            int contactId = GetUserId(conn, row);
            
            if (contactId == 0) 
            {
                // I should add validation for field values.  But, for the demo, I'll skip it.

                var cmd = conn.CreateCommand();
                string sql = "INSERT INTO contacts (first_name, last_name) VALUES (@FirstName, @LastName);";
                cmd.Parameters.AddWithValue("@FirstName", row.FirstName);
                cmd.Parameters.AddWithValue("@LastName", row.LastName);
                cmd.CommandText = sql;
                var rowInserted = cmd.ExecuteNonQuery();
                contactId = GetUserId(conn, row);
            }

            if (contactId > 0)
            {
                var cmd = conn.CreateCommand();
                string sql = "INSERT INTO addresses (contact_id, street, city, zip) VALUES (@contactId, @Street, @City, @Zip);";
                sql += "INSERT INTO phones (contact_id, phone) VALUES (@contactId, @Phone);";
                cmd.Parameters.AddWithValue("@contactId", contactId);
                cmd.Parameters.AddWithValue("@Street", row.Street);
                cmd.Parameters.AddWithValue("@City", row.City);
                cmd.Parameters.AddWithValue("@Zip", row.Zip);
                cmd.Parameters.AddWithValue("@Phone", row.Phone);
                cmd.CommandText = sql;
                var rowInserted = cmd.ExecuteNonQuery();
            }
            else 
            {
                throw new Exception("Failed to insert record");
            }
        }

        return true;

    }
    private static bool CreateDemoDatabase(string schemaFilename, string dbFilename, string dataSource)
    {
        using var conn = new SqliteConnection(Config.ConnStr);
        conn.Open();
        if (!File.Exists(schemaFilename)) { throw new Exception($"Could not find schema {schemaFilename}"); }
        if (!File.Exists(dataSource)) { throw new Exception($"Could not find schema {dataSource}"); }

        // fetch the data from the seeded .dat file.
        List<FlatData> data = new List<FlatData>();
        Importer imp = new Importer();
        bool success = imp.Import(dataSource, out data);
        if (!success) { throw new Exception("Could not import datasource"); }

        // Now lets create a database
        // Example to create a table

        var cmd = conn.CreateCommand();
        cmd.CommandText = File.ReadAllText(schemaFilename);
        cmd.ExecuteNonQuery();


        if (!File.Exists(dbFilename)) { throw new Exception("Attempted to create database but failed."); }
        PopulateDemoDatabase(conn, data);

        return true;
    }

    private static void Main(string[] args)
    {
        Emitter.WriteLine("================================");
        Emitter.WriteLine("Template Demo Import/Export Data");
        Emitter.WriteLine("================================");

        bool doTests = true;
        if (doTests)
        {
            if (!RunTests()) { Emitter.WriteLine("\n\nTests Failed: Quitting."); return; }
            else { Emitter.WriteLine("\n\nTests Passed: Continue with Demo."); }
        }

        // Let's make sure there a database and that it's
        // fresh before creating a connection to it from main()
        if (File.Exists(Config.DBFilename))
        {
            File.Delete(Config.DBFilename);
        }

        if (!CreateDemoDatabase(Config.DBSchemaFilename, Config.DBFilename, Config.DBInitData))
        {
            Emitter.WriteLine("Could not create database: Quitting.");
            return;
        }

        using var conn = new SqliteConnection(Config.ConnStr);
        conn.Open();

        if (false)
        {
            Emitter.WriteLine("================================");
            Emitter.WriteLine("Print contacts");
            Emitter.WriteLine("================================");
            PrintContactsTable(conn);

            Emitter.WriteLine("================================");
            Emitter.WriteLine("Print addresses");
            Emitter.WriteLine("================================");
            PrintAddressesTable(conn);

            Emitter.WriteLine("================================");
            Emitter.WriteLine("Print phones");
            Emitter.WriteLine("================================");
            PrintPhonesTable(conn);

            Emitter.WriteLine("================================");
            Emitter.WriteLine("Entire Database");
            Emitter.WriteLine("================================");
            PrintDatabase(conn);

        }

        Emitter.WriteLine("================================");
        Emitter.WriteLine("List of Tables");
        Emitter.WriteLine("================================");
        QueryAndPrint(conn, "SELECT name FROM sqlite_schema WHERE type='table';");

        TemplateEngine eagle = new TemplateEngine();
        string templateFilePath = "templates/create_sqlimport.tpl";
        string templateStr = "";
        if (File.Exists(templateFilePath))
        {
            using (StreamReader reader = new StreamReader(templateFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    templateStr += line + '\n';
                }
            }
        }
        else
        {
            throw new FileNotFoundException($"file not found [{templateFilePath}]");
        }

        var context = new Dictionary<string, string>();
        eagle.AddAction("tables", new TablesAction());
        string parsed = eagle.Parse(templateStr, context);
        Emitter.WriteLine(parsed);

    }
}

    public class FlatData
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
        public string? Phone { get; set; }

    }

public class TablesAction: ITemplateAction
{
    public string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
    {
        using var conn = new SqliteConnection(Config.ConnStr);
        conn.Open();
        string sql = "SELECT name FROM sqlite_schema WHERE type='table';";
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();
        string retval = "";
        TemplateEngine eagle = new TemplateEngine();
        eagle.AddAction("tableCreate", new TableCreateAction());
        eagle.AddAction("data", new DataAction());
        
        while (reader.Read())
        {
            string tableName = reader.GetString(0);
            if (tableName == "sqlite_sequence") { continue; }
            var newContext = new Dictionary<string, string>();
            newContext.Add("table_name", tableName);
            retval += eagle.ParseTokens(tokens, newContext);
            tokens.Rewind();
        }
        return retval;
    }
}

public class DataAction : ITemplateAction
{
    public string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
    {
        using var conn = new SqliteConnection(Config.ConnStr);
        conn.Open();

        string tableName = context["table_name"];
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT * FROM '{tableName}';";
        using var reader = cmd.ExecuteReader();

        string retval = "";
        while (reader.Read())
        {
            var rowContext = new Dictionary<string, string>(context)
            {
                ["fields"] = BuildFields(reader),
                ["values"] = BuildValues(reader)
            };

            TemplateEngine eagle = new TemplateEngine();
            retval += eagle.ParseTokens(tokens, rowContext);
            tokens.Rewind();
        }

        return retval;
    }

    private static string BuildFields(DbDataReader reader)
    {
        List<string> fields = new List<string>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            fields.Add(reader.GetName(i));
        }
        return string.Join(", ", fields);
    }

    private static string BuildValues(DbDataReader reader)
    {
        List<string> values = new List<string>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            values.Add(FormatSqlLiteral(reader.GetValue(i)));
        }
        return string.Join(", ", values);
    }

    private static string FormatSqlLiteral(object value)
    {
        if (value == DBNull.Value)
        {
            return "NULL";
        }

        return value switch
        {
            string txt => $"'{txt.Replace("'", "''")}'",
            char ch => $"'{ch.ToString().Replace("'", "''")}'",
            bool flag => flag ? "1" : "0",
            byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal
                => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "NULL",
            _ => $"'{value.ToString()?.Replace("'", "''")}'"
        };
    }
}

public class TableCreateAction : ITemplateAction
{
    public string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
    {
        using var conn = new SqliteConnection(Config.ConnStr);
        conn.Open();
        string sql = $"SELECT sql FROM sqlite_schema WHERE name = @table_name;";
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("table_name", context["table_name"]);
        using var reader = cmd.ExecuteReader();
        string retval = "";
        if (reader.Read()) 
        {
            retval = reader.GetString(0); 
        }
        return retval;
    }
}
