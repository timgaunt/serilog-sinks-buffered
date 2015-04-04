using System;
using Serilog.Context;

namespace SerilogSinksBuffered.NestedScopes
{
    public class NestedLoggingScope : IDisposable
    {
        private readonly int[] _scope;
        private readonly Action<int[]> _endScopeAction;
        private readonly IDisposable _nestedContextProperty;
        private readonly IDisposable _nestedContextNameProperty;

        public NestedLoggingScope(int[] scope, string name, Action<int[]> endScopeAction)
        {
            _scope = scope;
            _endScopeAction = endScopeAction;
            _nestedContextProperty = LogContext.PushProperty(NestedLoggingSettings.NestingLevelPropertyName, scope);
            _nestedContextNameProperty = LogContext.PushProperty(NestedLoggingSettings.NestingLevelNamePropertyName, name);
        }

        public void Dispose()
        {
            _nestedContextNameProperty.Dispose();
            _nestedContextProperty.Dispose();
            _endScopeAction(_scope);
        }
    }
}