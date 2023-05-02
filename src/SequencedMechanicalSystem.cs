public List<Sequence> sequences = new List<Sequence>();
public Sequence currentSequence;
public bool useCustomData = true;
public int initialize = 0;
public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
}
public bool Init(int i) {
    return false;
}
public void newSequence(String name){
    currentSequence = new Sequence(name, this);
    sequences.Add(currentSequence);
}
public void addRotor(String block, float target){
    addRotor(block, target, 30);
}
public void addRotor(String block, float target, float speed){
    addRotor(block, target, speed, 30);
}
public void addRotor(String block, float target, float speed, float easing){
    addSequenceStep(new RotorSequenceStep(this, block, target*(float)Math.PI/180, speed*(float)Math.PI/180, easing*(float)Math.PI/180));
}
public void addRotorStop(string rotor)
{
    addSequenceStep(new RotorStopSequenceStep(this, rotor));
}
public void addPiston(String block, float target){
    addPiston(block, target, 10);
}
public void addPiston(String block, float target, float speed){
    addPiston(block, target, speed, 0.25f);
}
public void addPiston(String block, float target, float speed, float easing){
    addSequenceStep(new PistonSequenceStep(this, block, target, speed, easing));
}
public void addDoor(String block, bool open){
    addSequenceStep(new DoorSequenceStep(this, block, open));
}
public void addMerge(String block){
    addSequenceStep(new MergeSequenceStep(this, block));
}
public void addConnector(String block, bool connect){
    addSequenceStep(new ConnectorSequenceStep(this, block, connect));
}
public void addToggle(String block, bool enable){
    addSequenceStep(new BlockToggleSequenceStep(this, block, enable));
}
public void addDelay(int delay){
    addSequenceStep(new DelayStep(this, delay));
}
public void addParallel(){
    addSequenceStep(new ParallelStep(this));
}
public void addRepeat(){
    addSequenceStep(new RepeatStep(this, currentSequence));
}
public void endParallel(){
    if(currentSequence.steps.Count==0){
        return;
    }
    SequenceStep last = currentSequence.steps[currentSequence.steps.Count-1];
    if(last is ParallelStep&&((ParallelStep)last).building){
        while(last is ParallelStep&&((ParallelStep)last).building){
            if(((ParallelStep)last).substeps.Count==0){
                ((ParallelStep)last).building = false;
                return;
            }
            SequenceStep s = ((ParallelStep)last).substeps[((ParallelStep)last).substeps.Count-1];
            if(s is ParallelStep&&((ParallelStep)s).building){
                last = s;
            }else break;
        }
        ((ParallelStep)last).building = false;
    }
}
public void addSequenceStep(SequenceStep step){
    if(currentSequence.steps.Count==0){
        currentSequence.steps.Add(step);
        return;
    }
    SequenceStep last = currentSequence.steps[currentSequence.steps.Count-1];
    if(last is ParallelStep&&((ParallelStep)last).building){
        while(last is ParallelStep&&((ParallelStep)last).building){
            if(((ParallelStep)last).substeps.Count==0){
                ((ParallelStep)last).substeps.Add(step);
                return;
            }
            SequenceStep s = ((ParallelStep)last).substeps[((ParallelStep)last).substeps.Count-1];
            if(s is ParallelStep&&((ParallelStep)s).building){
                last = s;
            }else break;
        }
        ((ParallelStep)last).substeps.Add(step);
    }else currentSequence.steps.Add(step);
}
public void Main(String arg){
    if(Init(initialize)){
        initialize++;
        Echo("Initializing... ("+initialize+")");
        return;
    }
    if(arg=="Break"){
        foreach(Sequence s in sequences){
            s.breakRepeat = true;
        }
    }
    if(arg=="Reset"){
        foreach(Sequence s in sequences){
            s.breakRepeat = false;
        }
    }
    foreach(Sequence s in sequences){
        if(s.step!=-1){
            Echo("Running "+s.name);
            s.process();
            if(s.isFinished()){
                s.step = -1;
                Echo("Finished!");
            }
        }
        if(s.name==arg){
            s.start();
            Echo("Started Sequence "+arg);
        }
    }
}
public class Sequence{
    public float tolerance = 0.01f;
    public string name;
    public int step = -1;
    public bool breakRepeat = false;
    public List<SequenceStep> steps = new List<SequenceStep>();
    public Program p;
    public Sequence(String name, Program p){
        this.name = name;
        this.p = p;
    }
    public void start(){
        step = 0;
        if(steps.Count>0)steps[0].start();
    }
    public void process(){
        if(step==-1)return;
        p.Echo("Step "+(step+1)+"/"+steps.Count);
        if(steps.Count>step){
            SequenceStep s = steps[step];
            s.process();
            p.Echo("Progress: "+Math.Round(s.getProgress()*100)+"%");
            if(s.isFinished()){
                s.finish();
                step++;
                if(steps.Count>step)steps[step].start();
            }
        }
    }
    public bool isFinished(){
        return step==steps.Count;
    }
}
public abstract class SequenceStep{
    public float tolerance = 0.001f;
    public bool isFinished(){
        return getProgress()>=1-tolerance;
    }
    public abstract float getProgress();
    public virtual void start(){}
    public abstract void process();
    public virtual void finish(){}
}
public class ParallelStep : SequenceStep{
    public List<SequenceStep> substeps = new List<SequenceStep>();
    public bool building = true;
    public Program p;
    public ParallelStep(Program p){
        this.p = p;
    }
    public override float getProgress(){
        float prog = 0;
        foreach(SequenceStep step in substeps)prog+=step.getProgress();
        return prog/substeps.Count;
    }
    public override void start(){
        foreach(SequenceStep step in substeps)step.start();
    }
    public override void process(){
        int done = 0;
        foreach(SequenceStep step in substeps){
            if(step.isFinished())done++;
        }
        p.Echo(done+"/"+substeps.Count+" Parallel steps:");
        foreach(SequenceStep step in substeps){
            if(step.isFinished()){
                step.finish();
            }else step.process();
            p.Echo("Progress: "+Math.Round(step.getProgress()*100)+"%");
        }
    }
    public override void finish(){
        foreach(SequenceStep step in substeps)step.finish();
    }
}
public class RotorSequenceStep : SequenceStep{
    private float toleran = 0.001f;
    public IMyMotorStator rotor;
    public float target;
    public float vel;
    public float easing;
    public float startAngle;
    public Program p;
    public RotorSequenceStep(Program p, String name, float targetAngle, float velocity, float easingSize){
        this.p = p;
        rotor = p.findBlock(name) as IMyMotorStator;
        target = targetAngle;
        vel = velocity;
        easing = easingSize;
    }
    public float getAngle(){
        return rotor.Angle;
    }
    public override float getProgress(){
        float dist = Math.Abs(startAngle-target);
        float diff = Math.Abs(getAngle()-target);
        if(diff<toleran)return 1;
        return 1-(diff/dist);
    }
    public override void start(){
        startAngle = rotor.Angle;
    }
    public override void process(){
        p.Echo((p.useCustomData?rotor.CustomData:rotor.CustomName)+": "+dr(startAngle)+" > "+dr(getAngle())+" > "+dr(target));
        float val = p.easeCurve(startAngle, target, easing, getAngle(), 0.01f)*vel*10;
        if(rotor.TargetVelocityRad!=val){
            rotor.TargetVelocityRad = val;
            p.Echo("Adjusting Velocity to "+dr(val));
        }
    }
    public override void finish(){
        rotor.TargetVelocityRad = 0;
    }
    float dr(float f){
        return (float)Math.Round(f*180/Math.PI);
    }
}
public class RotorStopSequenceStep : SequenceStep
{
    private float toleran = 0.001f;
    public IMyMotorStator rotor;
    public Program p;
    public float startAngle, lastAngle;
    public float startVel, vel;
    public bool isFirstStep = true;
    public RotorStopSequenceStep(Program p, String name)
    {
        this.p = p;
        rotor = p.findBlock(name) as IMyMotorStator;
    }
    public float getAngle()
    {
        return rotor.Angle;
    }
    public override float getProgress()
    {
        if (isFirstStep) return 0;
        if (Math.Abs(vel) < toleran) return 1;
        return 1-(Math.Abs(vel) / Math.Abs(startVel));
    }
    public override void start()
    {
        isFirstStep = true;
        lastAngle = startAngle = getAngle();
    }
    public override void process()
    {
        vel = getAngle() - lastAngle;
        if (isFirstStep) startVel = vel;
        isFirstStep = false;
        lastAngle = getAngle();
        p.Echo("Stopping "+(p.useCustomData ? rotor.CustomData : rotor.CustomName) + ": " + dr(vel)*10 + " RPM");
    }
    float dr(float f)
    {
        return (float)Math.Round(f * 18000 / Math.PI)/100f;
    }
}
public class PistonSequenceStep : SequenceStep{
    private float toleran = 0.001f;
    public IMyPistonBase piston;
    public float target;
    public float speed;
    public float easing;
    public float startPos;
    public Program p;
    public PistonSequenceStep(Program p, String name, float targetExtent, float speed, float easingSize){
        this.p = p;
        piston = p.findBlock(name) as IMyPistonBase;
        target = targetExtent;
        this.speed = speed;
        easing = easingSize;
    }
    public override float getProgress(){
        float dist = Math.Abs(startPos-target);
        float diff = Math.Abs(piston.CurrentPosition-target);
        if(diff<toleran)return 1;
        return 1-(diff/dist);
    }
    public override void start(){
        startPos = piston.CurrentPosition;
    }
    public override void process(){
        p.Echo((p.useCustomData?piston.CustomData:piston.CustomName)+": "+r(startPos)+" > "+r(piston.CurrentPosition)+" > "+r(target));
        float val = p.easeCurve(startPos, target, easing, piston.CurrentPosition, 0.02f/speed, 0.05f)*speed;
        if(piston.Velocity!=val){
            piston.Velocity = val;
            p.Echo("Adjusting Velocity to "+r(val));
        }
    }
    public override void finish(){
        piston.Velocity = 0;
    }
    float r(float f){
        return (float)Math.Round(f*10)/10f;
    }
}
public class DoorSequenceStep : SequenceStep{
    public IMyDoor door;
    public bool open;
    public Program p;
    public DoorSequenceStep(Program p, String name, bool open){
        this.p = p;
        door = p.findBlock(name) as IMyDoor;
        this.open = open;
    }
    public override float getProgress(){//could do a real value but whatever
        if(open){
            if(door.Status==DoorStatus.Open)return 1;
            if(door.Status==DoorStatus.Opening)return 0.5f;
        }else{
            if(door.Status==DoorStatus.Closed)return 1;
            if(door.Status==DoorStatus.Closing)return 0.5f;
        }
        return 0f;
    }
    public override void process(){
        p.Echo((p.useCustomData?door.CustomData:door.CustomName)+": "+door.Status.ToString());
        if(open)door.OpenDoor();
        else door.CloseDoor();
    }
}
public class ConnectorSequenceStep : SequenceStep{
    public IMyShipConnector connector;
    public bool connect;
    public Program p;
    public ConnectorSequenceStep(Program p, String name, bool connect){
        this.p = p;
        connector = p.findBlock(name) as IMyShipConnector;
        this.connect = connect;
    }
    public override float getProgress(){//could do a real value but whatever
        if(connect){
            if(connector.Status==MyShipConnectorStatus.Connected)return 1;
            if(connector.Status==MyShipConnectorStatus.Connectable)return 0.5f;
        }else{
            if(connector.Status==MyShipConnectorStatus.Unconnected||connector.Status==MyShipConnectorStatus.Connectable)return 1f;
        }
        return 0f;
    }
    public override void process(){
        p.Echo((p.useCustomData?connector.CustomData:connector.CustomName)+": "+connector.Status.ToString());
        if(connect&&connector.Status==MyShipConnectorStatus.Connectable)connector.Connect();
        if(!connect)connector.Disconnect();
    }
}
public class MergeSequenceStep : SequenceStep{
    public IMyShipMergeBlock merge;
    public Program p;
    public MergeSequenceStep(Program p, String name){
        this.p = p;
        merge = p.findBlock(name) as IMyShipMergeBlock;
    }
    public override float getProgress(){
        return merge.IsConnected?1:0;
    }
    public override void process(){
        p.Echo((p.useCustomData?merge.CustomData:merge.CustomName)+": "+(merge.IsConnected?"Merged!":"Waiting..."));
    }
}
public class BlockToggleSequenceStep : SequenceStep{
    public IMyFunctionalBlock block;
    public bool enable;
    public Program p;
    public BlockToggleSequenceStep(Program p, String name, bool enable){
        this.p = p;
        block = p.findBlock(name) as IMyFunctionalBlock;
        this.enable = enable;
    }
    public override float getProgress(){
        return block.Enabled==enable?1:0;
    }
    public override void process(){
        block.Enabled = enable;
    }
}
public class DelayStep : SequenceStep{
    public int delay;
    public int timer;
    public Program p;
    public DelayStep(Program p, int delay){
        this.p = p;
        this.delay = delay;
    }
    public override float getProgress(){
        return timer/(float)delay;
    }
    public override void start(){
        timer = 0;
    }
    public override void process(){
        p.Echo("Delay: "+timer+"/"+delay);
        timer++;
    }
}
public class RepeatStep : SequenceStep{
    public Program p;
    public Sequence sequence;
    public RepeatStep(Program p, Sequence sequence){
        this.p = p;
        this.sequence = sequence;
    }
    public override float getProgress(){
        return 1;
    }
    public override void start(){
        if(sequence.breakRepeat)return;
        sequence.step = 0;
        sequence.start();
    }
    public override void process(){}
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