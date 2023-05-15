public bool useCustomData = true;
public IMySensorBlock sensor;
public IMyGravityGenerator gravityGen;
public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    sensor = findBlock("Gravity Lab Sensor") as IMySensorBlock;
    gravityGen = findBlock("Gravity Lab Generator") as IMyGravityGenerator;
}
Vector3 lastVel = Vector3.Zero;
public void Main(String arg){
    Vector3 vel = sensor.LastDetectedEntity.Velocity;
    Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
    Me.GetSurface(0).WriteText("Acceleration\n"+(vel-lastVel).Length(), false);
    lastVel = vel;
}
public IMyTerminalBlock findBlock(String name){
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(blocks);
    foreach(IMyTerminalBlock b in blocks){
        if(useCustomData){
            if(name.Equals(b.CustomData))return b;
        }else{
            if(name.Equals(b.CustomName))return b;
        }
    }
    Echo("Could not find block "+name);
    return null;
}
public List<IMyTerminalBlock> findBlocks(String name){
    List<IMyTerminalBlock> found = new List<IMyTerminalBlock>();
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(blocks);
    foreach(IMyTerminalBlock b in blocks){
        if(useCustomData){
            if(name.Equals(b.CustomData))found.Add(b);
        }else{
            if(name.Equals(b.CustomName))found.Add(b);
        }
    }
    return found;
}
public float easeCurve(float x1, float x2, float ease, float x, float min){
    return easeCurve(x1, x2, ease, x, min, min);
}
public float easeCurve(float x1, float x2, float ease, float x, float goodMin, float badMin){
    if(x1>x2)return -easeCurve(-x1, -x2, ease, -x, goodMin, badMin);
    if(ease==0)return x>x2?-badMin:1;
    ease = Math.Min(ease, Math.Abs(x1-x2)/2);
    double minVal = Math.Max(-badMin, Math.Min(badMin, -badMin*((x-(x1+x2)/2)/(Math.Abs(x1-x2)/2))));
    double eV1 = Math.Min(-Math.Abs((x-(x1+ease))/ease)+1.3, Math.Sin((Math.PI*(x-x1))/(ease*2)));
    double eV2 = Math.Min(-Math.Abs((x-(x2-ease))/ease)+1.3, -Math.Sin((Math.PI*(x-x2))/(ease*2)));
    double val = Math.Min(1, Math.Min((x-x1)/ease, (x2-x)/ease));
    if(Math.Abs(val)<goodMin){
        if(val<0)val = -goodMin;
        else val = goodMin;
    }
    double final = Math.Max(Math.Max(minVal, eV1), Math.Max(eV2, val));
    return (float)Math.Max(-1, Math.Min(1, final));//sanity check
}