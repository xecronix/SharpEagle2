using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Demo_SQLiteExporter
{
    internal class Config
    {
        public const string DBFilename = "demo.db";
        public const string DBInitData = "data/flat_import.dat";
        public const string DBSchemaFilename = "data/schema.sql";
        public const string ConnStr = $"Data Source = {DBFilename}";
    }
}
