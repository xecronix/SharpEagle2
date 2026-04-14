using System;
using System.Collections.Generic;
using System.Text;

namespace Demo_SQLiteExporter
{
    internal class ImporterTests
    {

        public static bool ImporterFromHardCodedList()
        {
            List<string> rows = new List<string>();
            List<FlatData> data;
            rows.Add("|Ronald|      |Weidner|  |999 Great Street| |Blueville|  |12345| |555-1212| ");
            rows.Add("|Peter|       |Disciple|  |777 Holy Street|  |Greenville| |12347| |555-1414| ");
            Importer imp = new Importer();
            string fun = "Test_ImporterFromHardCodedList";
            bool success = imp.Import(rows, out data, false);
            if (success)
            {
                if (data.Count != 2) { Emitter.WriteLine($"Failure {fun}: Expected 2 FlatData objects. Found : [{data.Count}]"); return false; }
                if (data[0].FirstName != "Ronald") { Emitter.WriteLine($"Failure{fun}: Expected value [Ronald] : Found [{data[0].FirstName}]"); return false; }
                if (data[0].Phone != "555-1212") { Emitter.WriteLine($"Failure{fun}: Expected value [555-1212] : Found [{data[0].Phone}]"); return false; }
                if (data[1].LastName != "Disciple") { Emitter.WriteLine($"Failure{fun}: Expected value [Disciple] : Found [{data[1].LastName}]"); return false; }
                if (data[1].City != "Greenville") { Emitter.WriteLine($"Failure{fun}: Expected value [Greenville] : Found [{data[1].City}]"); return false; }
                if (data[1].Street != "777 Holy Street") { Emitter.WriteLine($"Failure{fun}: Expected value [777 Holy Street] : Found [{data[1].Street}]"); return false; }
                if (data[1].Zip != "12347") { Emitter.WriteLine($"Failure{fun}: Expected value [12347] : Found [{data[1].Zip}]"); return false; }
            }
            else { Emitter.WriteLine($"Failure{fun}: Could not load data"); }
            
            Emitter.WriteLine($"{fun}: PASS"); 
            return success;
        }
        public static bool ImporterFromFilePath()
        {
            List<string> rows = new List<string>();
            List<FlatData> data;
            rows.Add("|Ronald|      |Weidner|  |999 Great Street| |Blueville|  |12345| |555-1212| ");
            rows.Add("|Peter|       |Disciple|  |777 Holy Street|  |Greenville| |12347| |555-1414| ");
            string fun = "Test_ImporterFromFilePath";
            Importer imp = new Importer();
            bool success = imp.Import("data/flat_import.dat", out data);
            if (success)
            {
                if (data.Count != 6) { Emitter.WriteLine($"Failure {fun}: Expected 2 FlatData objects. Found : [{data.Count}]"); return false; }
                if (data[1].Phone != "555-9999") { Emitter.WriteLine($"Failure {fun}: Expected value [555-9999] : Found [{data[1].Phone}]"); return false; }
            }
            else { Emitter.WriteLine($"Failure: Could not load data"); }

            Emitter.WriteLine($"{fun}: PASS");
            return success;
        }
    }
}
