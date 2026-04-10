using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SharpEagle2;
using XecronixCursor;


namespace BzTests
{
    internal class TokenizerTest
    {
        private static List<Token> MakeTokens(string template)
        {
            Tokenizer tokenizer = new Tokenizer();
            Cursor<char> csr = new Cursor<char>(template.ToCharArray());
            List<Token> tokens = tokenizer.MakeTokens(csr);
            return tokens;
        }

        private static bool ExpectTokenCount(List<Token> tokens, int expected)
        {
            if (tokens.Count != expected)
            {
                BzEmitter.WriteLine($"tokens.Count != {expected} : actual [{tokens.Count}]");
                return false;
            }
            return true;
        }

        private static bool ExpectToken(List<Token> tokens, int index, TokenType expectedType, string expectedStr)
        {
            if (index < 0 || index >= tokens.Count)
            {
                BzEmitter.WriteLine($"Token index out of range: [{index}]");
                return false;
            }

            if (tokens[index].Type != expectedType)
            {
                BzEmitter.WriteLine($"tokens[{index}].Type != {expectedType} : Actual [{tokens[index].Type}]");
                return false;
            }

            if (tokens[index].Str != expectedStr)
            {
                BzEmitter.WriteLine($"tokens[{index}].Str != [{expectedStr}] : Actual [{tokens[index].Str}]");
                return false;
            }

            return true;
        }

        private static bool ExpectThrows(string template)
        {
            try
            {
                MakeTokens(template);
                BzEmitter.WriteLine("Expected exception was not thrown.");
                return false;
            }
            catch
            {
                return true;
            }
        }

        private static bool Test_SingleToken()
        {
            //string template = "This is a template.";
            string template = "abc";
            List<Token> tokens = MakeTokens(template);
            if (tokens.Count != 1) { return false; }
            if (tokens[0].Str != template) { return false; }
            if (tokens[0].Type != TokenType.Text) { return false; }

            return true;
        }

        private static bool Test_SingleSubstitutionTag()
        {
            //string template = "This is a template.";
            string template = "abc {=name:}";
            List<Token> tokens = MakeTokens(template);
            if (tokens.Count == 4)
            {
                if (tokens[1].Type != TokenType.OpenSubstitution)
                {
                    BzEmitter.WriteLine($"Token.Type != {TokenType.OpenSubstitution} : Actual [{tokens[1].Type}]");
                    return false;
                }

                if (tokens[2].Type != TokenType.Key)
                {
                    BzEmitter.WriteLine($"Token.Type != {TokenType.Key} : Actual [{tokens[2].Type}]");
                    return false;
                }

                if (tokens[2].Str != "name")
                {
                    BzEmitter.WriteLine($"Token.Str != name");
                    return false;
                }

                if (tokens[3].Type != TokenType.CloseSubstitution)
                {
                    BzEmitter.WriteLine($"Token.Type != {TokenType.CloseSubstitution} : Actual [{tokens[3].Type}]");
                    return false;
                }
                return true;
            }
            else
            {
                BzEmitter.WriteLine($"tokens.Count != 4 : actual [{tokens.Count}]");
            }

            return false;
        }

        private static bool Test_SingleActionTag()
        {
            //string template = "This is a template.";
            string template = "def {@items:}";
            List<Token> tokens = MakeTokens(template);

            if (tokens.Count != 4)
            {
                BzEmitter.WriteLine($"tokens.Count != 4 : actual [{tokens.Count}]");
                return false;
            }

            if (tokens[1].Type != TokenType.OpenAction)
            {
                BzEmitter.WriteLine($"Token.Type != {TokenType.OpenAction} : Actual [{tokens[1].Type}]");
                return false;
            }

            if (tokens[2].Type != TokenType.Key)
            {
                BzEmitter.WriteLine($"Token.Type != {TokenType.Key} : Actual [{tokens[2].Type}]");
                return false;
            }

            if (tokens[2].Str != "items")
            {
                BzEmitter.WriteLine($"Token.Str != items");
                return false;
            }

            if (tokens[3].Type != TokenType.CloseAction)
            {
                BzEmitter.WriteLine($"Token.Type != {TokenType.CloseAction} : Actual [{tokens[3].Type}]");
                return false;
            }

            return true;
        }

        private static bool Test_MultiActionTag()
        {
            string template = "Month {@week day[{@day:}]:}";
            List<Token> tokens = MakeTokens(template);

            if (tokens.Count != 9)
            {
                BzEmitter.WriteLine($"tokens.Count != 9 : actual [{tokens.Count}]");
                return false;
            }

            if (tokens[0].Type != TokenType.Text)
            {
                BzEmitter.WriteLine($"tokens[0].Type != {TokenType.Text} : Actual [{tokens[0].Type}]");
                return false;
            }

            if (tokens[0].Str != "Month ")
            {
                BzEmitter.WriteLine($"tokens[0].Str != Month : Actual [{tokens[0].Str}]");
                return false;
            }

            if (tokens[1].Type != TokenType.OpenAction)
            {
                BzEmitter.WriteLine($"tokens[1].Type != {TokenType.OpenAction} : Actual [{tokens[1].Type}]");
                return false;
            }

            if (tokens[2].Type != TokenType.Key)
            {
                BzEmitter.WriteLine($"tokens[2].Type != {TokenType.Key} : Actual [{tokens[2].Type}]");
                return false;
            }

            if (tokens[2].Str != "week")
            {
                BzEmitter.WriteLine($"tokens[2].Str != week : Actual [{tokens[2].Str}]");
                return false;
            }

            if (tokens[3].Type != TokenType.Text)
            {
                BzEmitter.WriteLine($"tokens[3].Type != {TokenType.Text} : Actual [{tokens[3].Type}]");
                return false;
            }

            if (tokens[3].Str != " day[")
            {
                BzEmitter.WriteLine($"tokens[3].Str != day[ : Actual [{tokens[3].Str}]");
                return false;
            }

            if (tokens[4].Type != TokenType.OpenAction)
            {
                BzEmitter.WriteLine($"tokens[4].Type != {TokenType.OpenAction} : Actual [{tokens[4].Type}]");
                return false;
            }

            if (tokens[5].Type != TokenType.Key)
            {
                BzEmitter.WriteLine($"tokens[5].Type != {TokenType.Key} : Actual [{tokens[5].Type}]");
                return false;
            }

            if (tokens[5].Str != "day")
            {
                BzEmitter.WriteLine($"tokens[5].Str != day : Actual [{tokens[5].Str}]");
                return false;
            }

            if (tokens[6].Type != TokenType.CloseAction)
            {
                BzEmitter.WriteLine($"tokens[6].Type != {TokenType.CloseAction} : Actual [{tokens[6].Type}]");
                return false;
            }

            if (tokens[7].Type != TokenType.Text)
            {
                BzEmitter.WriteLine($"tokens[7].Type != {TokenType.Text} : Actual [{tokens[7].Type}]");
                return false;
            }

            if (tokens[7].Str != "]")
            {
                BzEmitter.WriteLine($"tokens[7].Str != ] : Actual [{tokens[7].Str}]");
                return false;
            }

            if (tokens[8].Type != TokenType.CloseAction)
            {
                BzEmitter.WriteLine($"tokens[8].Type != {TokenType.CloseAction} : Actual [{tokens[8].Type}]");
                return false;
            }

            return true;
        }

        private static bool Test_ActionAndSubstitutionTag()
        {
            string template = "Books {@chapter page[{=pg:}]:}";
            List<Token> tokens = MakeTokens(template);

            if (tokens.Count != 9)
            {
                BzEmitter.WriteLine($"tokens.Count != 9 : actual [{tokens.Count}]");
                return false;
            }

            if (tokens[0].Type != TokenType.Text)
            {
                BzEmitter.WriteLine($"tokens[0].Type != {TokenType.Text} : Actual [{tokens[0].Type}]");
                return false;
            }

            if (tokens[0].Str != "Books ")
            {
                BzEmitter.WriteLine($"tokens[0].Str != \"Books \" : Actual [{tokens[0].Str}]");
                return false;
            }

            if (tokens[1].Type != TokenType.OpenAction)
            {
                BzEmitter.WriteLine($"tokens[1].Type != {TokenType.OpenAction} : Actual [{tokens[1].Type}]");
                return false;
            }

            if (tokens[2].Type != TokenType.Key)
            {
                BzEmitter.WriteLine($"tokens[2].Type != {TokenType.Key} : Actual [{tokens[2].Type}]");
                return false;
            }

            if (tokens[2].Str != "chapter")
            {
                BzEmitter.WriteLine($"tokens[2].Str != chapter : Actual [{tokens[2].Str}]");
                return false;
            }

            if (tokens[3].Type != TokenType.Text)
            {
                BzEmitter.WriteLine($"tokens[3].Type != {TokenType.Text} : Actual [{tokens[3].Type}]");
                return false;
            }

            if (tokens[3].Str != " page[")
            {
                BzEmitter.WriteLine($"tokens[3].Str != \" page[\" : Actual [{tokens[3].Str}]");
                return false;
            }

            if (tokens[4].Type != TokenType.OpenSubstitution)
            {
                BzEmitter.WriteLine($"tokens[4].Type != {TokenType.OpenSubstitution} : Actual [{tokens[4].Type}]");
                return false;
            }

            if (tokens[5].Type != TokenType.Key)
            {
                BzEmitter.WriteLine($"tokens[5].Type != {TokenType.Key} : Actual [{tokens[5].Type}]");
                return false;
            }

            if (tokens[5].Str != "pg")
            {
                BzEmitter.WriteLine($"tokens[5].Str != pg : Actual [{tokens[5].Str}]");
                return false;
            }

            if (tokens[6].Type != TokenType.CloseSubstitution)
            {
                BzEmitter.WriteLine($"tokens[6].Type != {TokenType.CloseSubstitution} : Actual [{tokens[6].Type}]");
                return false;
            }

            if (tokens[7].Type != TokenType.Text)
            {
                BzEmitter.WriteLine($"tokens[7].Type != {TokenType.Text} : Actual [{tokens[7].Type}]");
                return false;
            }

            if (tokens[7].Str != "]")
            {
                BzEmitter.WriteLine($"tokens[7].Str != \"]\" : Actual [{tokens[7].Str}]");
                return false;
            }

            if (tokens[8].Type != TokenType.CloseAction)
            {
                BzEmitter.WriteLine($"tokens[8].Type != {TokenType.CloseAction} : Actual [{tokens[8].Type}]");
                return false;
            }

            return true;
        }
    
        private static bool Test_LeadingSubstitutionTag()
        {
            string template = "{=name:} rocks";
            List<Token> tokens = MakeTokens(template);

            if (!ExpectTokenCount(tokens, 4)) { return false; }
            if (!ExpectToken(tokens, 0, TokenType.OpenSubstitution, "{=")) { return false; }
            if (!ExpectToken(tokens, 1, TokenType.Key, "name")) { return false; }
            if (!ExpectToken(tokens, 2, TokenType.CloseSubstitution, ":}")) { return false; }
            if (!ExpectToken(tokens, 3, TokenType.Text, " rocks")) { return false; }

            return true;
        }

        private static bool Test_TrailingSubstitutionTag()
        {
            string template = "Hi {=name:}";
            List<Token> tokens = MakeTokens(template);

            if (!ExpectTokenCount(tokens, 4)) { return false; }
            if (!ExpectToken(tokens, 0, TokenType.Text, "Hi ")) { return false; }
            if (!ExpectToken(tokens, 1, TokenType.OpenSubstitution, "{=")) { return false; }
            if (!ExpectToken(tokens, 2, TokenType.Key, "name")) { return false; }
            if (!ExpectToken(tokens, 3, TokenType.CloseSubstitution, ":}")) { return false; }

            return true;
        }

        private static bool Test_AdjacentSubstitutionTags()
        {
            string template = "{=a:}{=b:}";
            List<Token> tokens = MakeTokens(template);

            if (!ExpectTokenCount(tokens, 6)) { return false; }
            if (!ExpectToken(tokens, 0, TokenType.OpenSubstitution, "{=")) { return false; }
            if (!ExpectToken(tokens, 1, TokenType.Key, "a")) { return false; }
            if (!ExpectToken(tokens, 2, TokenType.CloseSubstitution, ":}")) { return false; }
            if (!ExpectToken(tokens, 3, TokenType.OpenSubstitution, "{=")) { return false; }
            if (!ExpectToken(tokens, 4, TokenType.Key, "b")) { return false; }
            if (!ExpectToken(tokens, 5, TokenType.CloseSubstitution, ":}")) { return false; }

            return true;
        }

        private static bool Test_MultipleSiblingSubstitutionTags()
        {
            string template = "A {=x:} B {=y:} C";
            List<Token> tokens = MakeTokens(template);

            if (!ExpectTokenCount(tokens, 9)) { return false; }
            if (!ExpectToken(tokens, 0, TokenType.Text, "A ")) { return false; }
            if (!ExpectToken(tokens, 1, TokenType.OpenSubstitution, "{=")) { return false; }
            if (!ExpectToken(tokens, 2, TokenType.Key, "x")) { return false; }
            if (!ExpectToken(tokens, 3, TokenType.CloseSubstitution, ":}")) { return false; }
            if (!ExpectToken(tokens, 4, TokenType.Text, " B ")) { return false; }
            if (!ExpectToken(tokens, 5, TokenType.OpenSubstitution, "{=")) { return false; }
            if (!ExpectToken(tokens, 6, TokenType.Key, "y")) { return false; }
            if (!ExpectToken(tokens, 7, TokenType.CloseSubstitution, ":}")) { return false; }
            if (!ExpectToken(tokens, 8, TokenType.Text, " C")) { return false; }

            return true;
        }

        private static bool Test_ActionWithTextBody()
        {
            string template = "List {@items value:}";
            List<Token> tokens = MakeTokens(template);

            if (!ExpectTokenCount(tokens, 5)) { return false; }
            if (!ExpectToken(tokens, 0, TokenType.Text, "List ")) { return false; }
            if (!ExpectToken(tokens, 1, TokenType.OpenAction, "{@")) { return false; }
            if (!ExpectToken(tokens, 2, TokenType.Key, "items")) { return false; }
            if (!ExpectToken(tokens, 3, TokenType.Text, " value")) { return false; }
            if (!ExpectToken(tokens, 4, TokenType.CloseAction, ":}")) { return false; }

            return true;
        }

        private static bool Test_WhitespaceInsideSubstitutionTag()
        {
            string template = "X {=   name   :}";
            List<Token> tokens = MakeTokens(template);

            if (!ExpectTokenCount(tokens, 4)) { return false; }
            if (!ExpectToken(tokens, 0, TokenType.Text, "X ")) { return false; }
            if (!ExpectToken(tokens, 1, TokenType.OpenSubstitution, "{=")) { return false; }
            if (!ExpectToken(tokens, 2, TokenType.Key, "name")) { return false; }
            if (!ExpectToken(tokens, 3, TokenType.CloseSubstitution, ":}")) { return false; }

            return true;
        }

        private static bool Test_WhitespaceInsideActionWithNestedSubstitution()
        {
            string template = "Books {@chapter   page[{=  pg   :}]   :}";
            List<Token> tokens = MakeTokens(template);

            if (!ExpectTokenCount(tokens, 9)) { return false; }
            if (!ExpectToken(tokens, 0, TokenType.Text, "Books ")) { return false; }
            if (!ExpectToken(tokens, 1, TokenType.OpenAction, "{@")) { return false; }
            if (!ExpectToken(tokens, 2, TokenType.Key, "chapter")) { return false; }
            if (!ExpectToken(tokens, 3, TokenType.Text, "   page[")) { return false; }
            if (!ExpectToken(tokens, 4, TokenType.OpenSubstitution, "{=")) { return false; }
            if (!ExpectToken(tokens, 5, TokenType.Key, "pg")) { return false; }
            if (!ExpectToken(tokens, 6, TokenType.CloseSubstitution, ":}")) { return false; }
            if (!ExpectToken(tokens, 7, TokenType.Text, "]   ")) { return false; }
            if (!ExpectToken(tokens, 8, TokenType.CloseAction, ":}")) { return false; }

            return true;
        }

        private static bool Test_StrayCloseTag_Throws()
        {
            string template = "abc :}";
            return ExpectThrows(template);
        }

        private static bool Test_UnclosedActionTag_Throws()
        {
            string template = "abc {@items value";
            return ExpectThrows(template);
        }

        private static bool Test_UnclosedSubstitutionTag_Throws()
        {
            string template = "abc {=name";
            return ExpectThrows(template);
        }

        private static bool Test_IncompleteActionOpenAtEnd_Throws()
        {
            string template = "abc {@";
            return ExpectThrows(template);
        }

        private static bool Test_IncompleteSubstitutionOpenAtEnd_Throws()
        {
            string template = "abc {=";
            return ExpectThrows(template);
        }
    } 
}