using System;

namespace Serilog.Sinks.Buffered.NestedScopes
{
    public static class NestedLoggingSettings
    {
        
        public const string NestingLevelPropertyName = "TheSiteDoctor.NestedLoggingScope.Level";
        public const string NestingLevelNamePropertyName = "TheSiteDoctor.NestedLoggingScope.LevelName";

        private static INestedLoggingProvider _provider;

        public static INestedLoggingProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    throw new InvalidOperationException("No NestedLoggingSettings.NestedLoggingProvider has been set");
                }
                return _provider;
            }
            set { _provider = value; }
        }
    }
}