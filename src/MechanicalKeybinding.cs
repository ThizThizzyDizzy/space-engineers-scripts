//Set this to false to use block names instead of custom data
public bool useCustomData = true;

public Program(){
    // How often the script runs; set to `Update1`, `Update10`, or `Update100` to run every 1, 10, or 100 ticks
    // i.e. This controls how responsive the pistons&rotors will be. set to Update1 for very responsive.
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    
    // To make a keybinding, first designate which cockpit you want to use with `newController("<custom data>")`
    // NOTE: The controller must not be on a static grid (it can be on any subgrid)
    // then bind rotors with `bindRotor("<custom data>", <keybind>, <velocity>);`
    // bind pistons with `bindPiston("<custom data>", <keybind>, <velocity>);`
    // bind other blocks with `bindBlock("<custom data>", <keybind>);` (This will power the block on and off)

    //to bind all blocks with the same custom data, use `bindRotors`, `bindPistons`, or `bindBlocks`

    //KEY BINDINGS GO HERE VVVVV

    newController("Workshop Welder Controller");
    bindPiston("Workshop Welder Piston X1", A, -1);
    bindPiston("Workshop Welder Piston X1", D, 1);
    bindPiston("Workshop Welder Piston X2", A, 1);
    bindPiston("Workshop Welder Piston X2", D, -1);
    bindPiston("Workshop Welder Piston Z", Space, 1);
    bindPiston("Workshop Welder Piston Z", C, -1);
    bindPiston("Workshop Welder Piston Y1", W, 1);
    bindPiston("Workshop Welder Piston Y1", S, -1);
    bindPiston("Workshop Welder Piston Y2", W, 1);
    bindPiston("Workshop Welder Piston Y2", S, -1);
    bindPiston("Workshop Welder Piston Y3", W, 1);
    bindPiston("Workshop Welder Piston Y3", S, -1);
    bindPiston("Workshop Welder Piston Y4", W, -1);
    bindPiston("Workshop Welder Piston Y4", S, 1);
}

// !!! DO NOT MODIFY BELOW THIS LINE !!!

// This is a list of all keybinds you can use; Mouse controls are up/down/left/right (you can also use lowercase and first-letter-uppercase) 

public const int W = 0;
public const int A = 1;
public const int S = 2;
public const int D = 3;
public const int SPACE = 4;
public const int C = 5;
public const int Q = 6;
public const int E = 7;
public const int UP = 8;
public const int DOWN = 9;
public const int LEFT = 10;
public const int RIGHT = 11;

// No useful information beyond this point

public List<Controller> controllers = new List<Controller>();
public Controller currentController;

public void newController(String name){
    currentController = new Controller(this, name);
    controllers.Add(currentController);
}
public void bindRotor(String name, int keybind, double velocity){
    IMyMotorStator rotor = findBlock(name) as IMyMotorStator;
    if(rotor==null)Echo("Invalid rotor: "+name);
    currentController.bindRotor(rotor, keybind, velocity);
}
public void bindPiston(String name, int keybind, double velocity){
    IMyPistonBase piston = findBlock(name) as IMyPistonBase;
    if(piston==null)Echo("Invalid piston: "+name);
    currentController.bindPiston(piston, keybind, velocity);
}
public void bindBlock(String name, int keybind){
    IMyFunctionalBlock block = findBlock(name) as IMyFunctionalBlock;
    if(block==null)Echo("Invalid functional block: "+name);
    currentController.bindBlock(block, keybind);
}
public void bindRotors(String name, int keybind, double velocity){
    foreach(IMyTerminalBlock block in findBlocks(name)){
        IMyMotorStator rotor = block as IMyMotorStator;
        if(rotor==null)Echo("Invalid rotor: "+name);
        currentController.bindRotor(rotor, keybind, velocity);
    }
}
public void bindPistons(String name, int keybind, double velocity){
    foreach(IMyTerminalBlock block in findBlocks(name)){
        IMyPistonBase piston = block as IMyPistonBase;
        if(piston==null)Echo("Invalid piston: "+name);
        currentController.bindPiston(piston, keybind, velocity);
    }
}
public void bindBlocks(String name, int keybind){
    foreach(IMyTerminalBlock block in findBlocks(name)){
        IMyFunctionalBlock fblock = block as IMyFunctionalBlock;
        if(fblock==null)Echo("Invalid functional block: "+name);
        currentController.bindBlock(fblock, keybind);
    }
}
public void Main(){
    foreach(Controller controller in controllers){
        controller.run(this);
    }
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
public const int w = W;
public const int a = A;
public const int s = S;
public const int d = D;
public const int space = SPACE;
public const int c = C;
public const int q = Q;
public const int e = E;
public const int up = UP;
public const int down = DOWN;
public const int left = LEFT;
public const int right = RIGHT;
public const int Up = UP;
public const int Down = DOWN;
public const int Left = LEFT;
public const int Right = RIGHT;
public const int Space = SPACE;
public class Controller{
    public IMyShipController controller;
    public List<RotorBinding> rotors = new List<RotorBinding>();
    public List<PistonBinding> pistons = new List<PistonBinding>();
    public List<BlockBinding> blocks = new List<BlockBinding>();
    public Controller(Program p, String name){
        controller = p.findBlock(name) as IMyShipController;
        if(controller==null)p.Echo("Invalid Ship Controller: "+name);
    }
    public void bindRotor(IMyMotorStator rotor, int keybind, double velocity){
        RotorBinding binding = null;
        foreach(RotorBinding b in rotors){
            if(b.rotor==rotor)binding = b;
        }
        if(binding==null){
            binding = new RotorBinding(rotor);
            rotors.Add(binding);
        }
        binding.bind(keybind, velocity);
    }
    public void bindPiston(IMyPistonBase piston, int keybind, double velocity){
        PistonBinding binding = null;
        foreach(PistonBinding b in pistons){
            if(b.piston==piston)binding = b;
        }
        if(binding==null){
            binding = new PistonBinding(piston);
            pistons.Add(binding);
        }
        binding.bind(keybind, velocity);
    }
    public void bindBlock(IMyFunctionalBlock block, int keybind){
        BlockBinding binding = null;
        foreach(BlockBinding b in blocks){
            if(b.block==block)binding = b;
        }
        if(binding==null){
            binding = new BlockBinding(block);
            blocks.Add(binding);
        }
        binding.bind(keybind);
    }
    public void run(Program p){
        double x = controller.MoveIndicator.X;
        double y = controller.MoveIndicator.Y;
        double z = controller.MoveIndicator.Z;
        double rx = controller.RotationIndicator.X;
        double ry = controller.RotationIndicator.Y;
        double rz = controller.RollIndicator;
        foreach(RotorBinding b in rotors){
            b.run(p,x,y,z,rx,ry,rz);
        }
        foreach(PistonBinding b in pistons){
            b.run(p,x,y,z,rx,ry,rz);
        }
        foreach(BlockBinding b in blocks){
            b.run(p,x,y,z,rx,ry,rz);
        }
    }
}
public class RotorBinding{
    public IMyMotorStator rotor;
    public Dictionary<int, double> keybinds = new Dictionary<int, double>();
    public RotorBinding(IMyMotorStator rotor){
        this.rotor = rotor;
    }
    public void bind(int keybind, double velocity){
        keybinds.Add(keybind, velocity);
    }
    public void run(Program p, double x, double y, double z, double rx, double ry, double rz){
        double vel = 0;
        if(keybinds.ContainsKey(A)&&x<0)vel += keybinds[A];
        if(keybinds.ContainsKey(D)&&x>0)vel += keybinds[D];
        if(keybinds.ContainsKey(C)&&y<0)vel += keybinds[C];
        if(keybinds.ContainsKey(SPACE)&&y>0)vel += keybinds[SPACE];
        if(keybinds.ContainsKey(S)&&z<0)vel += keybinds[S];
        if(keybinds.ContainsKey(W)&&z>0)vel += keybinds[W];
        if(keybinds.ContainsKey(LEFT)&&rx<0)vel += keybinds[LEFT];
        if(keybinds.ContainsKey(RIGHT)&&rx>0)vel += keybinds[RIGHT];
        if(keybinds.ContainsKey(DOWN)&&ry<0)vel += keybinds[DOWN];
        if(keybinds.ContainsKey(UP)&&ry>0)vel += keybinds[UP];
        if(keybinds.ContainsKey(Q)&&rz<0)vel += keybinds[Q];
        if(keybinds.ContainsKey(E)&&rz>0)vel += keybinds[E];
        rotor.TargetVelocityRPM = (float)vel;
    }
}
public class PistonBinding{
    public IMyPistonBase piston;
    public Dictionary<int, double> keybinds = new Dictionary<int, double>();
    public PistonBinding(IMyPistonBase piston){
        this.piston = piston;
    }
    public void bind(int keybind, double velocity){
        keybinds.Add(keybind, velocity);
    }
    public void run(Program p, double x, double y, double z, double rx, double ry, double rz){
        double vel = 0;
        if(keybinds.ContainsKey(A)&&x<0)vel += keybinds[A];
        if(keybinds.ContainsKey(D)&&x>0)vel += keybinds[D];
        if(keybinds.ContainsKey(C)&&y<0)vel += keybinds[C];
        if(keybinds.ContainsKey(SPACE)&&y>0)vel += keybinds[SPACE];
        if(keybinds.ContainsKey(S)&&z<0)vel += keybinds[S];
        if(keybinds.ContainsKey(W)&&z>0)vel += keybinds[W];
        if(keybinds.ContainsKey(LEFT)&&rx<0)vel += keybinds[LEFT];
        if(keybinds.ContainsKey(RIGHT)&&rx>0)vel += keybinds[RIGHT];
        if(keybinds.ContainsKey(DOWN)&&ry<0)vel += keybinds[DOWN];
        if(keybinds.ContainsKey(UP)&&ry>0)vel += keybinds[UP];
        if(keybinds.ContainsKey(Q)&&rz<0)vel += keybinds[Q];
        if(keybinds.ContainsKey(E)&&rz>0)vel += keybinds[E];
        piston.Velocity = (float)vel;
    }
}
public class BlockBinding{
    public IMyFunctionalBlock block;
    public List<int> keybinds = new List<int>();
    public BlockBinding(IMyFunctionalBlock block){
        this.block = block;
    }
    public void bind(int keybind){
        keybinds.Add(keybind);
    }
    public void run(Program p, double x, double y, double z, double rx, double ry, double rz){
        bool active = false;
        if(keybinds.Contains(A)&&x<0)active = true;
        if(keybinds.Contains(D)&&x>0)active = true;
        if(keybinds.Contains(C)&&y<0)active = true;
        if(keybinds.Contains(SPACE)&&y>0)active = true;
        if(keybinds.Contains(S)&&z<0)active = true;
        if(keybinds.Contains(W)&&z>0)active = true;
        if(keybinds.Contains(LEFT)&&rx<0)active = true;
        if(keybinds.Contains(RIGHT)&&rx>0)active = true;
        if(keybinds.Contains(DOWN)&&ry<0)active = true;
        if(keybinds.Contains(UP)&&ry>0)active = true;
        if(keybinds.Contains(Q)&&rz<0)active = true;
        if(keybinds.Contains(E)&&rz>0)active = true;
        block.Enabled = active;
    }
}