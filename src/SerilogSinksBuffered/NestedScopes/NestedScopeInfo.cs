using System.Collections.Generic;
using System.Linq;

namespace Serilog.Sinks.Buffered.NestedScopes
{
    public class NestedScopeInfo
    {
        public Stack<int> CurrentLevels { get; set; }
        public int NextLevel { get; set; }

        public NestedScopeInfo()
        {
            CurrentLevels = new Stack<int>();
        }

        public int[] BeginScope()
        {
            CurrentLevels.Push(NextLevel);
            NextLevel = 0;
            return CurrentLevels.ToArray();
        }


        public bool EndScope(int[] expectedScope)
        {
            if (CurrentLevels.SequenceEqual(expectedScope))
            {
                var currentLevel = CurrentLevels.Pop();
                NextLevel = currentLevel + 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool NoActiveScope()
        {
            return CurrentLevels.Count == 0;
        }
    }
}
