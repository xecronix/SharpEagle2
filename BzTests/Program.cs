// File: Program.cs

using System.Reflection;

namespace BzTests;

internal static class Program
{
    private static int Main(string[] args)
    {
        List<TestMethod> tests = DiscoverTests();

        if (tests.Count == 0)
        {
            BzEmitter.WriteLine("No tests found.");
            BzEmitter.WriteLine("Convention: static bool methods with names starting with Test_");
            return 1;
        }

        int passCount = 0;
        int failCount = 0;
        int errorCount = 0;

        BzEmitter.WriteLine("========================================");
        BzEmitter.WriteLine("BzLite Test Tool");
        BzEmitter.WriteLine("========================================");
        BzEmitter.WriteLine($"Tests found: {tests.Count}");
        BzEmitter.WriteLine();

        foreach (TestMethod test in tests)
        {
            bool passed = false;
            Exception? error = null;

            try
            {
                object? result = test.Method.Invoke(null, null);
                passed = result is bool value && value;
            }
            catch (TargetInvocationException ex)
            {
                error = ex.InnerException ?? ex;
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error is not null)
            {
                errorCount++;
                BzEmitter.WriteResult("ERROR", test.DisplayName, ConsoleColor.Yellow);
                BzEmitter.WriteLine($"       {error.GetType().Name}: {error.Message}");
                continue;
            }

            if (passed)
            {
                passCount++;
                BzEmitter.WriteResult("PASS ", test.DisplayName, ConsoleColor.Green);
            }
            else
            {
                failCount++;
                BzEmitter.WriteResult("FAIL ", test.DisplayName, ConsoleColor.Red);
            }
        }

        BzEmitter.WriteLine();
        BzEmitter.WriteLine("========================================");
        BzEmitter.WriteLine($"Pass : {passCount}");
        BzEmitter.WriteLine($"Fail : {failCount}");
        BzEmitter.WriteLine($"Error: {errorCount}");
        BzEmitter.WriteLine($"Total: {tests.Count}");
        BzEmitter.WriteLine("========================================");

        return (failCount == 0 && errorCount == 0) ? 0 : 1;
    }

    private static List<TestMethod> DiscoverTests()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        List<TestMethod> tests = assembly
            .GetTypes()
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .SelectMany(type => type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(IsTestMethod)
                .Select(method => new TestMethod(type, method)))
            .OrderBy(test => test.DisplayName, StringComparer.Ordinal)
            .ToList();

        return tests;
    }

    private static bool IsTestMethod(MethodInfo method)
    {
        if (!method.Name.StartsWith("Test_", StringComparison.Ordinal))
        {
            return false;
        }

        if (method.ReturnType != typeof(bool))
        {
            return false;
        }

        if (method.GetParameters().Length != 0)
        {
            return false;
        }

        if (method.IsGenericMethod)
        {
            return false;
        }

        return true;
    }

    private sealed class TestMethod
    {
        public Type Type { get; }
        public MethodInfo Method { get; }
        public string DisplayName => $"{Type.Name}.{Method.Name}";

        public TestMethod(Type type, MethodInfo method)
        {
            Type = type;
            Method = method;
        }
    }
}
