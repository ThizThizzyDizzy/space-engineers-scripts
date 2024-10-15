IMyShipController cockpit;
int updateTimer = 0;
int updateTime = 600;
List<IMyPistonBase> pistons = new List<IMyPistonBase>();
IMyMotorStator rotor1;
IMyMotorStator rotor2;
IMyMotorStator rotor3;
IMyMotorStator rotor4;
IMyMotorStator rotor5;
public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    updateAll();
}
public void Main(String arg){
    Echo("Running");
    updateTimer++;
    if(updateTimer>updateTime){
        updateTimer = 0;
        updateAll();
        return;
    }
    if(cockpit!=null){
        Echo(cockpit.CustomName);
        Vector3 move = cockpit.MoveIndicator;
        Vector2 rot = cockpit.RotationIndicator;
        float roll = cockpit.RollIndicator;
        if(rotor1==null)Echo("Failed to load rotor 1!");
        if(rotor2==null)Echo("Failed to load rotor 2!");
        if(rotor3==null)Echo("Failed to load rotor 3!");
        if(rotor4==null)Echo("Failed to load rotor 4!");
        if(rotor5==null)Echo("Failed to load rotor 5!");
        if(rotor1==null||rotor2==null||rotor3==null||rotor4==null||rotor5==null){
            return;
        }
    	Echo("Ready");
        float r1 = 0;
        float r2 = 0;
        float r3 = 0;
        float r4 = 0;
        float r5 = 0;
        float p = 0;
        if(move.Z!=0){
            float direction = move.Z/Math.Abs(move.Z);
            r1-=direction/2;
            r2+=direction;
            r3+=direction/2;
        }
        if(move.X!=0){
            float direction = move.X/Math.Abs(move.X);
            r1+=direction;
            r3+=direction;
        }
        if(move.Y!=0){
            float direction = move.Y/Math.Abs(move.Y);
            r4-=direction;
            r5-=direction;
        }
        if(roll!=0){
            float direction = roll/Math.Abs(roll);
            p+=direction;
        }
        rotor1.TargetVelocityRPM = r1;
        rotor2.TargetVelocityRPM = r2;
        rotor3.TargetVelocityRPM = r3;
        rotor4.TargetVelocityRPM = r4;
        rotor5.TargetVelocityRPM = r5;
        foreach(IMyPistonBase piston in pistons){
            piston.Velocity = p/pistons.Count;
        }
    }
}
void updateAll(){
    cockpit = null;
    pistons.Clear();
    rotor1 = rotor2 = rotor3 = rotor4 = rotor5 = null;
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(blocks);
    foreach(IMyTerminalBlock b in blocks){
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
        if(!b.IsWorking)continue;
        IMyShipController controller = b as IMyShipController;
        if(controller!=null){
            if(controller.IsUnderControl){
                cockpit = controller;
            }
        }
        IMyPistonBase piston = b as IMyPistonBase;
        if(piston!=null&&piston.CustomData.Contains("Arm")){
            pistons.Add(piston);
        }
        IMyMotorStator rotor = b as IMyMotorStator;
        if(rotor!=null&&rotor.CustomData.StartsWith("Arm ")){
            if(rotor.CustomData.Equals("Arm 1"))rotor1 = rotor;
            if(rotor.CustomData.Equals("Arm 2"))rotor2 = rotor;
            if(rotor.CustomData.Equals("Arm 3"))rotor3 = rotor;
            if(rotor.CustomData.Equals("Arm 4"))rotor4 = rotor;
            if(rotor.CustomData.Equals("Arm 5"))rotor5 = rotor;
        }
    }
}