// File: TemplateEngine.cs
using System.Runtime.InteropServices;
using XecronixCursor;
namespace SharpEagle2;

public sealed class TemplateEngine
{

    private readonly Dictionary<string, ITemplateAction> TemplateActions = new Dictionary<string, ITemplateAction>();

    public void AddAction(string key, ITemplateAction action) { TemplateActions.Add(key, action); }

    private static bool SubstitutionTag(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context, out string retval)
    {
        Token c = tokens.Current;
        Token? p = null;
        bool didPeek = tokens.TryPeek(out p);
        bool success = false;
        retval = "";

        if (didPeek && p.Type == TokenType.Key && p.Str != null && p.Str.Length > 0)
        {
            if (context.ContainsKey(p.Str))
            {
                retval += context[p.Str];
                tokens.Next();     // move to the key token
                if (tokens.Next()) // move to the close token
                {
                    c = tokens.Current;
                    if (c.Type != TokenType.CloseSubstitution)
                    {
                        throw new Exception($"Expected closing substitution token but got [{c.Type}]: Line: [{c.LineNumber}] Column [{c.ColumnNumber}]");
                    }
                }
            }
            else
            {   // TODO: add error checking and msg here.
                retval += c.Str;
                retval += p.Str;
                tokens.Next(); // move to the token after the opening tag
                if (tokens.Next()) // this should be the closing token
                {
                    c = tokens.Current;
                    retval += c.Str;
                }
            }

            success = true;
        }
        else
        {
            throw new Exception($"Tokens did not meet expectations. Line: [{c.LineNumber}] Column [{c.ColumnNumber}]");
        }
        return success;
    }

    private static bool MakeSubTemplate(Cursor<Token> tokens, out Cursor<Token> retval)
    {
        retval = default!;
        List<Token> subTokens = new List<Token>();
        int nestLevel = 1;
        while (nestLevel > 0)
        {
            Token c = tokens.Current;
            if (c.Type == TokenType.OpenAction)
            {
                nestLevel++;
            }
            else if (c.Type == TokenType.CloseAction)
            {
                nestLevel--;
            }
            if (nestLevel == 0) { break; } // break and don't move the cursor.  don't add the close tag to tokens
            subTokens.Add(c);
            if (!tokens.Next()) { break; }
        }

        if (subTokens.Count > 0)
        {
            retval = new Cursor<Token>(subTokens);
        }

        return subTokens.Count > 0;
    }

    private bool ActionTag(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context, out string retval)
    {
        retval = "";
        bool success = false;
        Token c = tokens.Current;
        Token? p = null;
        bool didPeek = tokens.TryPeek(out p);

        // let's first make sure the cursor is sane
        if (c.Type != TokenType.OpenAction) { throw new Exception($"Wrong token type for open token [{c.Type}]. Line: [{c.LineNumber}] Column [{c.ColumnNumber}]"); }
        if (!didPeek) { throw new Exception($"Ran out of looking for action key. Line: [{p.LineNumber}] Column [{p.ColumnNumber}]"); }
        if (p.Type != TokenType.Key) { throw new Exception($"Wrong token type for key token [{p.Type}]. Line: [{p.LineNumber}] Column [{p.ColumnNumber}]"); }

        tokens.Next();
        if (!tokens.Next())
        {
             throw new Exception($"Ran out of tokens before building subtemplate. Line: [{p.LineNumber}] Column [{p.ColumnNumber}]");
        }

        bool actionExists = TemplateActions.ContainsKey(p.Str);
        Cursor<Token> subTemplate;
        bool hasSubtemplate = MakeSubTemplate(tokens, out subTemplate);

        // action tag does not exist in the TemplateActions dictionary render the subtemplate and the tags. (should be close to the original template)
        if (!actionExists)
        {
            // render the open tag to retval
            retval += c.Str;
            retval += p.Str;
            // render the all the subtemplate token.Str to retval.
            while (hasSubtemplate)
            {
                retval+= subTemplate.Current.Str;
                if (!subTemplate.Next()) { break; }
            }

            // render the closing tag to retval
            retval += tokens.Current.Str;
            // set success to true
            success = true;
        }

        // action tag does exist in the TemplateActions dictionary 
        else
        {
            // call the action
            ITemplateAction action = TemplateActions[p.Str];
            // set retval to the result of the action
            retval += action.Run(subTemplate, context) ?? "";
            // consume the closing tag
            tokens.Next();
            // set success to true
            success = true;
        }

        return success;
    }

    public string ParseTokens(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
    {
        // ***  Remember the stream/cursor rules                                          *** //
        // ***  Call a function with the cursor on the first significant position         *** //
        // ***  Expect the caller to advance the cursor after the called function returns *** //


        ArgumentNullException.ThrowIfNull(tokens);
        ArgumentNullException.ThrowIfNull(context);

        if (tokens.Count == 0)
        {
            return string.Empty;
        }

        string retval="";

        do
        {
            Token c = tokens.Current;

            if (c.Type == TokenType.Text) { retval += c.Str; }
            else if (c.Type == TokenType.OpenSubstitution)
            {
                string txt = "";
                if (SubstitutionTag(tokens, context, out txt))
                {
                    retval += txt;
                }
            }
            else if (c.Type == TokenType.OpenAction)
            {
                string txt = "";
                if (ActionTag(tokens, context, out txt))
                {
                    retval += txt;
                }
            }
        } while (tokens.Next());
        return retval;
    }
    public string Parse(string template, IReadOnlyDictionary<string, string> context)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(context);
        
        if (template.Length == 0)
        {
            return string.Empty;
        }

        Cursor<char> csr = new Cursor<char>(template.ToCharArray());
        Tokenizer t = new Tokenizer();
        List<Token> tkns = t.MakeTokens(csr);
        Cursor<Token> tokens = new Cursor<Token>(tkns);
        return ParseTokens(tokens, context);
    }
}