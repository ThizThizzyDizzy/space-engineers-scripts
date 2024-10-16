using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        const int rebuildTime = 60;
        int rebuildTimer = 0;
        Dictionary<String, Dictionary<String, List<IMyDoor>>> airlocks = new Dictionary<String, Dictionary<String, List<IMyDoor>>>();
        Dictionary<String, IMyAirVent> vents = new Dictionary<String, IMyAirVent>();
        Dictionary<String, List<IMyDoor>> temp = new Dictionary<String, List<IMyDoor>>();
        String errors;
        Random rand = new Random();
        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            rebuild();
        }
        public void Main(String arg) {
            rebuildTimer++;
            if (rebuildTimer > rebuildTime) {
                rebuild();
                rebuildTimer -= rebuildTime;
                return;
            }
            foreach (String airlock in airlocks.Keys) {
                int openDoors = 0;
                foreach (String doors in airlocks[airlock].Keys) {
                    foreach (IMyDoor d in airlocks[airlock][doors]) {
                        if (d.Status == DoorStatus.Open || d.Status == DoorStatus.Opening || d.Status == DoorStatus.Closing) {
                            openDoors++;
                            break;
                        }
                    }
                }
                if (vents.Keys.Contains(airlock)) vents[airlock].Depressurize = openDoors == 0;
                if (openDoors > 1) {
                    foreach (String doors in airlocks[airlock].Keys) {
                        foreach (IMyDoor d in airlocks[airlock][doors]) {
                            d.Enabled = true;
                            d.CloseDoor();
                        }
                    }
                }
                else {
                    foreach (String doors in airlocks[airlock].Keys) {
                        if (openDoors == 0) {
                            foreach (IMyDoor d in airlocks[airlock][doors]) {
                                d.Enabled = true;
                            }
                            Echo(airlock + " " + doors + " Open");
                        }
                        else {
                            bool doorOpen = false;
                            foreach (IMyDoor d in airlocks[airlock][doors]) {
                                if (d.Status == DoorStatus.Open || d.Status == DoorStatus.Opening || d.Status == DoorStatus.Closing) {
                                    doorOpen = true;
                                    break;
                                }
                            }
                            Echo(airlock + " " + doors + " " + doorOpen);
                            foreach (IMyDoor d in airlocks[airlock][doors]) {
                                d.Enabled = doorOpen;
                            }
                        }
                    }
                }
            }
            Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
            Me.GetSurface(0).WriteText(errors == "" ? "" : errors, false);
        }
        void rebuild() {
            errors = "";
            airlocks.Clear();
            vents.Clear();
            List<IMyTerminalBlock> lst = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(lst);
            foreach (IMyTerminalBlock b in lst) {
                if (b.CubeGrid != Me.CubeGrid) continue;
                if (!b.CustomData.StartsWith("Airlock ")) continue;
                IMyAirVent v = b as IMyAirVent;
                if (v != null) {
                    String airlock = v.CustomData;
                    vents.Add(airlock, v);
                }
                IMyDoor d = b as IMyDoor;
                if (d != null) {
                    String data = d.CustomData;
                    String airlock;
                    String door;
                    if (data.Contains("\n")) {
                        airlock = data.Substring(0, data.IndexOf("\n"));
                        door = data.Substring(data.IndexOf("\n") + 1);
                    }
                    else {
                        airlock = data;
                        door = "Door" + rand.NextDouble();
                    }
                    try {
                        airlocks[airlock][door].Add(d);
                    }
                    catch {
                        List<IMyDoor> doors = new List<IMyDoor>();
                        doors.Add(d);
                        try {
                            airlocks[airlock].Add(door, doors);
                        }
                        catch {
                            temp = new Dictionary<String, List<IMyDoor>>();
                            temp.Add(door, doors);
                            airlocks.Add(airlock, temp);
                        }
                    }
                }
            }
        }
        void error(String error) {
            Echo(error);
            if (errors == "") errors = error;
            else errors += "\n" + error;
        }
    }
}
