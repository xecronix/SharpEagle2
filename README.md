# SharpEagle2

SharpEagle2 is a small C# template engine.

The goal is not to build the biggest template language in the world.
The goal is to build one that is simple, useful, and easier to debug
than the first version.

SharpEagle2 exists because the original SharpEagle idea still made
sense, but I wanted tokens this time.

Why?

Because tokens give me better structure, better nesting behavior, and
much better error messages. If something goes wrong, line and column
matter.

That was one of the main reasons for doing version 2.

## What it does

SharpEagle2 currently supports:

- plain text
- substitution tags
- action tags
- nested action tags
- custom callbacks
- token-based parsing with line/column tracking

It is small on purpose.

## Template syntax

### Substitution tags

A substitution tag looks like this:

```txt
{=name:}
````

If `"name"` exists in the context dictionary, it gets replaced.

Example:

```txt
Hi {=name:}.
```

with:

```csharp
["name"] = "Ronald"
```

becomes:

```txt
Hi Ronald.
```

If the key does not exist, the tag is left alone.

That is intentional.

I would rather preserve the original text than silently break it.

### Action tags

An action tag looks like this:

```txt
{@actionName ...subtemplate... :}
```

Example:

```txt
{@demographics
Name      {=Name:}
Country   {=Country:}
Christian {=Christian:}
:}
```

The action name is looked up in the action dictionary.  
If the action exists, SharpEagle2 gives it the subtemplate and lets it  
return whatever string it wants.

If the action does not exist, the original tag is preserved.

Again, that is intentional.

## Why version 2 exists

SharpEagle 1 worked, but it was string-oriented.

SharpEagle2 keeps the same spirit, but it moves the parser to tokens so  
I can do a better job with:

- nesting
    
- malformed input
    
- line/column error reporting
    
- parser sanity checks
    

That was the mission from the start.

## Core ideas

### 1. Plain text should stay plain text

If the input is just text, the engine should return it.

No surprises.

### 2. Missing data should fail gently

If a substitution key is missing, keep the original tag.

If an action is missing, keep the original action text.

That gives the template author a fighting chance to see what happened  
instead of wondering where the content went.

### 3. Actions return strings

A custom action gets a subtemplate cursor and a context dictionary.  
It returns a string.

That string might come from:

- reparsing the subtemplate
    
- building a new context
    
- looking up more data
    
- combining several passes
    
- ignoring the subtemplate entirely
    

That is up to the action.

The parser's job is to stay safe.  
The callback's job is to return text.

## Basic usage

### Simple substitution

```csharp
using SharpEagle2;
using System.Collections.Generic;

var engine = new TemplateEngine();

var context = new Dictionary<string, string>
{
    ["name"] = "Ronald"
};

string result = engine.Parse("Hi {=name:}.", context);

// result == "Hi Ronald."
```

### Missing substitution key

```csharp
var engine = new TemplateEngine();

var context = new Dictionary<string, string>
{
    ["notname"] = "Ronald"
};

string result = engine.Parse("Hi {=name:}.", context);

// result == "Hi {=name:}."
```

### Missing action

```csharp
var engine = new TemplateEngine();
var context = new Dictionary<string, string>();

string result = engine.Parse("{@days Phone rang  :}.", context);

// result == "{@days Phone rang  :}."
```

## Writing an action

Custom behavior is added through `ITemplateAction`.

Current shape:

```csharp
public interface ITemplateAction
{
    string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context);
}
```

Here is a simple example:

```csharp
using SharpEagle2;
using System.Collections.Generic;
using XecronixCursor;

public class DemographicsAction : ITemplateAction
{
    public string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
    {
        var eagle = new TemplateEngine();

        var newTags = new Dictionary<string, string>
        {
            ["Name"] = "Xecronix",
            ["Country"] = "Unknown",
            ["Christian"] = "Yes"
        };

        return eagle.ParseTokens(tokens, newTags);
    }
}
```

Then register it:

```csharp
var engine = new TemplateEngine();
engine.AddAction("demographics", new DemographicsAction());
```

## Nested actions

Nested actions are supported.

That matters because one action often needs to render a subtemplate that  
contains more actions inside it.

Example idea:

- outer action sets up some context
    
- inner action fills in more detail
    
- parser handles nesting correctly
    

That pattern is already part of the tested behavior.

## Looping actions

Looping works too.

One thing that came up naturally during development was the need to  
rewind the subtemplate cursor for repeated passes.

That is why `Cursor<T>` now has:

- `Rewind()`
    
- `FreshCopy()`
    

That turned out to matter for actions like:

- loop through days of the week
    
- render the same subtemplate multiple times
    
- keep parser code simple instead of fighting cursor state
    

## Context in real use

In real life I use context for 2 things:

1. substitution values
    
2. data needed to look up more data
    

That second use might sound a little hacky at first, but honestly it is  
pretty normal.

A lot of the time, the outer template already has the exact values I  
need to derive the inner context. So I do not see that as abuse of the  
system. I see it as practical.

## What this project is not trying to be

SharpEagle2 is not trying to compete with giant template engines.

It is not trying to become a kitchen sink.

It is trying to stay:

- readable
    
- small
    
- debuggable
    
- useful as a library
    
- flexible enough for callback-driven rendering
    

That is the lane.

## Error handling

This version is token-based for a reason.

Tokens carry line and column information, which means parser errors can  
be much more useful than "something went wrong somewhere."

That was one of the biggest reasons to build SharpEagle2 instead of just  
keeping the old version as-is.

## Current status

As of the current test-backed state, SharpEagle2 has working support for:

- empty template handling
    
- null argument guards
    
- simple substitutions
    
- missing substitutions
    
- missing actions
    
- nested action/subtemplate parsing
    
- tokenizer error cases
    
- callback-based rendering
    
- looping callbacks
    

That is enough to call it real.

## Example

```csharp
using SharpEagle2;
using System.Collections.Generic;
using XecronixCursor;

public class HelloAction : ITemplateAction
{
    public string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
    {
        var eagle = new TemplateEngine();

        var newTags = new Dictionary<string, string>
        {
            ["name"] = "Ronald"
        };

        return eagle.ParseTokens(tokens, newTags);
    }
}

var engine = new TemplateEngine();
engine.AddAction("hello", new HelloAction());

string template = @"{@hello Hello {=name:}! :}";
string result = engine.Parse(template, new Dictionary<string, string>());

// result == "Hello Ronald! "
```

## Project layout

```txt
SharpEagle2/
|- BzTests/
|- deps/
|- src/
|  \- SharpEagle2/
|     |- ITemplateAction.cs
|     |- SharpEagle2.csproj
|     |- TemplateEngine.cs
|     |- Token.cs
|     \- Tokenizer.cs
|- .gitignore
|- README.md
\- SharpEagle2.slnx
```

## Final note

SharpEagle2 is not trying to be fancy.

It is trying to be solid.

The whole point is to keep the template idea simple, keep the parser  
safe, and make failures easier to understand when they happen.

That is enough.

## License

MIT License

Copyright (c) 2026 Ronald Weidner

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.