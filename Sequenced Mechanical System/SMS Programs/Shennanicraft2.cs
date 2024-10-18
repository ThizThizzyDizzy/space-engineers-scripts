
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program
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
            if (i == 2) {
                newSequence("Reset Main Arm");
                addToggle("Main Arm Merge Block", false);
                addParallel();
                addPiston("Main Arm Rail Piston 1", 5f, .25f);
                addPiston("Main Arm Rail Piston 2", 5f, .25f);
                addPiston("Main Arm Rail Piston 3", 5f, .25f);
                addRotor("Main Arm Yaw Rotor", 0, 0.5f);
                addHinge("Main Arm Elevation Rotor 1", 0, 1f);
                addRotor("Main Arm Elevation Rotor 2", -180, 1.5f);
                addRotor("Main Arm Tip Rotor", 0, 2f);
                addHinge("Main Arm Tool Hinge", 0, 1.5f);
                addRotor("Main Arm Tool Rotor", 0, 2f);
                endParallel();
                addParallel();
                addPiston("Main Arm Lower Piston 1", 0f, 0.5f);
                addPiston("Main Arm Lower Piston 2", 0f, 0.5f);
                addPiston("Main Arm Lower Piston 3", 0f, 0.5f);
                addPiston("Main Arm Upper Piston 1", 0f, 0.5f);
                addPiston("Main Arm Upper Piston 2", 0f, 0.5f);
                addPiston("Main Arm Upper Piston 3", 0f, 0.5f);
                addPiston("Main Arm Upper Piston 4", 0f, 0.5f);
                addPiston("Main Arm Upper Piston 5", 0f, 0.5f);
                endParallel();
                addParallel();
                MainArmElevation(0f, false);
                addHinge("Main Arm Tool Hinge", 90, 5);
                endParallel();
                MainArmRail(0f, true);
                return true;
            }
            if (i == 3) {
                newSequence("Stow Main Arm");
                addToggle("Main Arm Merge Block", false);
                MainArmElevation(.5f, .75f);
                addParallel();
                ExtendArm(0, 0, false, speedMultiplier: 3);
                MainArmYaw(0f);
                addHinge("Main Arm Tool Hinge", 90, 5);
                addRotor("Main Arm Tip Rotor", 0, 5);
                addRotor("Main Arm Tool Rotor", 0, 5);
                endParallel();
                addParallel();
                MainArmRail(0.25f, false);
                MainArmElevation(0f);
                endParallel();
                MainArmRail(0f);
                return true;
            }
            if (i == 4) {
                newSequence("Stow Drills");
                addToggle("Main Arm Merge Block", false);
                addParallel();
                MainArmRail(0.25f, false);
                MainArmElevation(0.375f, false);
                addRotor("Nose Rotor", 0f, 1, 90);
                addPiston("Nose Piston", 0f, 0.5f);
                endParallel();
                addParallel();
                MainArmYaw(-7f, 0.1f);
                ExtendArm(.62f, 0f, false, 2f);
                MainArmElevation(1.467f, 0.439f, false, 1f, 0.7f);
                addHinge("Main Arm Tool Hinge", 38, 1);
                addRotor("Main Arm Tool Rotor", -7, 1);
                addRotor("Main Arm Tip Rotor", 0, 1);
                endParallel();
                addToggle("Main Arm Merge Block", true);
                addMerge("Main Arm Merge Block");
                addRotorDetach("Nose Rotor");
                MainArmElevation(1.467f, .6f, true, speedMultiplier: 0.3f);
                addParallel();
                MainArmYaw(90, 0.2f);
                addHinge("Main Arm Tool Hinge", -10, 1);
                addRotor("Main Arm Tool Rotor", 180, 0.5f);
                endParallel();
                addParallel();
                MainArmRail(0.228f, false, 0.2f);
                MainArmElevation(1.718f, 0.359f, false, speedMultiplier: 0.2f);
                ExtendArm(0.242f, 0f, false);
                addHinge("Main Arm Tool Hinge", 90, 1f);
                addRotor("Main Arm Tip Rotor", 0, 0.5f);
                endParallel();
                addToggle("Drill Bay Merge Block", true);
                addMerge("Drill Bay Merge Block");
                addToggle("Main Arm Merge Block", false);
                runSequence("Stow Main Arm");
                return true;
            }
            if (i == 5) {
                newSequence("Attach Drills");
                addToggle("Main Arm Merge Block", false);
                addParallel();
                MainArmRail(0.25f, false);
                MainArmElevation(0.25f, false);
                MainArmYaw(90);
                endParallel();
                addParallel();
                addRotor("Main Arm Tool Rotor", 180, 3f);
                MainArmRail(0.228f, false, 0.2f);
                MainArmElevation(1.718f, 0.359f, false, 0.7f);
                ExtendArm(0.242f, 0f, false);
                addHinge("Main Arm Tool Hinge", 90, 2f);
                addRotor("Main Arm Tip Rotor", 0, 2f);
                endParallel();
                addToggle("Main Arm Merge Block", true);
                addMerge("Main Arm Merge Block");
                addToggle("Drill Bay Merge Block", false);
                addParallel();
                addHinge("Main Arm Tool Hinge", -10, 1f);
                ExtendArm(.62f, 0f, false);
                MainArmElevation(1.467f, .6f, false, speedMultiplier: 0.2f);
                MainArmRail(0.25f, false);
                endParallel();
                addParallel();
                addRotor("Main Arm Tool Rotor", -7, 0.5f);
                MainArmYaw(-7, 0.2f);
                endParallel();
                addParallel();
                addHinge("Main Arm Tool Hinge", 38, 1f);
                MainArmElevation(1.467f, 0.439f, true, speedMultiplier: 0.3f);
                endParallel();
                addRotorAttach("Nose Rotor");
                addToggle("Main Arm Merge Block", false);
                runSequence("Stow Main Arm");
                return true;
            }
            if (i == 6) {
                newSequence("Test");
                addParallel();
                endParallel();
                return true;
            }
            return false;
        }
        private void MainArmYaw(float degrees, float speedMultiplier = 1f) {
            addRotor("Main Arm Yaw Rotor", degrees, 2f*speedMultiplier, 90);
        }
        private void MainArmRail(float percent, bool standalone = true, float speedMultiplier = 1f) {
            if(standalone)addParallel();
            addPiston("Main Arm Rail Piston 1", percent * 10, .25f * speedMultiplier);
            addPiston("Main Arm Rail Piston 2", percent * 10, .25f * speedMultiplier);
            addPiston("Main Arm Rail Piston 3", percent * 10, .25f * speedMultiplier);
            if(standalone)endParallel();
        }
        private void MainArmElevation(float percent, bool standalone = true) {
            MainArmElevation(percent, percent, standalone);
        }
        private void MainArmElevation(float percent1, float percent2, bool standalone = true, float speedRatio = 1.5f, float speedMultiplier = 1f) {
            if (standalone) addParallel();
            addHinge("Main Arm Elevation Rotor 1", -90 + percent1 * 90, 1 * speedMultiplier);
            addRotor("Main Arm Elevation Rotor 2", 0 - percent2 * 180, speedRatio * speedMultiplier);
            if (standalone) endParallel();
        }
        private void ExtendArm(float percent1, float percent2, bool standalone = true, float speedMultiplier = 1f) {
            if (standalone) addParallel();
            addPiston("Main Arm Lower Piston 1", percent1 * 10, 0.2f * speedMultiplier, 0.5f);
            addPiston("Main Arm Lower Piston 2", percent1 * 10, 0.2f * speedMultiplier, 0.5f);
            addPiston("Main Arm Lower Piston 3", percent1 * 10, 0.2f * speedMultiplier, 0.5f);
            addPiston("Main Arm Upper Piston 1", percent2 * 10, 0.2f * speedMultiplier, 0.5f);
            addPiston("Main Arm Upper Piston 2", percent2 * 10, 0.2f * speedMultiplier, 0.5f);
            addPiston("Main Arm Upper Piston 3", percent2 * 10, 0.2f * speedMultiplier, 0.5f);
            addPiston("Main Arm Upper Piston 4", percent2 * 10, 0.2f * speedMultiplier, 0.5f);
            addPiston("Main Arm Upper Piston 5", percent2 * 10, 0.2f * speedMultiplier, 0.5f);
            if (standalone) endParallel();
        }
    }
}