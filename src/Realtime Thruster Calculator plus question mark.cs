bool showDamageReport = true; //Setting this to false will prevent any "DAMAGE REPORT" messages from appearing.
bool forceFlight = false; //Setting this to true will only provide information about the current environment.
bool planningMode = false; //Setting this to true will include all thrusters; damaged, turned off, or incomplete, in thrust calculations.
Planet EARTH;
Planet MARS;
Planet ALIEN;
Planet MOON;
Planet EUROPA;
Planet TITAN;
Planet TRITON;
Planet SPACE;
List<Planet> planets = new List<Planet>();
const int MODE_NONE = 0;
const int MODE_BUILD = 1;
const int MODE_FLY = 2;
const int buildTimeout = 6*5;
int buildTimer = 0;
const bool thrusterDirectionOverride = true;
int mode = MODE_BUILD;
Battery battery;
GasTank hydro;
GasTank oxy;
InventoryContainer ice;
InventoryOverview cargo;
JumpDrive jumpdrive;
Vector3D? naturalGravity;
Vector3D? artificialGravity;
Vector3D? gravity;
double? speed;
MyShipVelocities? velocities;
MyShipMass? mass;
Vector3D? planetPosition;
double? seaLevelElevation;
double? surfaceElevation;
bool controlled = false;
Planet planet;
ThrusterConfiguration thrusters;
IMyShipController cockpit;
IMyTextSurface panel;
int batTimer = 0;
int speakerTimer = 0;
String damageReport;
IMySoundBlock speaker;
bool wideMode;
Vector3I preferredDirection = Vector3I.Zero;
String preferredDirectionS = "None";
class PlanetSorter : IComparer<Planet>{
    public int Compare(Planet x, Planet y){
        return x.CompareTo(y);
    }
}
public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    EARTH = new Planet(this, "Earth", new Vector3D(0.5,0.5,0.5), 60000, 0, 14400, 9.81, 67200);
    MARS = new Planet(this, "Mars", new Vector3D(1031072.5,131072.5,1631072.5), 60000, 0, 14400, 8.829, 67200);
    ALIEN = new Planet(this, "Sabir", new Vector3D(131072.5, 131072.5, 5731072.5), 60000, 2400, 14400, 10.791, 67200);
    MOON = new Planet(this, "Moon", new Vector3D(16384.5,136384.5,-113615.5), 9500, 0, 0, 2.4525, 10640);
    EUROPA = new Planet(this, "Europa", new Vector3D(916384.5,16384.5,1616384.5), 9500, 0, 1140, 2.4525, 10640);
    TITAN = new Planet(this, "Titan", new Vector3D(36384.5,226384.5,5796384.5), 9500, 0, 570, 2.4525, 10640);
    TRITON = new Planet(this, "Triton", new Vector3D(-284463.5,-2434463.5,365536.5), 40000, 0, 2656, 9.81, 44800);
    SPACE = new Planet(this, "Space", new Vector3D(0,0,0), 0, 0, 0, 0, 0);
    planet = SPACE;
    thrusters = new ThrusterConfiguration(this);
    planets.Add(EARTH);
    planets.Add(MARS);
    planets.Add(ALIEN);
    planets.Add(MOON);
    planets.Add(EUROPA);
    planets.Add(TITAN);
    planets.Add(TRITON);
}
public void Main(String arg){
    if(arg=="Up"){
        preferredDirection = Vector3I.Down;
        preferredDirectionS = "Up";
    }
    if(arg=="Down"){
        preferredDirection = Vector3I.Up;
        preferredDirectionS = "Down";
    }
    if(arg=="Left"){
        preferredDirection = Vector3I.Right;
        preferredDirectionS = "Left";
    }
    if(arg=="Right"){
        preferredDirection = Vector3I.Left;
        preferredDirectionS = "Right";
    }
    if(arg=="Forward"){
        preferredDirection = Vector3I.Backward;
        preferredDirectionS = "Forward";
    }
    if(arg=="Backward"){
        preferredDirection = Vector3I.Forward;
        preferredDirectionS = "Backward";
    }
    if(arg=="None"||arg=="Reset"){
        preferredDirection = Vector3I.Zero;
        preferredDirectionS = "None";
    }
    if(battery==null){
        battery = new Battery(this);
        hydro = new GasTank(this, "Hydrogen");
        oxy = new GasTank(this, "Oxygen");
        ice = new InventoryContainer(this, "Ice");
        cargo = new InventoryOverview(this);
        jumpdrive = new JumpDrive(this);
    }
    batTimer++;
    if(batTimer>60){
        battery.refresh();
        hydro.refresh();
        oxy.refresh();
        ice.refresh();
        jumpdrive.refresh();
        batTimer = 0;
    }
    hydro.tick();
    oxy.tick();
    ice.tick();
    jumpdrive.tick();
    mode = MODE_NONE;
    updateAll();
    buildTimer++;
    if(speed>1&&controlled)mode = MODE_FLY;
    if(mode==MODE_FLY)buildTimer = 0;
    if(buildTimer<buildTimeout)mode = MODE_FLY;
    if(!controlled&&mode==MODE_FLY)mode = MODE_NONE;
    if(mode==MODE_NONE)mode = MODE_BUILD;          //probably not the best idea, but sure
    if(forceFlight)mode = MODE_FLY;
    if(seaLevelElevation==null||mass==null||mode==MODE_NONE){
        String txt = "";
        txt+="Current environment: UNKNOWN\n";
        if(mass==null)txt+="Cannot read ship mass! Please add a ship controller!";
        planet = SPACE;
        planets.Sort(new PlanetSorter());
        String dirS = thrusters.getMaxDirectionS(SPACE, 0);
        Vector3I dir = thrusters.getMaxDirection(SPACE, 0);
        double thrus = thrusters.getThrust(dir, SPACE, 0);
        if(thrus>0){
            txt+="Current Max Thrust: "+dirS+" ("+makeReadableMetric(thrus*1000)+"N)\n";
        }
        display(txt);
        return;
    }
    String text = (preferredDirection==Vector3I.Zero?"":"PREFERRED DIRECTION: "+preferredDirectionS+" ");//(mode==MODE_BUILD?"Construction Mode":(mode==MODE_FLY?"Flight Mode":"NONE"))+" ";
    if(planningMode)text+="PLANNING MODE ";
    text+="\n";
    double alt = (double)seaLevelElevation;
    String directionS = thrusters.getMaxDirectionS(planet, alt);
    Vector3I direction = thrusters.getMaxDirection(planet, alt);
    double thrust = thrusters.getThrust(direction, planet, alt);
    double shipMass = ((MyShipMass)mass).PhysicalMass;
    double rawAcceleration = thrust/shipMass;
    text+="Current environment: "+planet.name;
    if(mode==MODE_BUILD){
        if(thrust>0){
            planets.Sort(new PlanetSorter());
            if(wideMode){
                text+="\nCurrent Maximum Acceleration: "+directionS+" ("+makeReadable(rawAcceleration-planet.getGravity(alt))+" m/s^2)\n";
                text+="Name: low altitude | high altitude | in orbit | capable of planetary escape?\n";
            }else{
                text+="\nCurrent Max Accel: "+directionS+" ("+makeReadable(rawAcceleration-planet.getGravity(alt))+" m/s^2)\n";
                text+="\nPlanets:\nName: low | high | orbit | escape?\n";
            }
            foreach(Planet planet in planets){
                double lowAlt = planet.lowerAtmosphere;
                double highAlt = planet.maxHillRadius-planet.radius;
                double orbitAlt = 0.7*(planet.upperAtmosphere-planet.lowerAtmosphere)+planet.lowerAtmosphere;
                double low = thrusters.getThrust(thrusters.getMaxDirection(planet, lowAlt), planet, lowAlt)/shipMass-planet.getGravity(lowAlt);
                double high = thrusters.getThrust(thrusters.getMaxDirection(planet, highAlt), planet, highAlt)/shipMass-planet.getGravity(highAlt);
                double orbit = thrusters.getThrust(thrusters.getMaxDirection(planet, orbitAlt), planet, orbitAlt)/shipMass-planet.getGravity(orbitAlt);
                String escape = "FALSE";
                if(low>0&&high>0&&orbit>0)escape = "TRUE";
                if(wideMode){
                    text+=planet.name+": "+thrusters.getMaxDirectionS(planet, lowAlt)+" "+makeReadable(low)+" | "+thrusters.getMaxDirectionS(planet, highAlt)+" "+makeReadable(high)+" | "+thrusters.getMaxDirectionS(planet, orbitAlt)+" "+makeReadable(orbit)+" | Can escape: "+escape+"\n";
                }else{
                    text+=planet.name+": "+makeReadable(low)+" | "+makeReadable(high)+" | "+makeReadable(orbit)+" | "+escape+"\n";
                }
            }
        }
    }else if(mode==MODE_FLY){
        if(planet==SPACE){
            if(thrust>0){
                if(wideMode){
                    text+="\nCurrent Maximum Acceleration: "+directionS+" ("+makeReadable(rawAcceleration-planet.getGravity(alt))+" m/s^2)";
                }else{
                    text+="\nCurrent Max Accel: "+directionS+" ("+makeReadable(rawAcceleration-planet.getGravity(alt))+" m/s^2)\n";
                }
            }
        }else{
            double lowAlt = planet.lowerAtmosphere;
            double highAlt = planet.maxHillRadius-planet.radius;
            double orbitAlt = 0.7*(planet.upperAtmosphere-planet.lowerAtmosphere)+planet.lowerAtmosphere;
            double low = thrusters.getThrust(thrusters.getMaxDirection(planet, lowAlt), planet, lowAlt)/shipMass-planet.getGravity(lowAlt);
            double high = thrusters.getThrust(thrusters.getMaxDirection(planet, highAlt), planet, highAlt)/shipMass-planet.getGravity(highAlt);
            double orbit = thrusters.getThrust(thrusters.getMaxDirection(planet, orbitAlt), planet, orbitAlt)/shipMass-planet.getGravity(orbitAlt);
            double space = thrusters.getThrust(thrusters.getMaxDirection(SPACE, 0), SPACE, 0)/shipMass;
            String location = "Orbit";
            if(alt<orbitAlt)location = "High Altitude";
            if(alt<highAlt)location = "Medium Altitude";
            if(alt<lowAlt)location = "Low Altitude";
            if(wideMode){
                text+=" | Current Altitude: "+makeReadableMetric(alt)+"m ("+location+")\n";
            }else{
                text+="\nCurrent Altitude: "+makeReadableMetric(alt)+"m ("+location+")\n";
            }
            if(thrust>0){
                if(wideMode){
                    text+="Current Maximum Acceleration: "+directionS+" ("+makeReadable(rawAcceleration-planet.getGravity(alt))+" m/s^2)\n";
                    text+="at "+makeReadableMetric(lowAlt)+"m: "+thrusters.getMaxDirectionS(planet, lowAlt)+" "+makeReadable(low)+" m/s^2,  ";
                    text+="at "+makeReadableMetric(highAlt)+"m: "+thrusters.getMaxDirectionS(planet, highAlt)+" "+makeReadable(high)+" m/s^2,  ";
                    text+="at "+makeReadableMetric(orbitAlt)+"m: "+thrusters.getMaxDirectionS(planet, orbitAlt)+" "+makeReadable(orbit)+" m/s^2\n";
                    text+="in Space: "+thrusters.getMaxDirectionS(SPACE, 0)+" "+makeReadable(space)+" m/s^2   ";
                    if(low>0&&high>0&&orbit>0)text+="Capable of planetary escape!";
                    if(directionS!="Forward")text+="\nForward Acceleration: "+makeReadable(thrusters.getThrust(Vector3I.Backward, planet, (double)seaLevelElevation)/shipMass)+" m/s^2\n";
                }else{
                    text+="Current Max Accel: "+directionS+" ("+makeReadable(rawAcceleration-planet.getGravity(alt))+" m/s^2)\n";
                    text+="at Low altitude ("+makeReadableMetric(lowAlt)+"m): "+thrusters.getMaxDirectionS(planet, lowAlt)+" "+makeReadable(low)+" m/s^2\n";
                    text+="at High altitude ("+makeReadableMetric(highAlt)+"m): "+thrusters.getMaxDirectionS(planet, highAlt)+" "+makeReadable(high)+" m/s^2\n";
                    text+="in Orbit ("+makeReadableMetric(orbitAlt)+"m): "+thrusters.getMaxDirectionS(planet, orbitAlt)+" "+makeReadable(orbit)+" m/s^2\n";
                    text+="in Space: "+thrusters.getMaxDirectionS(SPACE, 0)+" "+makeReadable(space)+" m/s^2\n";
                    if(low>0&&high>0&&orbit>0)text+="\nCapable of planetary escape!\n";
                    if(directionS!="Forward")text+="\nForward Acceleration: "+makeReadable(thrusters.getThrust(Vector3I.Backward, planet, (double)seaLevelElevation)/shipMass)+" m/s^2\n";
                }
            }
        }
    }
    display(text);
}
void display(String text){
    if(damageReport!=null){
        if(wideMode){
            text = "DAMAGE REPORT: "+damageReport+"\n"+text;
        }else{
            text = "DAMAGE REPORT: "+damageReport+"\n\n,"+text;
        }
        speakerTimer++;
        if(speakerTimer>1&&speaker!=null){
            speakerTimer = 0;
            speaker.Play();
        }
    }
    if(battery.batteries.Count>0){
        if(wideMode){
            bool charging = battery.getNetChange()>=0;
            text+="\nBattery power: "+makeReadable(battery.getChargePercent()*100)+"% ("+(charging?"Charging":"Discharging")+" at "+makeReadableMetric(Math.Abs(battery.getNetChange()))+"W, "+(charging?"Charged":"Discharged")+" in "+makeReadableTime(battery.getSecondsUntilCharged())+")";
        }else{
            bool charging = battery.getNetChange()>=0;
            text+="\nBattery power: "+makeReadable(battery.getChargePercent()*100)+"%";
            text+="\nBattery "+(charging?"Charging":"Discharging")+" at "+makeReadableMetric(Math.Abs(battery.getNetChange()))+"W";
            text+="\nBattery "+(charging?"Charged":"Discharged")+" in "+makeReadableTime(battery.getSecondsUntilCharged());
        }
    }
    if(hydro.tanks.Count>0){
        if(wideMode){
            bool charging = hydro.getNetChange()>=0;
            text+="\nHydrogen level: "+makeReadable(hydro.getFillPercent()*100)+"% ("+(charging?"Filling":"Draining")+" at "+makeReadableMetric(Math.Abs(hydro.getNetChange()))+"L/s, "+(charging?"Full":"Empty")+" in "+makeReadableTime(hydro.getSecondsUntilCharged())+")";
        }else{
            text+="\nHydrogen level: "+makeReadable(hydro.getFillPercent()*100)+"%";
            bool charging = hydro.getNetChange()>=0;
            text+="\nHydrogen "+(charging?"Filling":"Draining")+" at "+makeReadableMetric(Math.Abs(hydro.getNetChange()))+"L/s";
            text+="\nHydrogen "+(charging?"Full":"Empty")+" in "+makeReadableTime(hydro.getSecondsUntilCharged());
        }
    }
    if(oxy.tanks.Count>0){
        if(wideMode){
            bool charging = oxy.getNetChange()>=0;
            text+="\nOxygen level: "+makeReadable(oxy.getFillPercent()*100)+"% ("+(charging?"Filling":"Draining")+" at "+makeReadableMetric(Math.Abs(oxy.getNetChange()))+"L/s, "+(charging?"Full":"Empty")+" in "+makeReadableTime(oxy.getSecondsUntilCharged())+")";
        }else{
            text+="\nOxygen level: "+makeReadable(oxy.getFillPercent()*100)+"%";
            bool charging = oxy.getNetChange()>=0;
            text+="\nOxygen "+(charging?"Filling":"Draining")+" at "+makeReadableMetric(Math.Abs(oxy.getNetChange()))+"L/s";
            text+="\nOxygen "+(charging?"Full":"Empty")+" in "+makeReadableTime(oxy.getSecondsUntilCharged());
        }
    }
    if(ice.containers.Count>0){
        text+="\nCargo: "+makeReadable(cargo.getFillPercent()*100)+"%";
        if(ice.getFillLevelI()>0){
            bool charging = ice.rateT>=0;
            if(wideMode){
                text+=", Ice: "+ice.getFillLevelI();
            }else{
                text+="\nIce: "+ice.getFillLevelI();
            }
            if(!charging){
                if(wideMode){
                    text+=" (Draining at "+makeReadableMetric(Math.Abs(ice.getNetChange()))+"/s, Empty in "+makeReadableTime(ice.getSecondsUntilCharged())+")";
                }else{
                    text+="\nIce Draining at "+makeReadableMetric(Math.Abs(ice.getNetChange()))+"/s";
                    text+="\nIce Empty in "+makeReadableTime(ice.getSecondsUntilCharged());
                }
            }
        }
    }
    if(jumpdrive.drives.Count>0){
        if(wideMode){
            text+="\nJump Drive Charge: "+makeReadable(jumpdrive.getChargePercent()*100)+"% (Charging at "+makeReadableMetric(Math.Abs(jumpdrive.getNetChange()))+"W, Charged in "+makeReadableTime(jumpdrive.getSecondsUntilCharged())+")";
        }else{
            text+="\nJump Drive Charge: "+makeReadable(jumpdrive.getChargePercent()*100)+"%";
            text+="\nJump Drive Charging at "+makeReadableMetric(Math.Abs(jumpdrive.getNetChange()))+"W";
            text+="\nJump Drive Charged in "+makeReadableTime(jumpdrive.getSecondsUntilCharged());
        }
    }
    if(panel==null){
        Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
        Me.GetSurface(0).WriteText(text, false);
        return;
    }
    Me.GetSurface(0).WriteText("", false);
    Me.GetSurface(0).ContentType = ContentType.NONE;
    panel.ContentType = ContentType.TEXT_AND_IMAGE;
    panel.WriteText(text, false);
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
String makeReadableMetric(double number){
    String prefix = "";
    if(number>1000){
        number/=1000;
        prefix = "k";
        if(number>1000){
            number/=1000;
            prefix = "M";
            if(number>1000){
                number/=1000;
                prefix = "G";
            }
        }
    }
    return makeReadable(number)+" "+prefix;
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
void updateAll(){
    cockpit = null;
    damageReport = null;
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(blocks);
    naturalGravity = artificialGravity = gravity = null;
    speed = null;
    velocities = null;
    mass = null;
    seaLevelElevation = surfaceElevation = null;
    planetPosition = null;
    controlled = false;
    panel = null;
    wideMode = false;
    speaker = null;
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
    thrusters.clear();
    foreach(IMyTerminalBlock b in blocks){
        if(b.CubeGrid!=Me.CubeGrid) continue;
        IMyThrust thrust = b as IMyThrust;
        if(thrust!=null&&(b.IsWorking||planningMode)){
           thrusters.add(cockpit, thrust);
        }
        if(!b.IsWorking)continue;
        IMyShipController controller = b as IMyShipController;
        if(controller!=null){
            if(controller.IsUnderControl){
                cockpit = controller;
            }
        }
        if(b.CustomData=="")continue;
        if(b.CustomData.Contains("Thruster LCD")){
            IMyTextPanel pan = b as IMyTextPanel;
            if(pan!=null)panel = pan;
            IMyTextSurfaceProvider provider = b as IMyTextSurfaceProvider;
            if(provider!=null)panel = provider.GetSurface(0);
            if(panel!=null){
                if(b.DefinitionDisplayNameText.Contains("Wide"))wideMode = true;
                else wideMode = !b.DefinitionDisplayNameText.ToLower().Contains("panel")&&!b.DefinitionDisplayNameText.ToLower().Contains("transparent");
                if(b.DefinitionDisplayNameText=="Cockpit")wideMode = false;
            }
        }
        IMySoundBlock sp = b as IMySoundBlock;
        if(sp!=null&&sp.CustomData.Contains("Damage Report"))speaker = sp;
    }
    foreach(IMyTerminalBlock b in blocks){
        if(b.CubeGrid!=Me.CubeGrid) continue;
        if(showDamageReport&&(b.DisassembleRatio<1||!b.IsFunctional)){
            if(wideMode){
                if(damageReport==null)damageReport = b.CustomName;
                else damageReport+=", "+b.CustomName;
            }else{
                damageReport+="\n"+b.CustomName;
            }
        }
    }
    if(cockpit==null)return;
    Vector3 move = cockpit.MoveIndicator;
    Vector2 rot = cockpit.RotationIndicator;
    float roll = cockpit.RollIndicator;
    if(move.X!=0||move.Y!=0||move.Z!=0||rot.X!=0||rot.Y!=0||roll!=0){
        mode = MODE_FLY;
    }
    if(cockpit.IsUnderControl)controlled = true;
    naturalGravity = cockpit.GetNaturalGravity();
    artificialGravity = cockpit.GetArtificialGravity();
    gravity = cockpit.GetTotalGravity();
    speed = cockpit.GetShipSpeed();
    velocities = cockpit.GetShipVelocities();
    mass = cockpit.CalculateShipMass();
    Vector3D planPos;
    if(cockpit.TryGetPlanetPosition(out planPos)){
        planet = getClosestPlanet(planPos);
    }else planet = SPACE;
    planetPosition = planPos;
    double seaElev;
    cockpit.TryGetPlanetElevation(MyPlanetElevation.Sealevel, out seaElev);
    seaLevelElevation = seaElev;
    double surfElev;
    cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out surfElev);
    surfaceElevation = surfElev;
}
public Planet getClosestPlanet(Vector3D pos){
    double earthDist = Vector3D.Distance(pos,EARTH.pos);
    double marsDist = Vector3D.Distance(pos,MARS.pos);
    double alienDist = Vector3D.Distance(pos,ALIEN.pos);
    double moonDist = Vector3D.Distance(pos,MOON.pos);
    double europaDist = Vector3D.Distance(pos,EUROPA.pos);
    double titanDist = Vector3D.Distance(pos,TITAN.pos);
    double tritonDist = Vector3D.Distance(pos,TRITON.pos);
    double closest = Math.Min(Math.Min(Math.Min(Math.Min(Math.Min(earthDist,Math.Min(titanDist, tritonDist)),marsDist),alienDist),moonDist),europaDist);
    if(closest==earthDist)return EARTH;
    if(closest==marsDist)return MARS;
    if(closest==alienDist)return ALIEN;
    if(closest==moonDist)return MOON;
    if(closest==europaDist)return EUROPA;
    if(closest==titanDist)return TITAN;
    if(closest==tritonDist)return TRITON;
    return SPACE;
}
public class Planet{
    public double radius;
    public double upperAtmosphere;
    public double lowerAtmosphere;
    public double gravity;
    public double maxHillRadius;
    public Vector3D pos;
    public String name;
    public Program p;
    public Planet(Program p, String name, Vector3D center, double radius, double lowerAtmosphere, double upperAtmosphere, double gravity, double maxHillRadius){
        this.p = p;
        this.name = name;
        pos = center;
        this.radius = radius;
        this.upperAtmosphere = upperAtmosphere;
        this.lowerAtmosphere = lowerAtmosphere;
        this.gravity = gravity;
        this.maxHillRadius = maxHillRadius;
    }
    public double getGravity(double altitude){
        double grav = Math.Min(gravity,gravity*Math.Pow((radius+altitude)/maxHillRadius,-7));
        if(grav<.05)return 0;
        return grav;
    }
    public int CompareTo(Planet other){
        double dist = Vector3D.Distance(pos,p.Me.GetPosition())-radius-upperAtmosphere;
        double otherDist = Vector3D.Distance(other.pos,p.Me.GetPosition())-radius-upperAtmosphere;
        return dist.CompareTo(otherDist);
    }
}
public class ThrusterConfiguration{
    public IonThrusterConfiguration ion = new IonThrusterConfiguration();
    public AtmosphericThrusterConfiguration atmospheric = new AtmosphericThrusterConfiguration();
    public HydrogenThrusterConfiguration hydrogen = new HydrogenThrusterConfiguration();
    public Program program;
    public ThrusterConfiguration(Program p){
        this.program = p;
    }
    public void add(IMyShipController referenceCockpit, IMyThrust thruster){
        if(thruster.DefinitionDisplayNameText.Contains("Hydrogen")){
            hydrogen.add(referenceCockpit, thruster);
        }
        if(thruster.DefinitionDisplayNameText.Contains("Ion")){
            ion.add(referenceCockpit, thruster);
        }
        if(thruster.DefinitionDisplayNameText.Contains("Atmospheric")){
            atmospheric.add(referenceCockpit, thruster);
        }
    }
    public void clear(){
        ion.clear();
        atmospheric.clear();
        hydrogen.clear();
    }
    public String getMaxDirectionS(Planet planet, double altitude){
        if(program.preferredDirection!=Vector3I.Zero){
            return program.preferredDirectionS;
        }
        double up = ion.getThrust(Vector3I.Up, planet, altitude)+atmospheric.getThrust(Vector3I.Up, planet, altitude)+hydrogen.getThrust(Vector3I.Up, planet, altitude);
        double down = ion.getThrust(Vector3I.Down, planet, altitude)+atmospheric.getThrust(Vector3I.Down, planet, altitude)+hydrogen.getThrust(Vector3I.Down, planet, altitude);
        double left = ion.getThrust(Vector3I.Left, planet, altitude)+atmospheric.getThrust(Vector3I.Left, planet, altitude)+hydrogen.getThrust(Vector3I.Left, planet, altitude);
        double right = ion.getThrust(Vector3I.Right, planet, altitude)+atmospheric.getThrust(Vector3I.Right, planet, altitude)+hydrogen.getThrust(Vector3I.Right, planet, altitude);
        double forward = ion.getThrust(Vector3I.Forward, planet, altitude)+atmospheric.getThrust(Vector3I.Forward, planet, altitude)+hydrogen.getThrust(Vector3I.Forward, planet, altitude);
        double backward = ion.getThrust(Vector3I.Backward, planet, altitude)+atmospheric.getThrust(Vector3I.Backward, planet, altitude)+hydrogen.getThrust(Vector3I.Backward, planet, altitude);
        double max = Math.Max(up,Math.Max(down,Math.Max(left,Math.Max(right,Math.Max(forward,backward)))));
        if(max<1)return "Unknown";
        if(max==down)return "Up";
        if(max==backward)return "Forward";
        if(max==forward)return "Backward";
        if(max==up)return "Down";
        if(max==left)return "Right";
        if(max==right)return "Left";
        return "Unknown";
    }
    public Vector3I getMaxDirection(Planet planet, double altitude){
        if(program.preferredDirection!=Vector3I.Zero){
            return program.preferredDirection;
        }
        double up = ion.getThrust(Vector3I.Up, planet, altitude)+atmospheric.getThrust(Vector3I.Up, planet, altitude)+hydrogen.getThrust(Vector3I.Up, planet, altitude);
        double down = ion.getThrust(Vector3I.Down, planet, altitude)+atmospheric.getThrust(Vector3I.Down, planet, altitude)+hydrogen.getThrust(Vector3I.Down, planet, altitude);
        double left = ion.getThrust(Vector3I.Left, planet, altitude)+atmospheric.getThrust(Vector3I.Left, planet, altitude)+hydrogen.getThrust(Vector3I.Left, planet, altitude);
        double right = ion.getThrust(Vector3I.Right, planet, altitude)+atmospheric.getThrust(Vector3I.Right, planet, altitude)+hydrogen.getThrust(Vector3I.Right, planet, altitude);
        double forward = ion.getThrust(Vector3I.Forward, planet, altitude)+atmospheric.getThrust(Vector3I.Forward, planet, altitude)+hydrogen.getThrust(Vector3I.Forward, planet, altitude);
        double backward = ion.getThrust(Vector3I.Backward, planet, altitude)+atmospheric.getThrust(Vector3I.Backward, planet, altitude)+hydrogen.getThrust(Vector3I.Backward, planet, altitude);
        double max = Math.Max(up,Math.Max(down,Math.Max(left,Math.Max(right,Math.Max(forward,backward)))));
        if(max<1)return Vector3I.Zero;
        if(max==down)return Vector3I.Down;
        if(max==backward)return Vector3I.Backward;
        if(max==forward)return Vector3I.Forward;
        if(max==up)return Vector3I.Up;
        if(max==left)return Vector3I.Left;
        if(max==right)return Vector3I.Right;
        return Vector3I.Zero;
    }
    public double getThrust(Vector3I direction, Planet planet, double altitude){
        return ion.getThrust(direction, planet, altitude)+atmospheric.getThrust(direction, planet, altitude)+hydrogen.getThrust(direction, planet, altitude);
    }
}
public class IonThrusterConfiguration : SpecificThrusterConfiguration{
    public double getThrust(Vector3I direction, Planet planet, double altitude){
        return getRawThrust(direction)*getIonEfficiency(planet, altitude);
    }
    public double getIonEfficiency(Planet planet, double altitude){
        if(planet.upperAtmosphere==0)return 1;
        return Math.Max(0.2,Math.Min(1,(0.8/(planet.upperAtmosphere-planet.lowerAtmosphere))*(altitude-planet.lowerAtmosphere)+0.2));
    }
}
public class AtmosphericThrusterConfiguration : SpecificThrusterConfiguration{
    public double getThrust(Vector3I direction, Planet planet, double altitude){
        return getRawThrust(direction)*getAtmosphericEfficiency(planet, altitude);
    }
    public double getAtmosphericEfficiency(Planet planet, double altitude){
        if(planet.upperAtmosphere==0)return 0;
        return Math.Max(0,Math.Min(1,(-.8/((planet.upperAtmosphere-planet.lowerAtmosphere)*.56))*(altitude-planet.lowerAtmosphere)+1));
    }
}
public class HydrogenThrusterConfiguration : SpecificThrusterConfiguration{
    public double getThrust(Vector3I direction, Planet planet, double altitude){
        return getThrust(direction);
    }
    public double getThrust(Vector3I direction){
        return getRawThrust(direction);
    }
}
public class SpecificThrusterConfiguration{
    public double up;
    public double down;
    public double left;
    public double right;
    public double forward;
    public double backward;
    public void clear(){
        up = down = left = right = forward = backward = 0;
    }
    public void add(IMyShipController referenceCockpit, IMyThrust thruster){
        Vector3I direction = thruster.GridThrustDirection;
        if(direction==Vector3I.Zero||thrusterDirectionOverride){
            if(referenceCockpit==null){
                if(thruster.Orientation.Forward==Base6Directions.Direction.Up)up+=thruster.MaxThrust;
                if(thruster.Orientation.Forward==Base6Directions.Direction.Down)down+=thruster.MaxThrust;
                if(thruster.Orientation.Forward==Base6Directions.Direction.Left)left+=thruster.MaxThrust;
                if(thruster.Orientation.Forward==Base6Directions.Direction.Right)right+=thruster.MaxThrust;
                if(thruster.Orientation.Forward==Base6Directions.Direction.Forward)forward+=thruster.MaxThrust;
                if(thruster.Orientation.Forward==Base6Directions.Direction.Backward)backward+=thruster.MaxThrust;
                return;
            }
            Base6Directions.Direction Up = referenceCockpit.Orientation.Up;
            Base6Directions.Direction Forward = referenceCockpit.Orientation.Forward;
            Base6Directions.Direction Left = referenceCockpit.Orientation.Left;
            Base6Directions.Direction Backward = new MyBlockOrientation(Left, Up).Left;
            Base6Directions.Direction Right = new MyBlockOrientation(Backward, Up).Left;
            Base6Directions.Direction Down = new MyBlockOrientation(Forward, Left).Left;
            if(thruster.Orientation.Forward==Up)up+=thruster.MaxThrust;
            if(thruster.Orientation.Forward==Down)down+=thruster.MaxThrust;
            if(thruster.Orientation.Forward==Left)left+=thruster.MaxThrust;
            if(thruster.Orientation.Forward==Right)right+=thruster.MaxThrust;
            if(thruster.Orientation.Forward==Forward)forward+=thruster.MaxThrust;
            if(thruster.Orientation.Forward==Backward)backward+=thruster.MaxThrust;
        }else{
            if(direction==Vector3I.Up)up+=thruster.MaxThrust;
            if(direction==Vector3I.Down)down+=thruster.MaxThrust;
            if(direction==Vector3I.Left)left+=thruster.MaxThrust;
            if(direction==Vector3I.Right)right+=thruster.MaxThrust;
            if(direction==Vector3I.Forward)forward+=thruster.MaxThrust;
            if(direction==Vector3I.Backward)backward+=thruster.MaxThrust;
        }
    }
    public Vector3I getMaxDirection(){
        double max = Math.Max(up,Math.Max(down,Math.Max(left,Math.Max(right,Math.Max(forward,backward)))));
        if(max==down)return Vector3I.Down;
        if(max==backward)return Vector3I.Backward;
        if(max==forward)return Vector3I.Forward;
        if(max==up)return Vector3I.Up;
        if(max==left)return Vector3I.Left;
        if(max==right)return Vector3I.Right;
        return Vector3I.Zero;
    }
    public double getRawThrust(Vector3I direction){
        if(direction==Vector3I.Zero)return 0;
        if(direction==Vector3I.Up)return up;
        if(direction==Vector3I.Down)return down;
        if(direction==Vector3I.Left)return left;
        if(direction==Vector3I.Right)return right;
        if(direction==Vector3I.Forward)return forward;
        if(direction==Vector3I.Backward)return backward;
        return 0;
    }
}
public class InventoryContainer{
    public List<IMyTerminalBlock> containers = new List<IMyTerminalBlock>();
    Program p;
    double lastFill = 0;
    double lastFillT = 0;
    double rate = 0;
    public double rateT = 0;
    String type = "";
    public int interval = 10;
    public int timer = 0;
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
        timer++;
        if(timer>=interval){
            timer-=interval;
        }else return;
        double fill = getFillLevel();
        double diff = fill-lastFill;
        rate = diff*6*interval;
        lastFill = fill;
        double fillT = getFillLevelI();
        double diffT = fillT-lastFillT;
        rateT = diffT*6*interval;
        lastFillT = fillT;
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