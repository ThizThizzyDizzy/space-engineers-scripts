public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    variables["Rings Safe"] = false;
    variables["Solar Down"] = false;
}
public bool useCustomData = true;
 public bool Init(int i) {
    if(i==0){
        newSequence("Fold Solar");
        addToggle("Solar Controller", false);
        setConditional("Solar Down", false);
        addRotor("Solar Hinge", 0, 1f, 10);
        addRotor("Solar Rotor", 180, 1f, 10);
        addParallel();
        addRotor("Solar Hinge Left", 10, 1f, 10);
        addRotor("Solar Hinge Right", 10, 1f, 10);
        addRotor("Solar Hinge", -90, 1f, 10);
        endParallel();
        setVar("Solar Down", true);
        return true;
    }
    if(i==1){
        newSequence("Fold Gear");
        addParallel();
        addRotor("Landing Rotor Front", 180, 2, 10);
        addRotor("Landing Rotor Left", 180, 2, 10);
        addRotor("Landing Rotor Right", 180, 2, 10);
        addRotor("Landing Hinge 1 Front", -90, 2f, 10);
        addRotor("Landing Hinge 1 Left", -90, 2f, 10);
        addRotor("Landing Hinge 1 Right", -90, 2f, 10);
        addPiston("Landing Piston Front", 0, 5, 4);
        addPiston("Landing Piston Left", 0, 5, 4);
        addPiston("Landing Piston Right", 0, 5, 4);
        addRotor("Landing Hinge 2 Front", 0, 3, 8);
        addRotor("Landing Hinge 2 Left", 0, 3, 8);
        addRotor("Landing Hinge 2 Right", 0, 3, 8);
        addToggle("Landing Gear Front", false);
        addToggle("Landing Gear Left", false);
        addToggle("Landing Gear Right", false);
        endParallel();
        return true;
    }
    if(i==2){
        newSequence("Fold Rear Fuel Arm");
        addConnector("Rear Fuel Arm Connector", false);
        addParallel();
        addRotor("Rear Fuel Arm Hinge", 80, 2, 4);
        addPiston("Rear Fuel Arm Piston", 0, 3.5f, 6);
        endParallel();
        addRotor("Rear Fuel Arm Hinge", 90, 1, 5);
        return true;
    }
    if(i==3){
        newSequence("Fold Front Fuel Arm");
        addConnector("Front Fuel Arm Connector", false);
        addParallel();
        addRotor("Front Fuel Arm Hinge", 80, 2, 4);
        addPiston("Front Fuel Arm Piston", 0, 3.5f, 6);
        endParallel();
        addRotor("Front Fuel Arm Hinge", 90, 1, 5);
        return true;
    }
    if(i==4){
        newSequence("Fold Side Fuel Arm");
        addConnector("Side Fuel Arm Connector", false);
        addParallel();
        addToggle("Side Fuel Arm Hinge 1", true);
        addToggle("Side Fuel Arm Hinge 2", true);
        addToggle("Side Fuel Arm Hinge 3", true);
        addToggle("Side Fuel Arm Hinge 4", true);
        endParallel();
        addParallel();
        addHinge("Side Fuel Arm Hinge 1", -90, 1, 5);
        addPiston("Side Fuel Arm Piston 1", 1.25f, 1.5f, 5);
        addPiston("Side Fuel Arm Piston 2", 1.25f, 1.5f, 5);
        addHinge("Side Fuel Arm Hinge 2", 90, 1, 5);
        addPiston("Side Fuel Arm Piston 3", 0, 1, 5);
        addRotor("Side Fuel Arm Rotor", 180, 1, 5);
        addHinge("Side Fuel Arm Hinge 3", 45, 1.5f, 5);
        addHinge("Side Fuel Arm Hinge 4", -45, 1.5f, 5);
        endParallel();
        return true;
    }
    if (i == 5) {
        newSequence("Fold Rings");
        addToggle("Ring Controller", false);
        addParallel();
        setConditional("Rings Safe", false);
        runSequence("Fold Side Fuel Arm");
        setConditional("Rings Safe", false);
        runSequence("Fold Front Fuel Arm");
        setConditional("Rings Safe", false);
        runSequence("Fold Rear Fuel Arm");
        endParallel();
        setVar("Rings Safe", true);
        addParallel();
        addToggle("Ring 1 On", false);
        addToggle("Ring 2 On", false);
        endParallel();
        addParallel();
        addRotorStop("Ring 1 On");
        addRotorStop("Ring 2 On");
        endParallel();
        addParallel();
        addToggle("Ring 1 Reset",true);
        addToggle("Ring 2 Reset", true);
        endParallel();
        addParallel();
        addRotor("Ring 1 Reset", 180, 1f, 100);
        addRotor("Ring 2 Reset", 180, 1f, 100);
        endParallel();
        return true;
    }
    if (i == 6) {
        newSequence("Unfold Rings");
        addParallel();
        runSequence("Fold Rear Fuel Arm");
        runSequence("Fold Front Fuel Arm");
        runSequence("Fold Side Fuel Arm");
        runSequence("Fold Gear");
        runSequence("Fold Solar");
        endParallel();
        addParallel();
        addToggle("Ring 1 On", true);
        addToggle("Ring 2 On", true);
        addToggle("Ring 1 Reset", false);
        addToggle("Ring 2 Reset", false);
        endParallel();
        addToggle("Ring Controller", true);
        return true;
    }
    if(i==7){
        newSequence("Unfold Solar");
        runSequence("Fold Rings");
        setVar("Solar Down", false);
        addParallel();
        addRotor("Solar Hinge", 0, 1f, 10);
        addRotor("Solar Hinge Left", -90, 1f, 10);
        addRotor("Solar Hinge Right", -90, 1f, 10);
        endParallel();
        addToggle("Solar Controller", true);
        return true;
    }
    if(i==8){
        newSequence("Unfold Gear");
        runSequence("Fold Rings");
        addParallel();
        addRotor("Landing Rotor Front", 180, 2, 10);
        addRotor("Landing Rotor Left", 150, 2, 10);
        addRotor("Landing Rotor Right", 210, 2, 10);
        addRotor("Landing Hinge 1 Front", -70, 2f, 10);
        addRotor("Landing Hinge 1 Left", -70, 2f, 10);
        addRotor("Landing Hinge 1 Right", -70, 2f, 10);
        addPiston("Landing Piston Front", 10f, 5, 4);
        addPiston("Landing Piston Left", 10f, 5, 4);
        addPiston("Landing Piston Right", 10f, 5, 4);
        addRotor("Landing Hinge 2 Front", 70, 3, 8);
        addRotor("Landing Hinge 2 Left", 70, 3, 8);
        addRotor("Landing Hinge 2 Right", 70, 3, 8);
        addToggle("Landing Gear Front", true);
        addToggle("Landing Gear Left", true);
        addToggle("Landing Gear Right", true);
        endParallel();
        return true;
    }
    if(i==9){
        newSequence("Unfold Rear Fuel Arm");
        runSequence("Fold Rings");
        addRotor("Rear Fuel Arm Hinge", 80, 1, 3);
        addParallel();
        addRotor("Rear Fuel Arm Hinge", -80, 2, 4);
        addPiston("Rear Fuel Arm Piston", 10, 3f, 4);
        endParallel();
        addRotor("Rear Fuel Arm Hinge", -90, 1, 5);
        addConnector("Rear Fuel Arm Connector", true);
        return true;
    }
    if(i==10){
        newSequence("Unfold Front Fuel Arm");
        runSequence("Fold Rings");
        addRotor("Front Fuel Arm Hinge", 80, 1, 3);
        addParallel();
        addRotor("Front Fuel Arm Hinge", -80, 2, 4);
        addPiston("Front Fuel Arm Piston", 10, 3f, 4);
        endParallel();
        addRotor("Front Fuel Arm Hinge", -90, 1, 5);
        addConnector("Front Fuel Arm Connector", true);
        return true;
    }
    if(i==11){
        newSequence("Unfold Right Fuel Arm");
        runSequence("Fold Rings");
        addParallel();
        addHinge("Side Fuel Arm Hinge 1", -26, 1, 25);
        addPiston("Side Fuel Arm Piston 1", 10f, 1, 25);
        addPiston("Side Fuel Arm Piston 2", 10f, 1, 25);
        addHinge("Side Fuel Arm Hinge 2", -26, 1, 25);
        addPiston("Side Fuel Arm Piston 3", 5.5f, 1, 25);
        addRotor("Side Fuel Arm Rotor", 160, 1, 25);
        addHinge("Side Fuel Arm Hinge 3", -90, 1, 25);
        addHinge("Side Fuel Arm Hinge 4", 90, 1, 25);
        endParallel();
        addConnector("Side Fuel Arm Connector", true);
        addParallel();
        addToggle("Side Fuel Arm Hinge 1", false);
        addToggle("Side Fuel Arm Hinge 2", false);
        addToggle("Side Fuel Arm Hinge 3", false);
        addToggle("Side Fuel Arm Hinge 4", false);
        endParallel();
        return true;
    }
    if(i==12){
        newSequence("Unfold Left Fuel Arm");
        addParallel();
        runSequence("Fold Solar");
        runSequence("Fold Gear");
        runSequence("Fold Rear Fuel Arm");
        runSequence("Fold Front Fuel Arm");
        runSequence("Fold Side Fuel Arm");
        endParallel();
        runSequence("Fold Rings");
        setVar("Rings Safe", false);
        addParallel();
        addRotor("Ring 1 Reset", 180, 1f, 50);
        addRotor("Ring 2 Reset", 0, 1f, 50);
        endParallel();
        addParallel();
        addHinge("Side Fuel Arm Hinge 1", -26, 1, 25);
        addPiston("Side Fuel Arm Piston 1", 10f, 1, 25);
        addPiston("Side Fuel Arm Piston 2", 10f, 1, 25);
        addHinge("Side Fuel Arm Hinge 2", -26, 1, 25);
        addPiston("Side Fuel Arm Piston 3", 5.5f, 1, 25);
        addRotor("Side Fuel Arm Rotor", 160, 1, 25);
        addHinge("Side Fuel Arm Hinge 3", -90, 1, 25);
        addHinge("Side Fuel Arm Hinge 4", 90, 1, 25);
        endParallel();
        addConnector("Side Fuel Arm Connector", true);
        addParallel();
        addToggle("Side Fuel Arm Hinge 1", false);
        addToggle("Side Fuel Arm Hinge 2", false);
        addToggle("Side Fuel Arm Hinge 3", false);
        addToggle("Side Fuel Arm Hinge 4", false);
        endParallel();
        return true;
    }
    if(i==13){
        newSequence("Fold All");
        addParallel();
        runSequence("Fold Solar");
        runSequence("Fold Gear");
        runSequence("Fold Front Fuel Arm");
        runSequence("Fold Rear Fuel Arm");
        runSequence("Fold Side Fuel Arm");
        endParallel();
        runSequence("Fold Rings");
        return true;
    }
    if(i==14){
        newSequence("Rings Safe");
        setVar("Rings Safe", true);
        return true;
    }
    return false;
}