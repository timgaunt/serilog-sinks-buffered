using System;

namespace Serilog.Sinks.Buffered.NestedScopes
{
    public class NestedLogging : INestedLogging
    {
        private readonly NestedScopeInfo _nestedScopeInfo;

        public NestedLogging()
        {
            _nestedScopeInfo = new NestedScopeInfo();
        }

        public NestedLoggingScope BeginScope()
        {
            return BeginScope(string.Empty);
        }

        public NestedLoggingScope BeginScope(string name)
        {
            var scope = _nestedScopeInfo.BeginScope();
            var newScope = new NestedLoggingScope(scope, name, EndScope);
            return newScope;
        }

        internal void EndScope(int[] scope)
        {
            if (_nestedScopeInfo.NoActiveScope())
            {
                throw new InvalidOperationException("Cannot end a logging scope as none are active");
            }

            if (!_nestedScopeInfo.EndScope(scope))
            {
                throw new InvalidOperationException("The logging scope being ended is not the expected logging scope");
            }
        }
    }
}