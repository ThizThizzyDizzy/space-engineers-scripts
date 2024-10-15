const String name = "Drill A";
const int rebuildTime = 60;
int rebuildTimer = 0;
List<IMyPistonBase> pistons = new List<IMyPistonBase>();
List<IMyFunctionalBlock> drills = new List<IMyFunctionalBlock>();
String errors;
GasTank hydrogen;
GasTank oxygen;
Battery battery;
InventoryOverview inv;
double percentMined;
public Program(){
    try{
        percentMined = Double.Parse(Storage);
    }catch{percentMined = 0;}
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    hydrogen = new GasTank(this, "Hydrogen");
    oxygen = new GasTank(this, "Oxygen");
    battery = new Battery(this);
    inv = new InventoryOverview(this);
    rebuild();
}
public void Save(){
    Storage = percentMined+"";
}
public void Main(){
    rebuildTimer++;
    if(rebuildTimer>rebuildTime){
        rebuild();
        rebuildTimer-=rebuildTime;
        return;
    }
    hydrogen.tick();
    oxygen.tick();
    errors = "";
    bool drilling = inv.getFillPercent()<=.9995;
    float current = 0;
    float max = 0;
    float speed = 0;
    foreach(IMyPistonBase piston in pistons){
        current+=piston.CurrentPosition;
        max+=piston.HighestPosition;
        speed+=piston.Velocity;
    }
    percentMined = Math.Max(current/max, percentMined);
    double end = max*percentMined;
    double diff = end-current;
    if(!drilling){
        error("Drill disabled");
        foreach(IMyPistonBase piston in pistons)piston.Velocity = -.005f;
        foreach(IMyFunctionalBlock drill in drills)drill.Enabled = false;
    }else{
        error("Drill enabled");
        foreach(IMyPistonBase piston in pistons){
            piston.Velocity = diff<1?.001f:.005f;
        }
        foreach(IMyFunctionalBlock drill in drills)drill.Enabled = true;
    }
    double time = drilling?diff/speed:-current/speed;
    error("Hydrogen: "+makeReadable(hydrogen.getFillPercent()*100)+"%; "+makeReadableTime(hydrogen.getSecondsUntilCharged()));
    error("Cargo: "+makeReadable(inv.getFillPercent()*100)+"%");
    error("Mining progress: "+makeReadable(percentMined*100)+"% ("+makeReadable(percentMined*max)+"/"+makeReadable(max)+")");
    error("Current Position: "+makeReadable(current/max*100)+"% ("+makeReadable(current)+")");
    error("Speed: "+Math.Round(speed*1000)/1000d+" m/s");
    if(drilling){
        if(diff<.01)error("Done mining in: "+makeReadableTime((max-current)/speed));
        else error("Reaching mine floor in: "+makeReadableTime(time));
    }
    else error("Reaching top in: "+makeReadableTime(time));
    Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
    Me.GetSurface(0).WriteText(errors==""?"":errors, false);
    Me.GetSurface(0).FontSize = .9f;
}
String makeReadable(double number){
    long n = (long)Math.Round(number*10);
    if(n==0)return "0";
    String num = n+"";
    num = num.Insert(num.Length-1, ".");
    int i = num.Length-2;
    while(i>(n<0?4:3)){
        i-=3;
        num = num.Insert(i, ",");
    }
    return num;
}
String makeReadableTime(double seconds){
    double number = seconds;
    String suffix = "s";
    if(number>60){
        number/=60;
        suffix = "m";
        if(number>60){
            number/=60;
            suffix = "h";
            if(number>24){
                number/=24;
                suffix = "d";
                if(number>30){
                    number/=30;
                    suffix = "mo";
                    if(number>12){
                        number/=12;
                        suffix = "y";
                    }
                }
            }
        }
    }
    number = Math.Round(number*10)/10d;
    return number+suffix;
}
void rebuild(){
    battery.refresh();
    oxygen.refresh();
    hydrogen.refresh();
    inv.refresh();
    drills.Clear();
    pistons.Clear();
    List<IMyTerminalBlock> lst = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(lst);
    foreach(IMyTerminalBlock b in lst){
        if(!b.IsFunctional)continue;
        if(!b.CustomData.Equals(name))continue;
        if(b as IMyPistonBase!=null)pistons.Add(b as IMyPistonBase);
        else drills.Add(b as IMyFunctionalBlock);
    }
}
void error(String error){
    Echo(error);
    if(errors=="")errors = error;
    else errors+="\n"+error;
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
            if(!b.IsFunctional)continue;
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
        if(tank==null||!tank.Enabled) return 0;
        return tank.Capacity*tank.FilledRatio;
    }
    double getCapacity(IMyGasTank tank){
        if(tank==null) return 0;
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