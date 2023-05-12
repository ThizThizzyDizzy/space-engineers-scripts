const bool thrusterDirectionOverride = true;
Vector3D naturalGravity;
Vector3D artificialGravity;
Vector3D gravity;
double speed;
MyShipVelocities velocities;
MyShipMass mass;
Vector3D planetPosition;
double seaLevelElevation;
double surfaceElevation;
ThrusterConfiguration thrusters;
IMyShipController cockpit;
public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

    GridTerminalSystem.GetBlocks(blocks);

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

    thrusters = new ThrusterConfiguration(this, cockpit);

    foreach(IMyTerminalBlock b in blocks){
        if(b.CubeGrid!=Me.CubeGrid) continue;
        IMyThrust thrust = b as IMyThrust;
        if(thrust!=null){
           thrusters.add(thrust);
        }
    }
}
public void Main(String arg){
    naturalGravity = cockpit.GetNaturalGravity();
    artificialGravity = cockpit.GetArtificialGravity();
    gravity = cockpit.GetTotalGravity();
    speed = cockpit.GetShipSpeed();
    velocities = cockpit.GetShipVelocities();
    mass = cockpit.CalculateShipMass();
    cockpit.TryGetPlanetPosition(out planetPosition);
    cockpit.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out seaLevelElevation);
    cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out surfaceElevation);
    thrusters.refresh();
    Vector3 targetVelocity = Vector3.Zero;
    Vector3 currentVelocity = Vector3.TransformNormal(velocities.LinearVelocity.Normalized(), Matrix.Transpose(cockpit.WorldMatrix)).Normalized()*(float)velocities.LinearVelocity.Length();
    Vector3 localGravity = Vector3.TransformNormal(naturalGravity.Normalized(), Matrix.Transpose(cockpit.WorldMatrix)).Normalized()*(float)naturalGravity.Length();
    thrusters.setThrustForces((currentVelocity+localGravity-targetVelocity)*mass.TotalMass);
}
public class ThrusterConfiguration{
    public IMyShipController cockpit;
    public List<IMyThrust> thrusters = new List<IMyThrust>();
    public double up;
    public double down;
    public double left;
    public double right;
    public double forward;
    public double backward;
    Program p;
    public ThrusterConfiguration(Program p, IMyShipController referenceCockpit){
        this.cockpit = referenceCockpit;
        this.p = p;
    }
    public void clear(){
        up = down = left = right = forward = backward = 0;
    }
    public void add(IMyThrust thruster){
        thrusters.Add(thruster);
    }
    public void refresh(){
        clear();
        foreach(IMyThrust thruster in thrusters){
            Vector3I direction = thruster.GridThrustDirection;
            if(direction==Vector3I.Zero||thrusterDirectionOverride){
                if(cockpit==null){
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Up)up+=thruster.MaxEffectiveThrust;
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Down)down+=thruster.MaxEffectiveThrust;
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Left)left+=thruster.MaxEffectiveThrust;
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Right)right+=thruster.MaxEffectiveThrust;
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Forward)forward+=thruster.MaxEffectiveThrust;
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Backward)backward+=thruster.MaxEffectiveThrust;
                    return;
                }
                Base6Directions.Direction Up = cockpit.Orientation.Up;
                Base6Directions.Direction Forward = cockpit.Orientation.Forward;
                Base6Directions.Direction Left = cockpit.Orientation.Left;
                Base6Directions.Direction Backward = new MyBlockOrientation(Left, Up).Left;
                Base6Directions.Direction Right = new MyBlockOrientation(Backward, Up).Left;
                Base6Directions.Direction Down = new MyBlockOrientation(Forward, Left).Left;
                if(thruster.Orientation.Forward==Up)up+=thruster.MaxEffectiveThrust;
                if(thruster.Orientation.Forward==Down)down+=thruster.MaxEffectiveThrust;
                if(thruster.Orientation.Forward==Left)left+=thruster.MaxEffectiveThrust;
                if(thruster.Orientation.Forward==Right)right+=thruster.MaxEffectiveThrust;
                if(thruster.Orientation.Forward==Forward)forward+=thruster.MaxEffectiveThrust;
                if(thruster.Orientation.Forward==Backward)backward+=thruster.MaxEffectiveThrust;
            }else{
                if(direction==Vector3I.Up)up+=thruster.MaxEffectiveThrust;
                if(direction==Vector3I.Down)down+=thruster.MaxEffectiveThrust;
                if(direction==Vector3I.Left)left+=thruster.MaxEffectiveThrust;
                if(direction==Vector3I.Right)right+=thruster.MaxEffectiveThrust;
                if(direction==Vector3I.Forward)forward+=thruster.MaxEffectiveThrust;
                if(direction==Vector3I.Backward)backward+=thruster.MaxEffectiveThrust;
            }
        }
    }
    public Vector3 posify(Vector3 vec){
        if(vec.X<0)vec.X = 0;
        if(vec.Y<0)vec.Y = 0;
        if(vec.Z<0)vec.Z = 0;
        return vec;
    }
    public void setThrustForces(Vector3 forces){
        float targetLeft01 = posify(forces*Vector3.Left).Length()/(float)left;
        float targetRight01 = posify(forces*Vector3.Right).Length()/(float)right;
        float targetForward01 = posify(forces*Vector3.Forward).Length()/(float)forward;
        float targetBackward01 = posify(forces*Vector3.Backward).Length()/(float)backward;
        float targetUp01 = posify(forces*Vector3.Up).Length()/(float)up;
        float targetDown01 = posify(forces*Vector3.Down).Length()/(float)down;
        
        foreach(IMyThrust thruster in thrusters){
            Vector3I direction = thruster.GridThrustDirection;
            if(direction==Vector3I.Zero||thrusterDirectionOverride){
                if(cockpit==null){
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Up)thruster.ThrustOverridePercentage = targetUp01;
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Down)thruster.ThrustOverridePercentage = targetDown01;
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Left)thruster.ThrustOverridePercentage = targetLeft01;
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Right)thruster.ThrustOverridePercentage = targetRight01;
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Forward)thruster.ThrustOverridePercentage = targetForward01;
                    if(thruster.Orientation.Forward==Base6Directions.Direction.Backward)thruster.ThrustOverridePercentage = targetBackward01;
                    return;
                }
                Base6Directions.Direction Up = cockpit.Orientation.Up;
                Base6Directions.Direction Forward = cockpit.Orientation.Forward;
                Base6Directions.Direction Left = cockpit.Orientation.Left;
                Base6Directions.Direction Backward = new MyBlockOrientation(Left, Up).Left;
                Base6Directions.Direction Right = new MyBlockOrientation(Backward, Up).Left;
                Base6Directions.Direction Down = new MyBlockOrientation(Forward, Left).Left;
                if(thruster.Orientation.Forward==Up)thruster.ThrustOverridePercentage = targetUp01;
                if(thruster.Orientation.Forward==Down)thruster.ThrustOverridePercentage = targetDown01;
                if(thruster.Orientation.Forward==Left)thruster.ThrustOverridePercentage = targetLeft01;
                if(thruster.Orientation.Forward==Right)thruster.ThrustOverridePercentage = targetRight01;
                if(thruster.Orientation.Forward==Forward)thruster.ThrustOverridePercentage = targetForward01;
                if(thruster.Orientation.Forward==Backward)thruster.ThrustOverridePercentage = targetBackward01;
            }else{
                if(direction==Vector3I.Up)thruster.ThrustOverridePercentage = targetUp01;
                if(direction==Vector3I.Down)thruster.ThrustOverridePercentage = targetDown01;
                if(direction==Vector3I.Left)thruster.ThrustOverridePercentage = targetLeft01;
                if(direction==Vector3I.Right)thruster.ThrustOverridePercentage = targetRight01;
                if(direction==Vector3I.Forward)thruster.ThrustOverridePercentage = targetForward01;
                if(direction==Vector3I.Backward)thruster.ThrustOverridePercentage = targetBackward01;
            }
        }
    }
}
public class InventoryContainer{
    public List<IMyTerminalBlock> containers = new List<IMyTerminalBlock>();
    Program p;
    public double rateT = 0;
    String type = "";
    public int interval = 30;
    public double last = -1;
    public List<double> diffs = new List<double>();
    public InventoryContainer(Program p, String type){
        this.p = p;
        this.type = type;
        refresh();
    }
    public void refresh(){
        containers.Clear();
        search(p.Me.CubeGrid, type);
    }
    public void tick(){
        double fillT = getFillLevelI();
        if(last==-1)last = fillT;
        double diffT = fillT-last;
        diffs.Add(diffT);
        if(diffs.Count>interval)diffs.RemoveAt(0);
        double averageDiff = 0;
        foreach(double d in diffs)averageDiff+=d;
        averageDiff/=diffs.Count;
        rateT = averageDiff*6;
        last = fillT;
    }
    public void search(IMyCubeGrid grid, String type){
        List<IMyTerminalBlock> lst = new List<IMyTerminalBlock>();
        p.GridTerminalSystem.GetBlocks(lst);
        foreach(IMyTerminalBlock b in lst){
            if(b.CubeGrid!=grid)continue;
            if(b.HasInventory)containers.Add(b);
        }
    }
    public double getNetChange(){
        return rateT;
    }
    public double getSecondsUntilCharged(){
        if(rateT==0)return 0;
//        if(rate==0)return 0;
//        if(rate>0)return (getCapacity()-getFillLevel())/rate;
        return -getFillLevelI()/rateT;
    }
    public double getFillPercent(){
        return getFillLevel()/getCapacity();
    }
    public double getCapacity(){
        MyFixedPoint capacity = new MyFixedPoint();
        foreach(IMyTerminalBlock container in containers){
            IMyInventory inv = container.GetInventory();
            if(inv==null)continue;
            capacity = MyFixedPoint.AddSafe(capacity, inv.MaxVolume);
        }
        return MyFixedPoint.MultiplySafe(10000, capacity).ToIntSafe()/10000d;
    }
    public double getFillLevel(){
        MyFixedPoint fillLevel = new MyFixedPoint();
        foreach(IMyTerminalBlock container in containers){
            IMyInventory inv = container.GetInventory();
            if(inv==null)continue;
            fillLevel = MyFixedPoint.AddSafe(fillLevel, inv.CurrentVolume);
        }
        return MyFixedPoint.MultiplySafe(10000, fillLevel).ToIntSafe()/10000d;
    }
    public double getFillLevelI(){
        MyFixedPoint fillLevel = new MyFixedPoint();
        foreach(IMyTerminalBlock container in containers){
            IMyInventory inv = container.GetInventory();
            if(inv==null)continue;
            MyItemType item = MyItemType.MakeOre(type);
            fillLevel = MyFixedPoint.AddSafe(fillLevel, inv.GetItemAmount(item));
        }
        return fillLevel.ToIntSafe();
    }
}
public class InventoryOverview{
    public List<String> allowedContainers = new List<String>();
    public List<IMyTerminalBlock> containers = new List<IMyTerminalBlock>();
    Program p;
    public InventoryOverview(Program p){
        this.p = p;
        allowedContainers.Add("Small Cargo Container");
        allowedContainers.Add("Medium Cargo Container");
        allowedContainers.Add("Large Cargo Container");
        allowedContainers.Add("Connector");
        allowedContainers.Add("Cockpit");
        allowedContainers.Add("Fighter Cockpit");
        refresh();
    }
    public void refresh(){
        containers.Clear();
        search(p.Me.CubeGrid);
    }
    public void search(IMyCubeGrid grid){
        List<IMyTerminalBlock> lst = new List<IMyTerminalBlock>();
        p.GridTerminalSystem.GetBlocks(lst);
        foreach(IMyTerminalBlock b in lst){
            if(b.CubeGrid!=grid)continue;
            if(b.HasInventory&&allowedContainers.Contains(b.DefinitionDisplayNameText))containers.Add(b);
        }
    }
    public double getFillPercent(){
        return getFillLevel()/getCapacity();
    }
    public double getCapacity(){
        MyFixedPoint capacity = new MyFixedPoint();
        foreach(IMyTerminalBlock container in containers){
            IMyInventory inv = container.GetInventory();
            if(inv==null)continue;
            capacity = MyFixedPoint.AddSafe(capacity, inv.MaxVolume);
        }
        return MyFixedPoint.MultiplySafe(10000, capacity).ToIntSafe()/10000d;
    }
    public double getFillLevel(){
        MyFixedPoint fillLevel = new MyFixedPoint();
        foreach(IMyTerminalBlock container in containers){
            IMyInventory inv = container.GetInventory();
            if(inv==null)continue;
            fillLevel = MyFixedPoint.AddSafe(fillLevel, inv.CurrentVolume);
        }
        return MyFixedPoint.MultiplySafe(10000, fillLevel).ToIntSafe()/10000d;
    }
}
public class GasTank{
    public List<IMyTerminalBlock> tanks = new List<IMyTerminalBlock>();
    Program p;
    double lastFill = 0;
    double rate = 0;
    String type = "";
    public GasTank(Program p, String type){
        this.p = p;
        this.type = type;
        refresh();
    }
    public void refresh(){
        tanks.Clear();
        search(p.Me.CubeGrid, type);
    }
    public void tick(){
        double fill = getFillLevel();
        double diff = fill-lastFill;
        rate = diff*6;
        lastFill = fill;
    }
    public void search(IMyCubeGrid grid, String type){
        List<IMyTerminalBlock> lst = new List<IMyTerminalBlock>();
        p.GridTerminalSystem.GetBlocks(lst);
        foreach(IMyTerminalBlock b in lst){
            if(b.CubeGrid!=grid)continue;
            if(b as IMyGasTank!=null&&b.DefinitionDisplayNameText.Contains(type))tanks.Add(b);
        }
    }
    public double getNetChange(){
        return rate;
    }
    public double getSecondsUntilCharged(){
        double net = getNetChange();
        if(net==0)return 0;
        if(net>0)return (getCapacity()-getFillLevel())/net;
        return -getFillLevel()/net;
    }
    public double getFillPercent(){
        return getFillLevel()/getCapacity();
    }
    public double getCapacity(){
        double capacity = 0;
        foreach(IMyTerminalBlock tank in tanks){
            IMyGasTank t = tank as IMyGasTank;
            capacity += getCapacity(t);
        }
        return capacity;
    }
    public double getFillLevel(){
        double fillLevel = 0;
        foreach(IMyTerminalBlock tank in tanks){
            IMyGasTank t = tank as IMyGasTank;
            fillLevel += getFillLevel(t);
        }
        return fillLevel;
    }
    double getFillLevel(IMyGasTank tank){
        if(tank==null||!tank.IsWorking) return 0;
        return tank.Capacity*tank.FilledRatio;
    }
    double getCapacity(IMyGasTank tank){
        if(tank==null||!tank.IsWorking) return 0;
        return tank.Capacity;
    }
}
public class Battery{
    public List<IMyTerminalBlock> batteries = new List<IMyTerminalBlock>();
    Program p;
    public Battery(Program p){
        this.p = p;
        refresh();
    }
    public void refresh(){
        batteries.Clear();
        search(p.Me.CubeGrid);
    }
    public void search(IMyCubeGrid grid){
        List<IMyTerminalBlock> lst = new List<IMyTerminalBlock>();
        p.GridTerminalSystem.GetBlocks(lst);
        foreach(IMyTerminalBlock b in lst){
            if(b.CubeGrid!=grid) continue;
            if(b as IMyBatteryBlock!=null) batteries.Add(b);
        }
    }
    public double getNetChange(){
        double change = 0;
        foreach(IMyTerminalBlock batt in batteries){
            IMyBatteryBlock b = batt as IMyBatteryBlock;
            change += b.CurrentInput*1000000;
            change -= b.CurrentOutput*1000000;
        }
        return change;
    }
    public double getSecondsUntilCharged(){
        double net = getNetChange();
        if(net==0)return 0;
        if(net>0)return (getMaxCharge()-getCharge())/net*60*60;
        return -getCharge()/net*60*60;
    }
    public double getChargePercent(){
        long max = 0;
        long charge = 0;
        foreach(IMyTerminalBlock batt in batteries){
            IMyBatteryBlock b = batt as IMyBatteryBlock;
            charge += GetCharge(b);
            max += GetMaxCharge(b);
        }
        return (double)charge/max;
    }
    public double getMaxCharge(){
        long max = 0;
        foreach(IMyTerminalBlock batt in batteries){
            IMyBatteryBlock b = batt as IMyBatteryBlock;
            max += GetMaxCharge(b);
        }
        return (double)max;
    }
    public double getCharge(){
        long charge = 0;
        foreach(IMyTerminalBlock batt in batteries){
            IMyBatteryBlock b = batt as IMyBatteryBlock;
            charge += GetCharge(b);
        }
        return (double)charge;
    }
    long GetCharge(IMyBatteryBlock battery){
        if(battery==null||!battery.Enabled) return 0;
        return (long)(((double)battery.CurrentStoredPower)*1000000);
    }
    long GetMaxCharge(IMyBatteryBlock battery){
        if(battery==null) return 0;
        return (long)(((double)battery.MaxStoredPower)*1000000);
    }
}
public class JumpDrive{
    public List<IMyTerminalBlock> drives = new List<IMyTerminalBlock>();
    Program p;
    double lastFill = 0;
    double rate = 0;
    public JumpDrive(Program p){
        this.p = p;
        refresh();
    }
    public void refresh(){
        drives.Clear();
        search(p.Me.CubeGrid);
    }
    public void tick(){
        double fill = getFillLevel();
        double diff = fill-lastFill;
        rate = diff*6;
        lastFill = fill;
    }
    public void search(IMyCubeGrid grid){
        List<IMyTerminalBlock> lst = new List<IMyTerminalBlock>();
        p.GridTerminalSystem.GetBlocks(lst);
        foreach(IMyTerminalBlock b in lst){
            if(b.CubeGrid!=grid)continue;
            if(b as IMyJumpDrive!=null)drives.Add(b);
        }
    }
    public double getNetChange(){
        return rate;
    }
    public double getSecondsUntilCharged(){
        double net = getNetChange();
        if(net==0)return 0;
        if(net>0)return (getCapacity()-getFillLevel())/net;
        return -getFillLevel()/net;
    }
    public double getChargePercent(){
        return getFillLevel()/getCapacity();
    }
    public double getCapacity(){
        double capacity = 0;
        foreach(IMyTerminalBlock drive in drives){
            IMyJumpDrive d = drive as IMyJumpDrive;
            capacity += getCapacity(d);
        }
        return capacity;
    }
    public double getFillLevel(){
        double fillLevel = 0;
        foreach(IMyTerminalBlock drive in drives){
            IMyJumpDrive d = drive as IMyJumpDrive;
            fillLevel += getFillLevel(d);
        }
        return fillLevel;
    }
    double getFillLevel(IMyJumpDrive drive){
        if(drive==null||!drive.IsFunctional) return 0;
        return drive.CurrentStoredPower*1000000;
    }
    double getCapacity(IMyJumpDrive drive){
        if(drive==null||!drive.IsFunctional) return 0;
        return drive.MaxStoredPower*1000000;
    }
}