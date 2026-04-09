// PROTOTYPE - NOT FOR PRODUCTION
// MCP Test Framework - Test Case Interface
// Date: 2026-04-07

using System;
using System.Collections;
using System.Collections.Generic;

namespace TowerDefenseRush.Testing
{
    public enum TestResult
    {
        NotRun,
        Running,
        Passed,
        Failed,
        Skipped,
        Error
    }

    public enum TestLogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    [Serializable]
    public class TestLogEntry
    {
        public DateTime Timestamp;
        public TestLogLevel Level;
        public string Message;
        public string Context;
        public string StackTrace;

        public TestLogEntry(TestLogLevel level, string message, string context = "")
        {
            Timestamp = DateTime.Now;
            Level = level;
            Message = message;
            Context = context;
            StackTrace = "";
        }
    }

    public interface ITestCase
    {
        string TestId { get; }
        string TestName { get; }
        string Description { get; }
        string Category { get; }
        int Priority { get; }
        float Timeout { get; }

        TestResult Result { get; }
        string ErrorMessage { get; }
        List<TestLogEntry> Logs { get; }
        float ExecutionTime { get; }

        IEnumerator SetUp();
        IEnumerator Run();
        IEnumerator TearDown();
        bool Validate();
    }

    public class TestAssertionException : Exception
    {
        public TestAssertionException(string message) : base(message) { }
    }
}
