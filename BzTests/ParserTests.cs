// File: ParserTests.cs

using SharpEagle2;
using System;
using System.Collections.Generic;
using XecronixCursor;

namespace BzTests
{
    internal class ParserTest
    {
        private static bool ExpectEqual(string expected, string actual, string label)
        {
            if (actual != expected)
            {
                BzEmitter.WriteLine($"{label} != [{expected}] : Actual [{actual}]");
                return false;
            }
            return true;
        }

        private static bool ExpectThrowsArgumentNull(Action action, string label)
        {
            try
            {
                action();
                BzEmitter.WriteLine($"Expected ArgumentNullException was not thrown: {label}");
                return false;
            }
            catch (ArgumentNullException)
            {
                return true;
            }
            catch (Exception ex)
            {
                BzEmitter.WriteLine($"Wrong exception type for {label}: [{ex.GetType().Name}]");
                return false;
            }
        }

        private static bool Test_Parse_ReturnsInputTemplate_ForCurrentSkeleton()
        {
            string template = "abc";
            var context = new Dictionary<string, string>();

            var engine = new TemplateEngine();
            string result = engine.Parse(template, context);

            return ExpectEqual(template, result, "result");
        }

        private static bool Test_Parse_ThrowsOnNullTemplate()
        {
            var engine = new TemplateEngine();
            var context = new Dictionary<string, string>();

            return ExpectThrowsArgumentNull(() => engine.Parse(null!, context), "template");
        }

        private static bool Test_Parse_ThrowsOnNullContext()
        {
            var engine = new TemplateEngine();

            return ExpectThrowsArgumentNull(() => engine.Parse("abc", null!), "context");
        }

        private static bool Test_Parse_ReturnsEmptyString_WhenTemplateIsEmpty()
        {
            var engine = new TemplateEngine();
            var context = new Dictionary<string, string>();

            string result = engine.Parse("", context);

            return ExpectEqual("", result, "result");
        }

        private static bool Test_Parse_ReplacesSimpleVariable()
        {
            string template = "Hi {=name:}.";
            var context = new Dictionary<string, string>
            {
                ["name"] = "Ronald"
            };

            var engine = new TemplateEngine();
            string result = engine.Parse(template, context);

            return ExpectEqual("Hi Ronald.", result, "result");
        }

        private static bool Test_Parse_MissingSubstitutionTag()
        {
            string template = "Hi {=name:}.";
            var context = new Dictionary<string, string>
            {
                ["notname"] = "Ronald"
            };

            var engine = new TemplateEngine();
            string result = engine.Parse(template, context);

            return ExpectEqual(template, result, "result");
        }

        private static bool Test_Parse_MissingActionTagNoSubtemplate()
        {
            string template = "{@days:}.";
            var context = new Dictionary<string, string>();

            var engine = new TemplateEngine();
            string result = engine.Parse(template, context);

            return ExpectEqual(template, result, "result");
        }

        private static bool Test_Parse_MissingActionTagWithSubtemplate()
        {
            string template = "{@days Phone rang  :}.";
            var context = new Dictionary<string, string>();

            var engine = new TemplateEngine();
            string result = engine.Parse(template, context);

            return ExpectEqual(template, result, "result");
        }

        private static bool Test_Parse_NestedSubTemplateAction() 
        {
            bool success = false;
            string template =
 @"{@nested {=title:}{@demographics
Name      {=Name:}
Country   {=Country:}
Christian {=Christian:}:}:}";

            var engine = new TemplateEngine();
            engine.AddAction("nested", new NestedAction());
            var context = new Dictionary<string, string>();
            string result = engine.Parse(template, context);

            if (result.Contains("Nested Test") &&
                result.Contains("Xecronix") &&
                result.Contains("Unknown") &&
                result.Contains("Yes"))
            {
                success = true;
            }

            if (!success)
            {
                BzEmitter.WriteLine("Test failed: Output: ");
                BzEmitter.WriteLine(result);
            }
            return success;
        }

        private static bool Test_Parse_ReuseParserAction()
        {
            bool success = false;
            string template =
 @"{@nested {=title:}{@demographics
Name      {=Name:}
Country   {=Country:}
Christian {=Christian:}:}:}";

            var engine = new TemplateEngine();
            engine.AddAction("nested", new ReuseParserNestedAction(engine));
            engine.AddAction("demographics", new ReuseParserDemographicsAction(engine));
            var context = new Dictionary<string, string>();
            string result = engine.Parse(template, context);

            if (result.Contains("Nested Test") &&
                result.Contains("Xecronix") &&
                result.Contains("Unknown") &&
                result.Contains("Yes"))
            {
                success = true;
            }

            if (!success)
            {
                BzEmitter.WriteLine("Test failed: Output: ");
                BzEmitter.WriteLine(result);
            }
            return success;
        }


        private static bool Test_Parse_LoopingAction()
        {
            bool success = false;
            string template =
 @"{@week day {=day:}
:}";

            var engine = new TemplateEngine();
            engine.AddAction("week", new LoopingAction());
            var context = new Dictionary<string, string>();
            string result = engine.Parse(template, context);
            if (result.Contains("Monday") &&
                result.Contains("Sun"))
            {
                success = true;
            }

            if (!success)
            {
                BzEmitter.WriteLine("Test failed: Output: ");
                BzEmitter.WriteLine(result);
            }
            return success;
        }
    }

    public class ReuseParserDemographicsAction : ITemplateAction
    {
        TemplateEngine Eagle;
        public ReuseParserDemographicsAction(TemplateEngine eagle) 
        {
            Eagle = eagle;
        }

        public string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
        {
            Dictionary<string, string> newTags = new Dictionary<string, string>();
            newTags.Add("Name", "Xecronix");
            newTags.Add("Country", "Unknown");
            newTags.Add("Christian", "Yes");
            string retval = Eagle.ParseTokens(tokens, newTags);
            return retval;
        }
    }

    public class ReuseParserNestedAction : ITemplateAction
    {
        TemplateEngine Eagle;
        public ReuseParserNestedAction(TemplateEngine eagle)
        {
            Eagle = eagle;
        }

        public string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
        {
            Dictionary<string, string> newTags = new Dictionary<string, string>();
            newTags.Add("title", "Nested Test");
            string retval = Eagle.ParseTokens(tokens, newTags);
            return retval;
        }
    }

    public class DemographicsAction : ITemplateAction
    {
        public string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
        {
            var eagle = new TemplateEngine();
            Dictionary<string, string> newTags = new Dictionary<string, string>();
            newTags.Add("Name", "Xecronix");
            newTags.Add("Country", "Unknown");
            newTags.Add("Christian", "Yes");
            string retval = eagle.ParseTokens(tokens, newTags);
            return retval;
        }
    }

    public class NestedAction : ITemplateAction
    {
        public string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
        {
            var eagle = new TemplateEngine();
            eagle.AddAction("demographics", new DemographicsAction());
            Dictionary<string, string> newTags = new Dictionary<string, string>();
            newTags.Add("title", "Nested Test");
            string retval = eagle.ParseTokens(tokens, newTags);
            return retval;
        }
    }

    public class LoopingAction : ITemplateAction
    {
        public string Run(Cursor<Token> tokens, IReadOnlyDictionary<string, string> context)
        {
            string retval = "";
            string [] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Sat", "Sun" };
            var eagle = new TemplateEngine();
            foreach (var day in days)
            {
                tokens.Rewind();
                Dictionary<string, string> newTags = new Dictionary<string, string>();
                newTags.Add ("day", day);
                retval += eagle.ParseTokens(tokens, newTags);
            }
            return retval;
        }
    }
}