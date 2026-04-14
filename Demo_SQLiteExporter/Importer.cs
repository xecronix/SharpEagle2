using System;
using System.Collections.Generic;
using System.Text;

namespace Demo_SQLiteExporter
{

    public enum ParseState 
    {
        Searching,
        Reading
    }

    internal class Importer
    {
        private bool ReadDataFile(string filePath, out List<string> retval)
        {
            retval = new List<string>();
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        retval.Add(line);
                    }
                }
                return true;
            }
            else
            {
                throw new FileNotFoundException($"file not found [{filePath}]");
            }
            return false;
        }

        private bool ParseImportRow(string row, out FlatData retval)
        {
            retval = new FlatData();

            ParseState state = ParseState.Searching;
            string value = "";
            int fldFirstName = 0;
            int fldLastName = 1;
            int fldStreet = 2;
            int fldCity = 3;
            int fldZip = 4;
            int fldPhone = 5;
            int lastCol = fldPhone;
            int currentFld = fldFirstName;
            
            for (int i = 0; i < row.Length; i++)
            {
                char c = row[i];   
                if (state == ParseState.Searching)
                {
                    if (c == '|') 
                    { 
                        state = ParseState.Reading;
                        value = "";
                    }
                }
                else if (state == ParseState.Reading)
                {
                    if (c == '|') 
                    { 
                        state = ParseState.Searching;
                        if (currentFld == fldFirstName) { retval.FirstName = value; }
                        else if (currentFld == fldLastName) { retval.LastName = value; }
                        else if (currentFld == fldStreet) { retval.Street = value; }
                        else if (currentFld == fldCity) { retval.City = value; }
                        else if (currentFld == fldZip) { retval.Zip = value; }
                        else if (currentFld == fldPhone) { retval.Phone = value; }
                        else { throw new Exception($"Unknow Column line: [{i}] character: [{i}] Field Num[{currentFld}]"); }

                        currentFld++;
                        if (currentFld > lastCol) 
                        {
                            if (i != row.Length - 1)
                            {
                                throw new Exception("Too many columns in data.");
                            }
                            currentFld = fldFirstName;

                        }
                    }
                    else
                    {
                        value += c;   
                    }
                }
            }
            return true;
        }

        public bool Import(List<string> data, out List<FlatData> retval, bool skipHeader = true) 
        {
            retval = new List<FlatData>();
            int numDataRow = data.Count;
            for (int row = 0; row < data.Count; row++) 
            {
                if (row == 0 && skipHeader) { numDataRow--; continue; } // skip the first row it's the header
                if (ParseImportRow(data[row].Trim(), out FlatData f)) 
                {
                    retval.Add(f);
                }
            }
            return numDataRow == retval.Count;
        }

        public bool Import(string filePath, out List<FlatData> retval)
        {
            retval = default!;
            List<string> data;
            if (ReadDataFile(filePath, out data))
            {
                return Import(data, out retval);
            }
            return false;
        }
    }
}
