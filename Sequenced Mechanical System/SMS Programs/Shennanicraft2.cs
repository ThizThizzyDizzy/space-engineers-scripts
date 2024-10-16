
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Shenannicraft2 : Program
    {
        public bool Init(int i) {
            if (i == 0) {
                variables["Elevator Down"] = true;
                newSequence("Elevator 1 Up");
                setConditional("Elevator Down", true);
                setVar("Elevator Down", false);
                addMerge("Command Dock 1 Merge");
                addSensor("Command Dock 1 Sensor", true);
                addToggle("Command Dock 1 Door", true);
                addDoor("Command Dock 1 Door", true);
                addToggle("Command Dock 1 Door", false);
                addParallel();
                addToggle("Command Cube Door", true);
                addDoor("Command Cube Door", true);
                addToggle("Command Dock 1 Elevator", true);
                addPiston("Command Dock 1 Elevator", 10);
                endParallel();
                addToggle("Command Cube Door", false);
                addSensor("Command Dock 1 Sensor", false);
                addPiston("Command Dock 1 Elevator", 8);
                addParallel();
                addToggle("Command Cube Door", true);
                addDoor("Command Cube Door", false);
                addPiston("Command Dock 1 Elevator", 0);
                endParallel();
                addToggle("Command Cube Door", false);
                addToggle("Command Dock 1 Elevator", false);
                addToggle("Command Dock 1 Door", true);
                addDoor("Command Dock 1 Door", false);
                addToggle("Command Dock 1 Door", false);
                setVar("Elevator Down", true);
                return true;
            }
            if (i == 1) {
                newSequence("Elevator 1 Down");
                setConditional("Elevator Down", true);
                setVar("Elevator Down", false);
                addMerge("Command Dock 1 Merge");
                addSensor("Command Dock 1 Sensor", false);
                addToggle("Command Dock 1 Door", true);
                addDoor("Command Dock 1 Door", true);
                addToggle("Command Dock 1 Door", false);
                addToggle("Command Dock 1 Elevator", true);
                addPiston("Command Dock 1 Elevator", 8);
                addSensor("Command Dock 1 Sensor", false);
                addToggle("Command Cube Door", true);
                addDoor("Command Cube Door", true);
                addToggle("Command Cube Door", false);
                addPiston("Command Dock 1 Elevator", 10);
                addSensor("Command Dock 1 Sensor", true);
                addPiston("Command Dock 1 Elevator", 8);
                addParallel();
                addToggle("Command Cube Door", true);
                addDoor("Command Cube Door", false);
                addPiston("Command Dock 1 Elevator", 0);
                endParallel();
                addToggle("Command Cube Door", false);
                addToggle("Command Dock 1 Elevator", false);
                addToggle("Command Dock 1 Door", true);
                addDoor("Command Dock 1 Door", false);
                addToggle("Command Dock 1 Door", false);
                setVar("Elevator Down", true);
                return true;
            }
            return false;
        }
    }
}