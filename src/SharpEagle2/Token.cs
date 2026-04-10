using System;
using System.Collections.Generic;
using System.Text;

namespace SharpEagle2
{
    public enum TokenType
    {
        Text,
        OpenSubstitution,
        CloseSubstitution,
        OpenAction,
        CloseAction,
        Key
    }

    public class Token
    {


        public string Str { get; set; }
        public TokenType Type { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }

        public Token(TokenType type, string str, int lineNumber, int columnNumber)
        { 
            Type = type;
            Str = str;  
            LineNumber = lineNumber; 
            ColumnNumber = columnNumber;
        }
    }
}
