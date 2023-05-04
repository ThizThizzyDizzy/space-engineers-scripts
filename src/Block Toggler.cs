const int rebuildTime = 6;
int rebuildTimer = 0;
List<IMyFunctionalBlock> toggleables = new List<IMyFunctionalBlock>();
String errors;
GasTank hydrogen;
GasTank oxygen;
Battery battery;
public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
    hydrogen = new GasTank(this, "Hydrogen");
    oxygen = new GasTank(this, "Oxygen");
    battery = new Battery(this);
    rebuild();
}
public void Main(){
    rebuildTimer++;
    if(rebuildTimer>rebuildTime){
        rebuild();
        rebuildTimer-=rebuildTime;
        return;
    }
    errors = "";
    foreach(IMyFunctionalBlock block in toggleables){
        if(!block.CustomData.StartsWith("Toggle\n"))continue;
        String data = block.CustomData.Substring(7);
        bool shouldBeOn = true;
        while(true){
            String condition = data.Contains("\n")?data.Substring(0, data.IndexOf("\n")):data;
            try{
                bool conditionMet = false;
                if(condition.StartsWith("Power ")){
                    float threshold = Single.Parse(condition.Substring(6));
                    if(threshold<0){
                        conditionMet = battery.getChargePercent()<-threshold;
                    }else{
                        conditionMet = battery.getChargePercent()>threshold;
                    }
                }
                if(condition.StartsWith("O2 ")){
                    float threshold = Single.Parse(condition.Substring(3));
                    if(threshold<0){
                        conditionMet = oxygen.getFillPercent()<-threshold;
                    }else{
                        conditionMet = oxygen.getFillPercent()>threshold;
                    }
                }
                if(condition.StartsWith("H2 ")){
                    float threshold = Single.Parse(condition.Substring(3));
                    if(threshold<0){
                        conditionMet = hydrogen.getFillPercent()<-threshold;
                    }else{
                        conditionMet = hydrogen.getFillPercent()>threshold;
                    }
                }
                if(condition.StartsWith("OH ")){
                    float threshold = Single.Parse(condition.Substring(3));
                    double percent = (hydrogen.getFillPercent()+oxygen.getFillPercent())/2;
                    if(threshold<0){
                        conditionMet = percent<-threshold;
                    }else{
                        conditionMet = percent>threshold;
                    }
                }
                if(!conditionMet)shouldBeOn = false;
            }catch{
                error("Failed to parse condition "+condition+" in block "+block.CustomName);
            }
            if(data==condition)break;
            data = data.Substring(condition.Length+1);
        }
        error(block.CustomName+" "+(shouldBeOn?"ON":"OFF"));
        block.Enabled = shouldBeOn;
    }
    Echo(errors==""?"":errors);
    Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
    Me.GetSurface(0).WriteText(errors==""?"":errors, false);
    Me.GetSurface(0).FontSize = .9f;
}
void rebuild(){
    battery.refresh();
    oxygen.refresh();
    hydrogen.refresh();
    errors = "";
    toggleables.Clear();
    List<IMyTerminalBlock> lst = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(lst);
    foreach(IMyTerminalBlock b in lst){
        if(b.CubeGrid!=Me.CubeGrid)continue;
        if(b as IMyFunctionalBlock==null)continue;
        if(!b.IsFunctional)continue;
        if(b.CustomData.StartsWith("Toggle\n"))toggleables.Add(b as IMyFunctionalBlock);
    }
}
void error(String error){
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