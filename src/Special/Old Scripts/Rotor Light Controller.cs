IMyMotorStator pitch;
IMyMotorStator yaw;
IMyTerminalBlock light;
IMySensorBlock sensor;
float targetPitch = 0;
float targetYaw = 0;
float threshold = 1;
float speedThreshold = 10;
public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    pitch = GridTerminalSystem.GetBlockWithName("Light Pitch") as IMyMotorStator;
    yaw = GridTerminalSystem.GetBlockWithName("Light Yaw") as IMyMotorStator;
    light = GridTerminalSystem.GetBlockWithName("Lamp");
    sensor = GridTerminalSystem.GetBlockWithName("Light Sensor") as IMySensorBlock;
}
public void Main(String arg){
    if(arg=="Reset")arg = "0 0";
    if(arg=="Rover")arg = "-75 135";
    if(arg.Contains(" ")){
        targetPitch = Single.Parse(arg.Split(' ')[0]);
        targetYaw = Single.Parse(arg.Split(' ')[1]);
    }
    Echo("Running");
    rotate(pitch, -targetPitch, 1);
    rotate(yaw, 360-targetYaw, 1);
}
bool rotate(IMyMotorStator rotor, float angle, float speed){
    double rotorAngle = rotor.Angle*180/Math.PI;
    while(rotorAngle>180)rotorAngle-=360;
    while(rotorAngle<-180)rotorAngle+=360;
    while(angle>180)angle-=360;
    while(angle<-180)angle+=360;
    Echo(rotor.CustomData+" "+angle);
    if(rotorAngle+threshold<angle){
        if(rotorAngle+speedThreshold<angle)speed*=4.5f;
        rotor.TargetVelocityRPM = speed;
        return false;
    }else if(rotorAngle-threshold>angle){
        if(rotorAngle-speedThreshold>angle)speed*=4.5f;
        rotor.TargetVelocityRPM = -speed;
        return false;
    }else{
        rotor.TargetVelocityRPM = 0;
        return true;
    }
}