using Xunit;
using FluentAssertions;
using System.Reflection;

namespace CzuProjekt.Tests;

public class ProgramTests : IDisposable
{
    private readonly string _testFilePath = "test.txt";
    private readonly ConfigData _testConfig;
    private readonly MethodInfo? _listLinesByPatternMethod;
    private readonly MethodInfo? _replaceTextByPatternMethod;

    public ProgramTests()
    {
        _testConfig = Config.DefaultConfig();

        var programType = typeof(Program);
        _listLinesByPatternMethod = programType.GetMethod("ListLinesByPattern",
            BindingFlags.NonPublic | BindingFlags.Static);
        _replaceTextByPatternMethod = programType.GetMethod("ReplaceTextByPattern",
            BindingFlags.NonPublic | BindingFlags.Static);
    }

    [Fact]
    public void ListLinesByPattern_ShouldFindMatches()
    {
        File.WriteAllText(_testFilePath, "Hello World\nTest 123\nHello Test");
        var pattern = @"Hello";

        var matches = (List<string>)_listLinesByPatternMethod?.Invoke(null,
            [_testFilePath, pattern, true, _testConfig])!;

        matches.Should().HaveCount(2);
        matches.Should().Contain("Hello");
    }

    [Fact]
    public void ListLinesByPattern_WithNoMatches_ShouldReturnEmptyList()
    {
        File.WriteAllText(_testFilePath, "Test 123\nTest 456");
        var pattern = @"Hello";

        var matches = (List<string>)_listLinesByPatternMethod!.Invoke(null,
            [_testFilePath, pattern, true, _testConfig])!;

        matches.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceTextByPattern_ShouldReplaceText()
    {
        var originalText = "Hello World\nTest Hello\nHello Test";
        File.WriteAllText(_testFilePath, originalText);
        var pattern = @"Hello";
        var replacement = "Hi";

        _replaceTextByPatternMethod!.Invoke(null,
            [_testFilePath, pattern, replacement, _testConfig]);
        var result = File.ReadAllText(_testFilePath);

        result.Should().Be("Hi World\nTest Hi\nHi Test");
    }

    [Fact]
    public void ReplaceTextByPattern_WithNoMatches_ShouldNotModifyFile()
    {
        var originalText = "Test 123\nTest 456";
        File.WriteAllText(_testFilePath, originalText);
        var pattern = @"Hello";
        var replacement = "Hi";

        _replaceTextByPatternMethod!.Invoke(null,
            [_testFilePath, pattern, replacement, _testConfig]);
        var result = File.ReadAllText(_testFilePath);

        result.Should().Be(originalText);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }
}