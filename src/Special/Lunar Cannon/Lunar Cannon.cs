// Built on top of SMS (Sequenced Mechanical System)
public List<Sequence> sequences = new List<Sequence>();
public List<Sequence> removeSequences = new List<Sequence>();
public Sequence currentSequence;
public bool useCustomData = true;
public int initialize = 0;
public ConditionStep currentCondition;
public Dictionary<String, object> variables = new Dictionary<String, object>();
public Vector2 earthCenter = new Vector2(4.666443f, 4.432678f);
public Vector2 vertical = new Vector2(6.875f, 6.875f);
public Vector2 searchedPosition = Vector2.Zero;
public int searchStep = 0;
public int searchSubstep = 0;
public bool positionFound = true;
public Vector3 vec_xy;
public Vector3 vec_Xy;
public Vector3 vec_XY;
public Vector3 vec_xY;
public IMyShipController controller;
IMyBroadcastListener listener;
String igcTagAssist = "Lunar Gravity Assist";
String igcTagCannon = "Lunar Cannon";
public float speedLimit = 100f; //this script will do intended payloads only, no free rides >:[
public List<GravityField> gravityGenerators = new List<GravityField>();
public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    variables["Done"] = true;
    variables["Aligned"] = false;
    variables["Ready"] = false;
    controller = findBlock("Lunar Remote") as IMyShipController;
    listener = IGC.RegisterBroadcastListener(igcTagCannon);
}
public bool Init(int i) {
    if(i==0){
        newSequence("Zoom");
        addParallel();
        addToggle("Gravity Generator 1A", true);
        addToggle("Gravity Generator 1B", true);
        endParallel();
        addParallel();
        addToggle("Gravity Generator 2A", true);
        addToggle("Gravity Generator 2B", true);
        endParallel();
        addParallel();
        addToggle("Gravity Generator 3A", true);
        addToggle("Gravity Generator 3B", true);
        endParallel();
        addParallel();
        addToggle("Gravity Generator 4A", true);
        addToggle("Gravity Generator 4B", true);
        endParallel();
        addParallel();
        addToggle("Gravity Generator 5A", true);
        addToggle("Gravity Generator 5B", true);
        endParallel();
        addDelay(300);
        addParallel();
        addToggle("Gravity Generator 1A", false);
        addToggle("Gravity Generator 1B", false);
        endParallel();
        addParallel();
        addToggle("Gravity Generator 2A", false);
        addToggle("Gravity Generator 2B", false);
        endParallel();
        addParallel();
        addToggle("Gravity Generator 3A", false);
        addToggle("Gravity Generator 3B", false);
        endParallel();
        addParallel();
        addToggle("Gravity Generator 4A", false);
        addToggle("Gravity Generator 4B", false);
        endParallel();
        addParallel();
        addToggle("Gravity Generator 5A", false);
        addToggle("Gravity Generator 5B", false);
        endParallel();
        addToggle("Lunar Cannon Maglock", false);
        return true;
    }
    if(i==1){
        newSequence("Launch");
        setVar("Ready", false);
        waitVar("Aligned", true);
        addSequenceStep(new GravityAssistCalculationStep(this));
        waitVar("Ready", true);
        addDelay(300);
        runSequence("Zoom");
        addDelay(600);
        addSequenceStep(new ResetGravityAssistStep(this));
        return true;
    }
    if(i==2){
        newSequence("GOTO EARTH");
        setVar("Aligned", false);
        addParallel();
        addPiston("Piston X1", earthCenter.X, 0.1f, 200);
        addPiston("Piston X2", earthCenter.X, 0.1f, 200);
        addPiston("Piston Y1", earthCenter.Y, 0.1f, 200);
        addPiston("Piston Y2", earthCenter.Y, 0.1f, 200);
        endParallel();
        setVar("Done", true);
        setVar("Aligned", true);
        newSequence("GOTO CENTER");
        setVar("Aligned", false);
        addParallel();
        addPiston("Piston X1", vertical.X, 0.1f, 200);
        addPiston("Piston X2", vertical.X, 0.1f, 200);
        addPiston("Piston Y1", vertical.Y, 0.1f, 200);
        addPiston("Piston Y2", vertical.Y, 0.1f, 200);
        endParallel();
        setVar("Done", true);
        setVar("Aligned", true);
        return true;
    }
    if(i==3){
        newSequence("Ready Payload");
        runSequence("GOTO CENTER");
        //bring the payload arm down to pick up the payload
        addRotor("Payload Arm Hinge 2", -90, 1f, 2);
        addRotor("Payload Arm Hinge 3", 0, 1f, 2);
        addParallel();
        addPiston("Payload Arm Piston 4", 5, 0.8f, 0.5f);
        addPiston("Payload Arm Piston 5", 5, 0.8f, 0.5f);
        addRotor("Payload Arm Rotor", 180, 15, 2);
        addRotor("Payload Storage Rotor", 180, 2, 10);
        endParallel();
        addLandingGear("Payload Arm Maglock", true);
        addToggle("Payload Storage Merge Block", false);
        addParallel();
        addPiston("Payload Arm Piston 4", 2.5f, 0.8f, 0.5f);
        addPiston("Payload Arm Piston 5", 2.5f, 0.8f, 0.5f);
        endParallel();
        addRotor("Payload Arm Rotor", 90, 2, 30);
        addParallel();
        addPiston("Payload Arm Piston 4", 0, 0.8f, 0.5f);
        addPiston("Payload Arm Piston 5", 0, 0.8f, 0.5f);
        addHinge("Payload Arm Hinge 3", 60, 1f, 2);
        endParallel();
        addToggle("Payload Storage Merge Block", true);
        //open door
        addParallel();
        addHinge("Lunar Payload Door 1", -10f, 10);
        addHinge("Lunar Payload Door 2", -10f, 10);
        addHinge("Lunar Payload Door 3", -10f, 10);
        addHinge("Lunar Payload Door 4", -10f, 10);
        addHinge("Lunar Payload Door 5", -10f, 10);
        addDoors("Payload Arm Door", true);
        endParallel();
        //arm up
        addRotor("Payload Arm Hinge 2", 0, 1f, 2);
        addHinge("Payload Arm Hinge 1", 60, 0.5f, 2);
        addRotor("Payload Arm Rotor", 270, 2, 10);
        addParallel();
        addHinge("Payload Arm Hinge 1", 90, 0.5f, 2);
        addPiston("Payload Arm Piston 1", 8.5f, 2f, 2);
        addPiston("Payload Arm Piston 2", 8.5f, 2f, 2);
        addPiston("Payload Arm Piston 3", 8.5f, 2f, 2);
        addHinge("Payload Arm Hinge 2", 90, 1f, 2);
        addHinge("Payload Arm Hinge 3", 0, 1f, 2);
        endParallel();
        addParallel();
        addPiston("Payload Arm Piston 4", 8, 0.8f, 0.5f);
        addPiston("Payload Arm Piston 5", 8, 0.8f, 0.5f);
        endParallel();
        addParallel();
        addPiston("Payload Arm Piston 1", 6.85f, 0.4f, 0.2f);
        addPiston("Payload Arm Piston 2", 6.85f, 0.4f, 0.2f);
        addPiston("Payload Arm Piston 3", 6.85f, 0.4f, 0.2f);
        endParallel();
        //release payload
        addLandingGear("Lunar Cannon Maglock", true);
        addLandingGear("Payload Arm Maglock", false);
        //arm down
        addParallel();
        addPiston("Payload Arm Piston 4", 0, 2f, 1);
        addPiston("Payload Arm Piston 5", 0, 2f, 1);
        addHinge("Payload Arm Hinge 2", 0, 1.5f, 3);
        addHinge("Payload Arm Hinge 3", 0, 1f, 2);
        endParallel();
        addParallel();
        addRotor("Payload Arm Rotor", 90, 15f, 2);
        addPiston("Payload Arm Piston 1", 0, 2f, 0.8f);
        addPiston("Payload Arm Piston 2", 0, 2f, 0.8f);
        addPiston("Payload Arm Piston 3", 0, 2f, 0.8f);
        endParallel();
        addHinge("Payload Arm Hinge 1", 0, 1f, 1);
        //close door
        addParallel();
        addHinge("Lunar Payload Door 1", 90f, 10);
        addHinge("Lunar Payload Door 2", 90f, 10);
        addHinge("Lunar Payload Door 3", 90f, 10);
        addHinge("Lunar Payload Door 4", 90f, 10);
        addHinge("Lunar Payload Door 5", 90f, 10);
        addDoors("Payload Arm Door", false);
        endParallel();
        return true;
    }
    if(i==4){
        newSequence("Stow Payload");
        runSequence("GOTO CENTER");
        //open door
        addParallel();
        addHinge("Lunar Payload Door 1", -10f, 10);
        addHinge("Lunar Payload Door 2", -10f, 10);
        addHinge("Lunar Payload Door 3", -10f, 10);
        addHinge("Lunar Payload Door 4", -10f, 10);
        addHinge("Lunar Payload Door 5", -10f, 10);
        addDoors("Payload Arm Door", true);
        endParallel();
        //arm up
        addHinge("Payload Arm Hinge 1", 90, 1, 1);
        addParallel();
        addPiston("Payload Arm Piston 1", 6.85f, 2f, 2);
        addPiston("Payload Arm Piston 2", 6.85f, 2f, 2);
        addPiston("Payload Arm Piston 3", 6.85f, 2f, 2);
        addRotor("Payload Arm Rotor", 270, 15f, 5);
        addHinge("Payload Arm Hinge 2", 90, 1.5f, 4);
        addHinge("Payload Arm Hinge 3", 0, 1f, 2);
        addPiston("Payload Arm Piston 4", 8, 0.8f, 0.6f);
        addPiston("Payload Arm Piston 5", 8, 0.8f, 0.6f);
        endParallel();
        //Grab payload
        addLandingGear("Payload Arm Maglock", true);
        addLandingGear("Lunar Cannon Maglock", false);
        //arm down
        addParallel();
        addPiston("Payload Arm Piston 1", 8.5f, 0.4f, 0.2f);
        addPiston("Payload Arm Piston 2", 8.5f, 0.4f, 0.2f);
        addPiston("Payload Arm Piston 3", 8.5f, 0.4f, 0.2f);
        endParallel();
        addParallel();
        addPiston("Payload Arm Piston 4", 0, 0.8f, 0.5f);
        addPiston("Payload Arm Piston 5", 0, 0.8f, 0.5f);
        endParallel();
        addParallel();
        addHinge("Payload Arm Hinge 1", 90, 0.5f, 1.5f);
        addPiston("Payload Arm Piston 1", 0, 2f, 2);
        addPiston("Payload Arm Piston 2", 0, 2f, 2);
        addPiston("Payload Arm Piston 3", 0, 2f, 2);
        addHinge("Payload Arm Hinge 2", 0, 1f, 2);
        addHinge("Payload Arm Hinge 3", 0, 1f, 2);
        endParallel();
        addParallel();
        addRotor("Payload Arm Rotor", 90, 2, 10);
        addHinge("Payload Arm Hinge 1", 0, 0.5f, 2);
        addHinge("Payload Arm Hinge 3", 60, 1, 2);
        endParallel();
        addRotor("Payload Arm Hinge 2", -90, 1f, 2);
        //close door
        addParallel();
        addHinge("Lunar Payload Door 1", 90f, 10);
        addHinge("Lunar Payload Door 2", 90f, 10);
        addHinge("Lunar Payload Door 3", 90f, 10);
        addHinge("Lunar Payload Door 4", 90f, 10);
        addHinge("Lunar Payload Door 5", 90f, 10);
        addDoors("Payload Arm Door", false);
        endParallel();
        //bring the payload arm down to store the payload
        addParallel();
        addPiston("Payload Arm Piston 4", 2.5f, 0.8f, 0.5f);
        addPiston("Payload Arm Piston 5", 2.5f, 0.8f, 0.5f);
        addHinge("Payload Arm Hinge 3", 0, 1, 2);
        endParallel();
        addRotor("Payload Arm Rotor", 180, 2, 30);
        addParallel();
        addPiston("Payload Arm Piston 4", 5f, 0.8f, 0.5f);
        addPiston("Payload Arm Piston 5", 5f, 0.8f, 0.5f);
        addRotor("Payload Storage Rotor", 180, 2, 10);
        endParallel();
        addToggle("Payload Storage Merge Block", true);
        addMerge("Payload Storage Merge Block");
        addLandingGear("Payload Arm Maglock", false);
        addParallel();
        addPiston("Payload Arm Piston 4", 0, 0.8f, 0.5f);
        addPiston("Payload Arm Piston 5", 0, 0.8f, 0.5f);
        endParallel();
        addRotor("Payload Arm Hinge 2", 0, 1f, 2);
        return true;
    }
    return false;
}
public void Run(){
    // if(findCenter==Vector2.Zero&&searchPosition("Target", new Vector3(14280-2104, 131867-4517, -105618+7997))){
    //     findCenter = searchedPosition;
    // }
    // Echo("Found: "+findCenter.ToString());
}
public bool searchPosition(String name, Vector3 target){
    if(positionFound){
        positionFound = false;
        searchedPosition = new Vector2(5f, 5f);
        searchStep = 0;
        searchSubstep = -1;
    }
    Echo("Searching for position: "+name);
    float variance = (float)(5/Math.Pow(2, searchStep));//maximum variation in each cardinal direction; binary search should move in increments of half this much
    if(checkVar("Done", false))return false;
    searchSubstep++;
    float easing = (float)Math.Min(200, Math.Pow(2, searchStep)/10+0.1f);
    switch(searchSubstep){
        case 0:
            newSequence("Search xy ("+searchStep+")");
            addParallel();
            setVar("Aligned", false);
            addPiston("Piston X1", searchedPosition.X-variance/2, variance/2, easing);
            addPiston("Piston X2", searchedPosition.X-variance/2, variance/2, easing);
            addPiston("Piston Y1", searchedPosition.Y-variance/2, variance/2, easing);
            addPiston("Piston Y2", searchedPosition.Y-variance/2, variance/2, easing);
            endParallel();
            addDelay(20);
            setVar("Done", true);
            endTempSequence();
            currentSequence.start();
            variables["Done"] = false;
            return false;
        case 1:
            vec_xy = getVector();
            newSequence("Search Xy ("+searchStep+")");
            addParallel();
            setVar("Aligned", false);
            addPiston("Piston X1", searchedPosition.X+variance/2, variance/2, easing);
            addPiston("Piston X2", searchedPosition.X+variance/2, variance/2, easing);
            addPiston("Piston Y1", searchedPosition.Y-variance/2, variance/2, easing);
            addPiston("Piston Y2", searchedPosition.Y-variance/2, variance/2, easing);
            endParallel();
            addDelay(20);
            setVar("Done", true);
            endTempSequence();
            currentSequence.start();
            variables["Done"] = false;
            return false;
        case 2:
            vec_Xy = getVector();
            newSequence("Search XY ("+searchStep+")");
            addParallel();
            setVar("Aligned", false);
            addPiston("Piston X1", searchedPosition.X+variance/2, variance/2, easing);
            addPiston("Piston X2", searchedPosition.X+variance/2, variance/2, easing);
            addPiston("Piston Y1", searchedPosition.Y+variance/2, variance/2, easing);
            addPiston("Piston Y2", searchedPosition.Y+variance/2, variance/2, easing);
            endParallel();
            addDelay(20);
            setVar("Done", true);
            endTempSequence();
            currentSequence.start();
            variables["Done"] = false;
            return false;
        case 3:
            vec_XY = getVector();
            newSequence("Search xY ("+searchStep+")");
            addParallel();
            setVar("Aligned", false);
            addPiston("Piston X1", searchedPosition.X-variance/2, variance/2, easing);
            addPiston("Piston X2", searchedPosition.X-variance/2, variance/2, easing);
            addPiston("Piston Y1", searchedPosition.Y+variance/2, variance/2, easing);
            addPiston("Piston Y2", searchedPosition.Y+variance/2, variance/2, easing);
            endParallel();
            addDelay(20);
            setVar("Done", true);
            endTempSequence();
            currentSequence.start();
            variables["Done"] = false;
            return false;
        case 4:
            vec_xY = getVector();
            Vector3 targetVec = target-controller.GetPosition();
            double angle_xy = Vector3.Angle(vec_xy, targetVec);
            double angle_xY = Vector3.Angle(vec_xY, targetVec);
            double angle_XY = Vector3.Angle(vec_XY, targetVec);
            double angle_Xy = Vector3.Angle(vec_Xy, targetVec);

            double minAngle = Math.Min(Math.Min(angle_xy,angle_xY), Math.Min(angle_XY, angle_Xy));

            if(minAngle==angle_xy){
                searchedPosition.X-=variance/2;
                searchedPosition.Y-=variance/2;
            }else if(minAngle==angle_xY){
                searchedPosition.X-=variance/2;
                searchedPosition.Y+=variance/2;
            }else if(minAngle==angle_XY){
                searchedPosition.X+=variance/2;
                searchedPosition.Y+=variance/2;
            }else if(minAngle==angle_Xy){
                searchedPosition.X+=variance/2;
                searchedPosition.Y-=variance/2;
            }

            //Hone down search!
            searchStep++;
            searchSubstep = -1;
            break;
    }
    if(searchStep>14){
        Echo("Found at "+searchedPosition.ToString());
        positionFound = true;
        return true;
    }
    return false;
}
public Vector3 getVector(){
    return controller.WorldMatrix.Forward;
}
public void newSequence(String name){
    currentSequence = new Sequence(name, this);
    sequences.Add(currentSequence);
}
public void runSequence(String name){
    Sequence seq = null;
    foreach(Sequence se in sequences){
        if(se.name==name)seq = se;
    }
    addSequenceStep(seq);
}
public void addHinge(String block, float target){
    addRotor(block, target);
}
public void addHinge(String block, float target, float speed){
    addRotor(block, target, speed);
}
public void addHinge(String block, float target, float speed, float easing){
    addRotor(block, target, speed, easing);
}
public void addHingeStop(string hinge)
{
    addRotorStop(hinge);
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
public void addDoors(String block, bool open){
    addSequenceStep(new DoorsSequenceStep(this, block, open));
}
public void addMerge(String block){
    addSequenceStep(new MergeSequenceStep(this, block));
}
public void addConnector(String block, bool connect){
    addSequenceStep(new ConnectorSequenceStep(this, block, connect));
}
public void addLandingGear(String block, bool locked){
    addSequenceStep(new LandingGearSequenceStep(this, block, locked));
}
public void addToggle(String block, bool enable){
    addSequenceStep(new BlockToggleSequenceStep(this, block, enable));
}
public void addDelay(int delay){
    addSequenceStep(new DelayStep(this, delay));
}
public void setVar(String var, object val)
{
    addSequenceStep(new SetVarStep(this, var, val));
}
public void waitVar(String var, object val)
{
    addSequenceStep(new WaitVarStep(this, var, val));
}
public void setConditional(String var){
    setConditional(var, true);
}
public void setConditional(String var, object val){
    ConditionStep condition = new ConditionStep(this, var, val);
    addSequenceStep(condition);
    currentCondition = condition;
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
public void endTempSequence(){
    addSequenceStep(new RemoveSequenceStep(this, currentSequence));
}
public void addSequenceStep(SequenceStep step){
    if(currentSequence.steps.Count==0){
        currentSequence.steps.Add(step);
        return;
    }
    if(currentCondition!=null){
        currentCondition.step = step;
        currentCondition = null;
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
    if(listener.HasPendingMessage){
        MyIGCMessage message = listener.AcceptMessage();
        if(message.Tag==igcTagCannon){
            variables[message.Data.ToString()] = true;
        }
    }
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
    foreach(String s in variables.Keys){
        Echo(s+": "+variables[s]);
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
    foreach(Sequence s in removeSequences){
        sequences.Remove(s);
    }
    removeSequences.Clear();
    Run();
}
public class Sequence : SequenceStep{
    public new float tolerance = 0.01f;
    public string name;
    public int step = -1;
    public bool breakRepeat = false;
    public List<SequenceStep> steps = new List<SequenceStep>();
    public Program p;
    public Sequence(String name, Program p){
        this.name = name;
        this.p = p;
    }
    public override void start(){
        step = 0;
        if(steps.Count>0)steps[0].start();
    }
    public override void process(){
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
    public override float getProgress(){
        if(isFinished()||step==-1)return 1;
        return (step+(step<0?0:steps[step].getProgress()))/steps.Count;
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
public class GravityAssistCalculationStep : SequenceStep{
    float timeScale = 1/60f;
    Vector3 pos = Vector3.Zero;
    Vector3 vel = Vector3.Zero;
    float startingAlt = 0;
    float alt = 0;
    Planet origin;
    Program p;
    public List<Planet> planets = new List<Planet>();
    public GravityAssistCalculationStep(Program p){
        this.p = p;
        planets.Add(new Planet("Earth", new Vector3(.5f,.5f,.5f), 60000, 9.81f, 67200, 0, 14400));
        planets.Add(new Planet("Mars", new Vector3(1031072.5f, 131072.5f, 1631072.5f), 60000, 8.829f, 67200, 0, 14400));
        planets.Add(new Planet("Alien", new Vector3(131072.5f, 131072.5f, 5731072.5f), 60000, 10.791f, 67200, 2400, 14400));
        planets.Add(new Planet("Moon", new Vector3(16384.5f, 136384.5f, -113615.5f), 9500, 2.4525f, 9785, 0, 0));
        planets.Add(new Planet("Europa", new Vector3(916384.5f, 16384.5f, 1616384.5f), 9500, 2.4525f, 10640, 0, 1140));
        planets.Add(new Planet("Titan", new Vector3(36384.5f, 226384.5f, 5796384.5f), 9500, 2.4525f, 10640, 0, 570));
        planets.Add(new Planet("Triton", new Vector3(-284463.5f, -2434463.5f, 365536.5f), 40000, 9.81f, 44800, 0, 2656));
    }
    public override float getProgress(){
        return (alt-startingAlt)/800;
    }
    float gridMass, gridArtificialMass;
    public override void start(){
        gridMass = gridArtificialMass = -1;
        p.Me.CustomData = "";
        vel = Vector3.Zero;
        pos = p.controller.GetPosition()+p.controller.WorldMatrix.Forward*2.5f;
        p.gravityGenerators.Clear();
        p.addGravityGen(p.findBlock("Gravity Generator 1A") as IMyGravityGenerator);
        p.addGravityGen(p.findBlock("Gravity Generator 1B") as IMyGravityGenerator);
        p.addGravityGen(p.findBlock("Gravity Generator 2A") as IMyGravityGenerator);
        p.addGravityGen(p.findBlock("Gravity Generator 2B") as IMyGravityGenerator);
        p.addGravityGen(p.findBlock("Gravity Generator 3A") as IMyGravityGenerator);
        p.addGravityGen(p.findBlock("Gravity Generator 3B") as IMyGravityGenerator);
        p.addGravityGen(p.findBlock("Gravity Generator 4A") as IMyGravityGenerator);
        p.addGravityGen(p.findBlock("Gravity Generator 4B") as IMyGravityGenerator);
        p.addGravityGen(p.findBlock("Gravity Generator 5A") as IMyGravityGenerator);
        p.addGravityGen(p.findBlock("Gravity Generator 5B") as IMyGravityGenerator);
        origin = null;
        float dist = -1;
        foreach(Planet pl in planets){
            if(origin==null||Vector3.Distance(pos, pl.center)<dist){
                origin = pl;
                dist = Vector3.Distance(pos, pl.center);
            }
        }
        steps = 0;
        startingAlt = dist;
    }
    int steps = 0;
    public override void process(){
        if(p.Me.CustomData==""&&gridMass==-1){
            p.Echo("Please enter Grid Mass and Artificial Mass in Custom Data");
            return;
        }else if(gridMass==-1){
            String[] strs = p.Me.CustomData.Split(' ');
            gridMass = Single.Parse(strs[0]);
            gridArtificialMass = Single.Parse(strs[1]);
            p.Me.CustomData = "";
        }
        steps++;
        pos+=vel*timeScale;
        if(vel.Length()>p.speedLimit)vel = vel.Normalized()*p.speedLimit;
        float artificialGravityMult = 1f;
        foreach(Planet pl in planets){
            Vector3 grav = pl.getGravityAt(pos);
            if(grav.Length()>0){
                artificialGravityMult = Math.Min(artificialGravityMult, 1-2*(pl.getGravityAt(Vector3.Distance(pos, pl.center))/9.81f));
            }
            vel+=grav*timeScale;//*2 for players, but we don't care about players; this isn't an amusement park ride >:[
        }
        foreach(GravityField g in p.gravityGenerators){
            if(Vector3.Distance(pos, g.pos)<g.radius){//TODO assumes spherical area... which is not correct
                vel+=g.gravity*timeScale*(gridArtificialMass/gridMass)*artificialGravityMult;
            }
        }
        alt = Vector3.Distance(pos, origin.center);
        p.Echo("Calculating Trajectory... (Step "+steps+", "+Math.Round(alt-startingAlt)+"m)");
    }
    public override void finish(){
        if(alt<=startingAlt){//don't smash the gravity assist ship into the ground plz x.x
            p.Echo("INVALID TRAJECTORY");
            return;
        }
        Vector3 facing = p.getVector();
        Vector3 adjustment = new Vector3(facing.Y, facing.Z, facing.X);//rotated 90 degrees, don't care in which direction
        p.IGC.SendBroadcastMessage(p.igcTagAssist, pos+adjustment*10);//10 meters off to the side, just hope it's enough to avoid collision lol
    }
}
public void addGravityGen(IMyGravityGenerator gen){
    gravityGenerators.Add(new GravityField(gen.GetPosition(), 80, gen.GravityAcceleration*gen.WorldMatrix.Down));//actually 75, but it makes up for treating it as spherical... maybe?
}
public class ManualGravityAssistAdjustmentStep : SequenceStep{
    public Program p;
    List<Vector3> vecs = new List<Vector3>();
    List<Vector3> pvecs = new List<Vector3>();
    public ManualGravityAssistAdjustmentStep(Program p){
        this.p = p;
    }
    public override float getProgress(){
        return p.Me.CustomData==""?0:1;
    }
    public override void start(){
        p.Me.CustomData = "";
    }
    public override void process(){
        Vector3 vec = p.getVector();
        vecs.Add(vec);
        pvecs.Add(p.controller.GetPosition());
        if(vecs.Count>100)vecs.RemoveAt(0);
        if(pvecs.Count>100)pvecs.RemoveAt(0);
        Vector3 average = Vector3.Zero;
        Vector3 paverage = Vector3.Zero;
        foreach(Vector3 v in vecs){
            average+=v;
        }
        foreach(Vector3 v in pvecs){
            paverage+=v;
        }
        average/=vecs.Count;
        paverage/=pvecs.Count;
        p.Echo("BASE Position:");
        p.Echo("X "+paverage.X);
        p.Echo("Y "+paverage.Y);
        p.Echo("Z "+paverage.Z);
        p.Echo("Target Aligned:");
        p.Echo("X "+average.X);
        p.Echo("Y "+average.Y);
        p.Echo("Z "+average.Z);
        p.Echo("Waiting for Gravity Assist Adjustment...\n(Paste into Custom Data)");
    }
}
public class ManualCallGravityAssistStep : SequenceStep{
    bool done = false;
    public Program p;
    public ManualCallGravityAssistStep(Program p){
        this.p = p;
    }
    public override float getProgress(){
        return done?1:0;
    }
    public override void process(){
        Vector3 facing = p.getVector();
        //Vector3 pos = p.controller.GetPosition()+facing*1000;
        String[] s = p.Me.CustomData.Split(' ');
        Vector3 pos = new Vector3(Single.Parse(s[0]), Single.Parse(s[1]), Single.Parse(s[2]));
        Vector3 adjustment = new Vector3(facing.Y, facing.Z, facing.X);//rotated 90 degrees, don't care in which direction
        p.IGC.SendBroadcastMessage(p.igcTagAssist, pos+adjustment*10);//10 meters off to the side, just hope it's enough lol
        done = true;
    }
}
public class ResetGravityAssistStep : SequenceStep{
    bool done = false;
    public Program p;
    public ResetGravityAssistStep(Program p){
        this.p = p;
    }
    public override float getProgress(){
        return done?1:0;
    }
    public override void process(){
        p.IGC.SendBroadcastMessage(p.igcTagAssist, Vector3.Zero);
        done = true;
    }
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
public class SetVarStep : SequenceStep{
    public String var;
    public object val;
    public Program p;
    public SetVarStep(Program p, String var, object value){
        this.p = p;
        this.var = var;
        this.val = value;
    }
    public override float getProgress(){
        return p.variables[var].Equals(val)?1:0;
    }
    public override void process(){
        p.variables[var] = val;
    }
}
public class WaitVarStep : SequenceStep{
    public String var;
    public object val;
    public Program p;
    public WaitVarStep(Program p, String var, object value){
        this.p = p;
        this.var = var;
        this.val = value;
    }
    public override float getProgress(){
        return p.variables[var].Equals(val)?1:0;
    }
    public override void process(){
        p.Echo("Waiting for "+var+"...");
    }
}
public class ConditionStep : SequenceStep{
    public String conditionVar;
    public object conditionVal;
    public SequenceStep step = null;
    public Program p;
    public bool conditionMet = false;
    public ConditionStep(Program p, String var, object value){
        this.p = p;
        this.conditionVar = var;
        this.conditionVal = value;
    }
    public override float getProgress(){
        if(!conditionMet)return 1;
        return step.getProgress();
    }
    public override void start(){
        conditionMet = p.variables[conditionVar].Equals(conditionVal);
        if(conditionMet)step.start();
    }
    public override void process(){
        p.Echo("Condition "+conditionVar+"=="+conditionVal+": "+(conditionMet?"Met":"Not Met"));
        if(conditionMet)step.process();
    }
    public override void finish(){
        if(conditionMet)step.finish();
    }
}
public class RotorSequenceStep : SequenceStep{
    public float toleran = 0.001f;
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
    public float toleran = 0.0001f;
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
    public float toleran = 0.001f;
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
public class DoorsSequenceStep : SequenceStep{
    public List<IMyDoor> doors = new List<IMyDoor>();
    public bool open;
    public Program p;
    public DoorsSequenceStep(Program p, String name, bool open){
        this.p = p;
        foreach(IMyTerminalBlock block in p.findBlocks(name)){
            doors.Add(block as IMyDoor);
        }
        this.open = open;
    }
    public override float getProgress(){//could do a real value but whatever
        int progress = 0;
        foreach(IMyDoor door in doors){
            if(open){
                if(door.Status==DoorStatus.Open)progress+=2;
                if(door.Status==DoorStatus.Opening)progress++;
            }else{
                if(door.Status==DoorStatus.Closed)progress+=2;
                if(door.Status==DoorStatus.Closing)progress++;
            }
        }
        return doors.Count==0?0:progress/(doors.Count*2);
    }
    public override void process(){
        foreach(IMyDoor door in doors){
            p.Echo((p.useCustomData?door.CustomData:door.CustomName)+": "+door.Status.ToString());
            if(open)door.OpenDoor();
            else door.CloseDoor();
        }
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
public class LandingGearSequenceStep : SequenceStep{
    public IMyLandingGear gear;
    public bool locked;
    public Program p;
    public LandingGearSequenceStep(Program p, String name, bool locked){
        this.p = p;
        gear = p.findBlock(name) as IMyLandingGear;
        this.locked = locked;
    }
    public override float getProgress(){
        return gear.IsLocked==locked?1:0;
    }
    public override void process(){
        p.Echo((p.useCustomData?gear.CustomData:gear.CustomName)+": "+gear.IsLocked);
        if(locked)gear.Lock();
        else gear.Unlock();
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
public class RemoveSequenceStep : SequenceStep{
    public Program p;
    public Sequence sequence;
    public RemoveSequenceStep(Program p, Sequence sequence){
        this.p = p;
        this.sequence = sequence;
    }
    public override float getProgress(){
        return 1;
    }
    public override void start(){
        p.removeSequences.Add(sequence);
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
        if(!b.IsFunctional)continue;
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
    if(Double.IsNaN(final)||Double.IsInfinity(final))final = 0;//sanity sanity check
    return (float)Math.Max(-1, Math.Min(1, final));//sanity check
}
public bool checkVar(String var, object val){
    return variables[var].Equals(val);
}
public class Planet{
    public String name;
    public Vector3 center;
    public float radius;
    public float gravity;
    public float hillRadius;
    public float lowerAtmosphere;
    public float upperAtmosphere;
    public Planet(String name, Vector3 pos, float radius, float gravity, float hillRadius, float lowerAtmosphere, float upperAtmosphere){
        this.name = name;
        this.center = pos;
        this.radius = radius;
        this.gravity = gravity;
        this.hillRadius = hillRadius;
        this.lowerAtmosphere = lowerAtmosphere;
        this.upperAtmosphere = upperAtmosphere;
    }
    public Vector3 getGravityAt(Vector3 pos){
        float grav = getGravityAt(Vector3.Distance(pos,this.center));
        Vector3 direction = center-pos;
        return direction.Normalized()*grav;
    }
    public float getGravityAt(float distanceFromCenter){
        double grav = Math.Min(gravity, gravity*Math.Pow(distanceFromCenter/hillRadius,-7));
        return (float)(grav/9.81f>=0.05?grav:0);
    }
}
public class GravityField{
    public Vector3 pos;
    public float radius;
    public Vector3 gravity;
    public GravityField(Vector3 pos, float radius, Vector3 gravity){
        this.pos = pos;
        this.radius = radius;
        this.gravity = gravity;
    }
}