using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using XecronixCursor;

namespace SharpEagle2
{
    public class Tokenizer
    {
        private readonly List<Token> _stack = new List<Token>();
        
        private int StackLen => _stack.Count;
        private readonly List<Token> _tokens = new List<Token>();
        private int _lineNum = 0;
        private int _colNum = 0;    

        private void PushStack(Token tkn)
        {
            _stack.Add(tkn); 
        }

        private Token? PopStack()
        {
            Token? retval = null;
            if (StackLen > 0)
            {
                int index = StackLen - 1;
                retval = _stack[index];
                _stack.RemoveAt(index);
            }
            return retval;
        }

        private void CharPositioner(char c)
        {
            if (c == '\r') { _colNum = 0; }
            else if (c == '\n') { _lineNum++; _colNum = 0; }
            else { _colNum++; }
        }

        private char Current(Cursor<char> template)
        { 
            char c = template.Current;
            CharPositioner(c);
            return c;
        }

        private void ConsumeWhiteSpace(Cursor<char> template)
        {
            do
            {
                char c = Current(template);
                if (!char.IsWhiteSpace(c))
                {
                    _colNum--;
                    break;
                }
            } while (template.Next());
        }

        private Token MakeKeyToken(Cursor<char> template) 
        {
            // consume leading whitespace
            ConsumeWhiteSpace(template);
            
            // create a token to return
            Token retval = new Token(TokenType.Key, "", _lineNum, _colNum);
            // build key until peek == : or peek == whitespace
            do
            {
                char c = Current(template);
                if (char.IsWhiteSpace(c) || c == ':') { break; }
                retval.Str += c;
            } while (template.Next());
            return retval;
        }

        public void ActionTagOpen(Cursor<char> template) 
        {
            Token tkn = new Token(TokenType.OpenAction, "{@", _lineNum, _colNum - 2);
            PushStack(tkn);
            _tokens.Add(tkn);
            _tokens.Add(MakeKeyToken(template));
            MakeTokens(template);
        }
        public void SubstitutionTagOpen(Cursor<char> template)
        {
            Token tkn = new Token(TokenType.OpenSubstitution, "{=", _lineNum, _colNum - 2);
            _tokens.Add(tkn);
            _tokens.Add(MakeKeyToken(template));
            ConsumeWhiteSpace(template);
            char p;
            bool didPeek = template.TryPeek(out p);
            if (didPeek && template.Current == ':' && p == '}')
            {
                Token closeTkn = new Token(TokenType.CloseSubstitution, ":}", _lineNum, _colNum);
                _tokens.Add(closeTkn);
                template.Next(); // move to the }
                Current(template);
            }
            else
            {
                string msg = $"Invalid Substitution tag structure: line:[{_lineNum}] column:[{_colNum}]";
                throw new Exception(msg);
            }
        }
        public void TagClose()
        {
            Token tkn = new Token(TokenType.CloseAction, ":}", _lineNum, _colNum);
            _tokens.Add(tkn);
            Token? popTkn = PopStack();
            if (popTkn == null)
            {
                string msg = $"Invalid Closing action tag found without an open: line:[{_lineNum}] column:[{_colNum}]";
                throw new Exception(msg);
            }
        }

        public List<Token> MakeTokens(Cursor<char> template) 
        {
            // ***  Remember the stream/cursor rules                                          *** //
            // ***  Call a function with the cursor on the first significant position         *** //
            // ***  Expect the caller to advance the cursor after the called function returns *** //

            // create Text token with empty string.  empty String is a sentinel value.
            Token txt = new Token(TokenType.Text, "", _lineNum, _colNum);

            // start looping over chars
            do
            {
                char c = Current(template);
                char p;
                bool didPeek = template.TryPeek(out p);
                // if current is { and peek is = we have a substitution tag opening
                if (didPeek && c == '{' && p == '=')
                {
                    if (txt.Str.Length > 0)
                    {
                        _tokens.Add(txt);
                    }

                    template.Next(); // move to the =
                    Current(template);
                    if (template.Next()) // consume the =
                    {
                        Current(template);
                        SubstitutionTagOpen(template);
                    }
                    else { throw new Exception($"Unexpected end of data found: line:[{_lineNum}] column:[{_colNum}]"); }
                    txt = new Token(TokenType.Text, "", _lineNum, _colNum);
                }

                // if current is { and peek is @ we have a action tag opening
                else if (didPeek && c == '{' && p == '@')
                {
                    if (txt.Str.Length > 0)
                    {
                        _tokens.Add(txt);
                    }


                    template.Next(); // move to the @
                    Current(template);
                    if (template.Next()) // consume the @
                    {
                        Current(template);
                        ActionTagOpen(template);
                    }
                    else { throw new Exception($"Unexpected end of data found: line:[{_lineNum}] column:[{_colNum}]"); }
                    txt = new Token(TokenType.Text, "", _lineNum, _colNum);
                }

                // if current is : and peek is } we have a closing tag
                else if (didPeek && c == ':' && p == '}')
                {
                    if (txt.Str.Length > 0)
                    {
                        _tokens.Add(txt);
                    }

                    TagClose();
                    template.Next();
                    Current(template);
                    txt = new Token(TokenType.Text, "", _lineNum, _colNum);

                }

                // else just some data add it to the text token.
                else { txt.Str += c; }
            } while (template.Next());

            if (StackLen > 0) 
            {
                Token tkn = _stack[0];
                throw new Exception($"Action tag not closed: line:[{tkn.LineNumber}] column:[{tkn.ColumnNumber}]");
            }
            if (txt.Str.Length > 0) { _tokens.Add(txt); }
            return _tokens;
        }
    }
}
