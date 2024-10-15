IMyCameraBlock camera;
public Program(){
    List<IMyTerminalBlock> lst = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(lst);
    foreach(IMyTerminalBlock b in lst){
        if(!b.IsFunctional)continue;
        if(b.CustomData.Equals("Rangefinder")&&b as IMyCameraBlock!=null)camera = b as IMyCameraBlock;
    }
    if(camera==null)Echo("Camera \"Rangefinder\" not found!");
    else{
        Echo("Ready!");
        camera.EnableRaycast = true;
    }
}
public void Main(String arg){
    if(camera!=null){
        if(arg=="Check"){
            output("Range: "+makeReadableMetric(camera.AvailableScanRange)+"m");
            return;
        }
        Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
        double range = camera.AvailableScanRange;
        try{
            MyDetectedEntityInfo i = camera.Raycast(range, 0, 0);
            output(makeReadableMetric(Vector3D.Distance((Vector3D)i.HitPosition,camera.GetPosition()))+"m");
        }catch{
            output("Got nothin' for "+makeReadableMetric(range)+"m");
        }
    }
}
void output(String text){
    Me.GetSurface(0).WriteText(text);
    Echo(text);
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