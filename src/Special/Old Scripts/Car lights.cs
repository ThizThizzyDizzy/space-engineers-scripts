double? speed;
MyShipVelocities? velocities;
MyShipMass? mass;
IMyShipController cockpit;
public bool highBeams = false;
int updateTimer = 0;
int updateTime = 6;
List<IMyLightingBlock> leftSignal = new List<IMyLightingBlock>();
List<IMyLightingBlock> rightSignal = new List<IMyLightingBlock>();
List<IMyLightingBlock> brakes = new List<IMyLightingBlock>();
List<IMyLightingBlock> headlights = new List<IMyLightingBlock>();
List<IMySolarPanel> solars = new List<IMySolarPanel>();
public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}
public void Main(String arg){
    if(arg=="Hi")highBeams = true;
    if(arg=="Lo")highBeams = false;
    if(arg=="ToggleHeadlights")highBeams = !highBeams;
    updateTimer++;
    if(updateTimer>updateTime){
        updateTimer = 0;
        updateAll();
        return;
    }
    double max = 0;
    foreach(IMySolarPanel solar in solars){
        max = Math.Max(max, solar.CurrentOutput);
    }
    if(cockpit!=null){
        Vector3 move = cockpit.MoveIndicator;
        Vector2 rot = cockpit.RotationIndicator;
        float roll = cockpit.RollIndicator;
        bool left = move.X<0;
        bool right = move.X>0;
        bool stop = move.Y>0;
        bool headlightsOn = max<.01;
        foreach(IMyLightingBlock light in leftSignal){
            light.Radius = 2;
            light.Intensity = left?5:0;
            light.BlinkLength = 50f;
            light.BlinkIntervalSeconds = 1;
            light.Color = new Color(1f,0.5f,0f);
        }
        foreach(IMyLightingBlock light in rightSignal){
            light.Radius = 2;
            light.Intensity = right?5:0;
            light.BlinkLength = 50f;
            light.BlinkIntervalSeconds = 1;
            light.Color = new Color(1f,0.5f,0f);
        }
        foreach(IMyLightingBlock light in brakes){
            light.Color = new Color(1f,0f,0f);
            if(headlightsOn||highBeams)light.Radius = 1;
            else light.Radius = 0;
            light.Intensity = 2;
            if(stop){
                light.Radius = 6;
                light.Intensity = 3;
            }
        }
        foreach(IMyLightingBlock light in headlights){
            if(headlightsOn||highBeams){
                light.Radius = 25;
                if(highBeams)light.Radius = 120;
            }else light.Radius = 0;
        }
    }
}
void updateAll(){
    cockpit = null;
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(blocks);
    speed = null;
    velocities = null;
    mass = null;
    foreach(IMyTerminalBlock b in blocks){
        if(b.CubeGrid!=Me.CubeGrid) continue;
        if(!b.IsWorking)continue;
        IMyShipController controller = b as IMyShipController;
        if(controller!=null){
            if(controller.IsMainCockpit){
                cockpit = controller;
                break;
            }
            cockpit = controller;
        }
    }
    foreach(IMyTerminalBlock b in blocks){
        if(b.CubeGrid!=Me.CubeGrid) continue;
        if(!b.IsWorking)continue;
        IMyShipController controller = b as IMyShipController;
        if(controller!=null){
            if(controller.IsUnderControl){
                cockpit = controller;
            }
        }
        IMyLightingBlock light = b as IMyLightingBlock;
        if(light!=null){
            if(light.CustomData.Contains("Left Turn Signal"))leftSignal.Add(light);
            if(light.CustomData.Contains("Right Turn Signal"))rightSignal.Add(light);
            if(light.CustomData.Contains("Brake Light"))brakes.Add(light);
            if(light.CustomData.Contains("Headlight"))headlights.Add(light);
        }
        IMySolarPanel solar = b as IMySolarPanel;
        if(solar!=null)solars.Add(solar);
    }
    if(cockpit==null)return;
    speed = cockpit.GetShipSpeed();
    velocities = cockpit.GetShipVelocities();
    mass = cockpit.CalculateShipMass();
}