
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Template : Program
    {
        public bool Init(int i) {
            if (i == 0) {
                // Do some initialization; if it's too much for one frame, return true and continue when called again (i is incremented)
                return true;
            }
            return false;
        }
    }
}