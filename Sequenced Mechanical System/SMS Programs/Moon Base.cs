
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class MoonBase : Program
    {
        public bool Init(int i) {
            newSequence("Elewator");
            addPiston("Elewator Piston", 7.925f, 5, 10);
            addDelay(150);
            addPiston("Elewator Piston", 4.415f, 5, 10);
            return false;
        }
    }
}