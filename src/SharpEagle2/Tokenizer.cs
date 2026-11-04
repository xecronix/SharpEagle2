using System;
using System.Collections.Generic;
using XecronixCursor;

namespace SharpEagle2
{
    public class Tokenizer
    {
        private readonly List<Token> _stack = new List<Token>();
        private readonly List<Token> _tokens = new List<Token>();

        private void Reset()
        {
            _stack.Clear();
            _tokens.Clear();
        }

        private void PushStack(Token tkn)
        {
            _stack.Add(tkn);
        }

        private Token? PopStack()
        {
            Token? retval = null;
            if (_stack.Count > 0)
            {
                int index = _stack.Count - 1;
                retval = _stack[index];
                _stack.RemoveAt(index);
            }
            return retval;
        }

        private static List<char> Snapshot(Cursor<char> template)
        {
            List<char> chars = new List<char>();
            if (template.Count == 0)
            {
                return chars;
            }

            chars.Add(template.Current);
            while (template.Next())
            {
                chars.Add(template.Current);
            }
            return chars;
        }

        private static void BuildPositions(
            IReadOnlyList<char> chars,
            out int[] lineAt,
            out int[] colAt)
        {
            lineAt = new int[chars.Count];
            colAt = new int[chars.Count];

            int line = 0;
            line = 1;
            int col = 0;
            for (int i = 0; i < chars.Count; i++)
            {
                lineAt[i] = line;
                colAt[i] = col;

                char c = chars[i];
                if (c == '\n')
                {
                    line++;
                    col = 0;
                }
                else if (c == '\r')
                {
                    col = 0;
                }
                else
                {
                    col++;
                }
            }
        }

        private static void SkipWhiteSpace(IReadOnlyList<char> chars, ref int index)
        {
            while (index < chars.Count && char.IsWhiteSpace(chars[index]))
            {
                index++;
            }
        }

        private Token MakeKeyToken(IReadOnlyList<char> chars, int[] lineAt, int[] colAt, ref int index)
        {
            SkipWhiteSpace(chars, ref index);

            int line = index < chars.Count ? lineAt[index] : 0;
            int col = index < chars.Count ? colAt[index] : 0;
            Token retval = new Token(TokenType.Key, "", line, col);

            while (index < chars.Count)
            {
                char c = chars[index];
                if (char.IsWhiteSpace(c) || c == ':')
                {
                    break;
                }

                retval.Str += c;
                index++;
            }

            return retval;
        }

        private void TagClose(int line, int col)
        {
            Token tkn = new Token(TokenType.CloseAction, ":}", line, col);
            _tokens.Add(tkn);
            Token? popTkn = PopStack();
            if (popTkn == null)
            {
                throw new Exception($"Invalid Closing action tag found without an open: line:[{line}] column:[{col}]");
            }
        }

        private void ParseTokens(IReadOnlyList<char> chars, int[] lineAt, int[] colAt, ref int index)
        {
            Token? textToken = null;

            while (index < chars.Count)
            {
                char c = chars[index];
                bool didPeek = index + 1 < chars.Count;
                char p = didPeek ? chars[index + 1] : '\0';

                if (didPeek && c == '{' && p == '=')
                {
                    if (textToken != null && textToken.Str.Length > 0)
                    {
                        _tokens.Add(textToken);
                    }

                    _tokens.Add(new Token(TokenType.OpenSubstitution, "{=", lineAt[index], colAt[index]));
                    index += 2;
                    _tokens.Add(MakeKeyToken(chars, lineAt, colAt, ref index));
                    SkipWhiteSpace(chars, ref index);

                    if (index + 1 < chars.Count && chars[index] == ':' && chars[index + 1] == '}')
                    {
                        _tokens.Add(new Token(TokenType.CloseSubstitution, ":}", lineAt[index], colAt[index]));
                        index += 2;
                    }
                    else
                    {
                        int line = index < chars.Count ? lineAt[index] : lineAt[chars.Count - 1];
                        int col = index < chars.Count ? colAt[index] : colAt[chars.Count - 1];
                        throw new Exception($"Invalid Substitution tag structure: line:[{line}] column:[{col}]");
                    }

                    textToken = null;
                }
                else if (didPeek && c == '{' && p == '@')
                {
                    if (textToken != null && textToken.Str.Length > 0)
                    {
                        _tokens.Add(textToken);
                    }

                    Token open = new Token(TokenType.OpenAction, "{@", lineAt[index], colAt[index]);
                    PushStack(open);
                    _tokens.Add(open);
                    index += 2;
                    _tokens.Add(MakeKeyToken(chars, lineAt, colAt, ref index));
                    textToken = null;

                    ParseTokens(chars, lineAt, colAt, ref index);
                }
                else if (didPeek && c == ':' && p == '}')
                {
                    if (textToken != null && textToken.Str.Length > 0)
                    {
                        _tokens.Add(textToken);
                    }

                    TagClose(lineAt[index], colAt[index]);
                    index += 2;
                    return;
                }
                else
                {
                    if (textToken == null)
                    {
                        textToken = new Token(TokenType.Text, "", lineAt[index], colAt[index]);
                    }

                    textToken.Str += c;
                    index++;
                }
            }

            if (textToken != null && textToken.Str.Length > 0)
            {
                _tokens.Add(textToken);
            }
        }

        public List<Token> MakeTokens(Cursor<char> template)
        {
            Reset();

            List<char> chars = Snapshot(template);
            if (chars.Count == 0)
            {
                return new List<Token>();
            }

            BuildPositions(chars, out int[] lineAt, out int[] colAt);

            int index = 0;
            ParseTokens(chars, lineAt, colAt, ref index);

            if (_stack.Count > 0)
            {
                Token tkn = _stack[0];
                throw new Exception($"Action tag not closed: line:[{tkn.LineNumber}] column:[{tkn.ColumnNumber}]");
            }

            return new List<Token>(_tokens);
        }
    }
}
