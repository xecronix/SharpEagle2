using System;
using System.Collections.Generic;
using System.Text;
using XecronixCursor;

namespace SharpEagle2
{
    public interface ITemplateAction
    {
        string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context);
    }
}
