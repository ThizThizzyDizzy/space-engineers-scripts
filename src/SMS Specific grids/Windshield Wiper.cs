public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    variables["Unfolded"] = !(findBlock("Merge L2") as IMyShipMergeBlock).Enabled;
    variables["Folded"] = (findBlock("Merge C1") as IMyShipMergeBlock).Enabled;
}
public bool useCustomData = false;
float g0Mult = 5;
float g1Mult = 1;
public bool Init(int i) {
    if(i==0){
        newSequence("Unfold");
        setVar("Folded", false);
        addParallel();
        addRotor("Hinge L2", 0, 0.5f);
        addRotor("Hinge R2", 0, 0.5f);
        addRotor("Hinge L5", 0, 0.5f);
        addRotor("Hinge R5", 0, 0.5f);
        endParallel();
        addParallel();
        addMerge("Merge L1");
        addMerge("Merge R1");
        endParallel();
        addParallel();
        addToggle("Merge C1", false);
        addToggle("Merge C2", false);
        endParallel();
        addParallel();
        addHinge("Hinge C1", -45, 2f);
        addHinge("Hinge L3", 0, 0.5f);
        addHinge("Hinge R3", 0, 0.5f);
        addHinge("Hinge L4", 0, 0.5f);
        addHinge("Hinge R4", 0, 0.5f);
        
        addHinge("Hinge L1", 0, 0.5f);
        addHinge("Hinge R1", 0, 0.5f);
        addHinge("Hinge L6", 0, 0.5f);
        addHinge("Hinge R6", 0, 0.5f);
        endParallel();
        addParallel();
        addToggle("Merge L2", false);
        addToggle("Merge R2", false);
        addToggle("Merge L3", false);
        addToggle("Merge R3", false);
        endParallel();
        addParallel();
        addHinge("Hinge L7", 90, 0.75f);
        addHinge("Hinge R7", 90, 0.75f);
        addHinge("Hinge L8", 90, 0.75f);
        addHinge("Hinge R8", 90, 0.75f);
        endParallel();
        setVar("Unfolded", true);
        return true;
    }
    if(i==1){
        newSequence("Fold");
        setVar("Unfolded", false);
        addParallel();
        addRotor("Hinge R1", 0, 0.03f*g0Mult, 3);
        addRotor("Hinge L1", 0, 0.03f*g0Mult, 3);
        addRotor("Hinge R3", 0, 0.03f*g0Mult, 3);
        addRotor("Hinge L3", 0, 0.03f*g0Mult, 3);
        addRotor("Hinge R4", 0, 0.03f*g0Mult, 3);
        addRotor("Hinge L4", 0, 0.03f*g0Mult, 3);
        addRotor("Hinge R6", 0, 0.03f*g0Mult, 3);
        addRotor("Hinge L6", 0, 0.03f*g0Mult, 3);
        for(int p = 1; p<=16; p++){
            addPiston("Piston L"+p, 0, 0.05f*g0Mult, 3);
            addPiston("Piston R"+p, 0, 0.05f*g0Mult, 3);
        }
        endParallel();
        addParallel();
        addHinge("Hinge L7", 0, 0.75f);
        addHinge("Hinge R7", 0, 0.75f);
        addHinge("Hinge L8", 0, 0.75f);
        addHinge("Hinge R8", 0, 0.75f);
        endParallel();
        addParallel();
        addMerge("Merge L2");
        addMerge("Merge R2");
        addMerge("Merge L3");
        addMerge("Merge R3");
        endParallel();
        addParallel();
        addToggle("Hinge L7", false);
        addToggle("Hinge L8", false);
        addToggle("Hinge R7", false);
        addToggle("Hinge R8", false);
        endParallel();
        addParallel();
        addHinge("Hinge C1", 0, 0.25f);
        addHinge("Hinge L3", 90, 0.5f);
        addHinge("Hinge R3", 90, 0.5f);
        addHinge("Hinge L4", 90, 0.5f);
        addHinge("Hinge R4", 90, 0.5f);
        endParallel();
        addParallel();
        addMerge("Merge C1");
        addMerge("Merge C2");
        endParallel();
        addParallel();
        addToggle("Hinge L3", false);
        addToggle("Hinge R3", false);
        addToggle("Hinge L4", false);
        addToggle("Hinge R4", false);

        addToggle("Merge L1", false);
        addToggle("Merge R1", false);
        endParallel();
        addParallel();
        addRotor("Hinge L2", 90, 0.5f);
        addRotor("Hinge R2", 90, 0.5f);
        addRotor("Hinge L5", 90, 0.5f);
        addRotor("Hinge R5", 90, 0.5f);
        endParallel();
        setVar("Folded", true);
        return true;
    }
    if(i<=102){
        int num = i-2;
        float percent = num/100f;
        float rpercent = 1-percent;
        newSequence("G0 "+num+"%");
        setConditional("Unfolded");
        addParallel();
        addToggle("Welder C", false);
        for(int j = 1; j<=23; j++){
            addToggle("Welder L"+j, false);
            addToggle("Welder R"+j, false);
        }
        endParallel();
        setConditional("Unfolded");
        addParallel();
        addRotor("Hinge R1", 90*rpercent, 0.03f*g0Mult, 3);
        addRotor("Hinge L1", 90*rpercent, 0.03f*g0Mult, 3);
        addRotor("Hinge R3", -90*rpercent, 0.03f*g0Mult, 3);
        addRotor("Hinge L3", -90*rpercent, 0.03f*g0Mult, 3);
        addRotor("Hinge R4", -90*rpercent, 0.03f*g0Mult, 3);
        addRotor("Hinge L4", -90*rpercent, 0.03f*g0Mult, 3);
        addRotor("Hinge R6", -90*rpercent, 0.03f*g0Mult, 3);
        addRotor("Hinge L6", -90*rpercent, 0.03f*g0Mult, 3);
        for(int p = 1; p<=16; p++){
            addPiston("Piston L"+p, percent*10, 0.05f*g0Mult, 3);
            addPiston("Piston R"+p, percent*10, 0.05f*g0Mult, 3);
        }
        endParallel();
        return true;
    }
    if(i<=203){
        int num = i-103;
        float percent = num/100f;
        float rpercent = 1-percent;
        newSequence("G1 "+num+"%");
        setConditional("Unfolded");
        addParallel();
        addToggle("Welder C", true);
        for(int j = 1; j<=23; j++){
            addToggle("Welder L"+j, true);
            addToggle("Welder R"+j, true);
        }
        endParallel();
        setConditional("Unfolded");
        addParallel();
        addRotor("Hinge R1", 90*rpercent, 0.03f*g1Mult, 5);
        addRotor("Hinge L1", 90*rpercent, 0.03f*g1Mult, 5);
        addRotor("Hinge R3", -90*rpercent, 0.03f*g1Mult, 5);
        addRotor("Hinge L3", -90*rpercent, 0.03f*g1Mult, 5);
        addRotor("Hinge R4", -90*rpercent, 0.03f*g1Mult, 5);
        addRotor("Hinge L4", -90*rpercent, 0.03f*g1Mult, 5);
        addRotor("Hinge R6", -90*rpercent, 0.03f*g1Mult, 5);
        addRotor("Hinge L6", -90*rpercent, 0.03f*g1Mult, 5);
        for(int p = 1; p<=16; p++){
            addPiston("Piston L"+p, percent*10, 0.05f*g1Mult, 5);
            addPiston("Piston R"+p, percent*10, 0.05f*g1Mult, 5);
        }
        endParallel();
        setConditional("Unfolded");
        addParallel();
        addToggle("Welder C", false);
        for(int j = 1; j<=23; j++){
            addToggle("Welder L"+j, false);
            addToggle("Welder R"+j, false);
        }
        endParallel();
        return true;
    }
    return false;
}