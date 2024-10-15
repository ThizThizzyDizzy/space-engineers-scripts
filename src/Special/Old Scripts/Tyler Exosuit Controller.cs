IMyShipController cockpit;
IMyShipController leftCockpit;
IMyShipController rightCockpit;
IMyMotorStator leftLegRot;
IMyMotorStator leftUpperLeg;
IMyMotorStator leftKnee;
IMyMotorStator leftFootRot;
IMyMotorStator rightLegRot;
IMyMotorStator rightUpperLeg;
IMyMotorStator rightKnee;
IMyMotorStator rightFootRot;
IMyMotorStator waist;
IMyMotorStator leftShoulder;
IMyMotorStator leftArmRot;
IMyMotorStator leftElbow;
IMyMotorStator leftWrist;
IMyMotorStator rightShoulder;
IMyMotorStator rightArmRot;
IMyMotorStator rightElbow;
IMyMotorStator rightWrist;
IMyCameraBlock leftFootFront;
IMyCameraBlock leftFootBack;
IMyCameraBlock rightFootFront;
IMyCameraBlock rightFootBack;
IMyLandingGear leftFoot;
IMyLandingGear rightFoot;
int leftLegRotDirection = 1;//outward
int leftUpperLegDirection = -1;//straight
int leftKneeDirection = -1;//straight
int leftFootRotDirection = 1;//tilt up
int rightLegRotDirection = -1;//outward
int rightUpperLegDirection = 1;//straight
int rightKneeDirection = 1;//straight
int rightFootRotDirection = -1;//tilt up
int waistDirection = 1;//to the right
int leftShoulderDirection = 1;//forward
int leftArmRotDirection = 1;//outward
int leftElbowDirection = -1;//straight
int leftWristDirection = 1;//spinny counter-clockwise
int rightShoulderDirection = -1;//forward
int rightArmRotDirection = -1;//outward
int rightElbowDirection = 1;//straight
int rightWristDirection = -1;//spinny clockwise
float leftLegBend = 0;
float rightLegBend = 0;
float bodyPitch = 0;
float bodyYaw = 0;
float speedThreshold = 1;
float speedy = 2.5f;
float globalSpeed = 1;
float threshold = .25f;
float mouseSensitivity = 2.5f;
float distThreshold = -1f;
float raycastDist = 10f;
bool oops = false;
String command = null;
List<String> commands = new List<String>();
public Program(){
    commands.Add("Sit");
    commands.Add("Left");
    commands.Add("Right");
    commands.Add("Turn");
    commands.Add("Walk");
    Echo("Starting up...");
    cockpit = findController("Exosuit Controller");
    leftCockpit = findController("Left Exosuit Controller");
    rightCockpit = findController("Right Exosuit Controller");
    leftLegRot = findRotor("Left Leg Rotation", ref leftLegRotDirection);
    leftUpperLeg = findRotor("Left Upper Leg", ref leftUpperLegDirection);
    leftKnee = findRotor("Left Knee", ref leftKneeDirection);
    leftFootRot = findRotor("Left Foot Rotation", ref leftFootRotDirection);
    rightLegRot = findRotor("Right Leg Rotation", ref rightLegRotDirection);
    rightUpperLeg = findRotor("Right Upper Leg", ref rightUpperLegDirection);
    rightKnee = findRotor("Right Knee", ref rightKneeDirection);
    rightFootRot = findRotor("Right Foot Rotation", ref rightFootRotDirection);
    leftFootFront = findCamera("Left Foot Front");
    leftFootBack = findCamera("Left Foot Back");
    rightFootFront = findCamera("Right Foot Front");
    rightFootBack = findCamera("Right Foot Back");
    waist = findRotor("Waist", ref waistDirection);
    leftShoulder = findRotor("Left Shoulder", ref leftShoulderDirection);
    leftArmRot = findRotor("Left Arm Rotation", ref leftArmRotDirection);
    leftElbow = findRotor("Left Elbow", ref leftElbowDirection);
    leftWrist = findRotor("Left Wrist", ref leftWristDirection);
    rightShoulder = findRotor("Right Shoulder", ref rightShoulderDirection);
    rightArmRot = findRotor("Right Arm Rotation", ref rightArmRotDirection);
    rightElbow = findRotor("Right Elbow", ref rightElbowDirection);
    rightWrist = findRotor("Right Wrist", ref rightWristDirection);
    leftFoot = findLandingGear("Left Foot");
    rightFoot = findLandingGear("Right Foot");
    if(oops)return;
    Echo("Startup Complete!");
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
}
public void Main(String arg){
    if(arg=="Reset")command = null;
    else if(commands.Contains(arg))command = arg;
    String output = "Mode: "+(command==null?"Normal":command)+"\n";
    if(command=="Left"&&leftCockpit.IsUnderControl)command = null;
    if(command=="Right"&&rightCockpit.IsUnderControl)command = null;
    Echo("Running");
    if(command!=null)Echo("OVERRIDE COMMAND: "+command);
    Vector3 move = cockpit.MoveIndicator;
    Vector2 rot = cockpit.RotationIndicator;
    float roll = cockpit.RollIndicator;
    if(command=="TLock"){
        IMyLandingGear foot;
        IMyLandingGear otherFoot;
        IMyCameraBlock footFront;
        IMyCameraBlock footBack;
        IMyMotorStator footRot;
        int footRotDirection;
        if(move.X==0){
            command = "Turn";
            return;
        }
        if(leftFoot.LockMode==LandingGearMode.Locked){//lock right
            foot = rightFoot;
            otherFoot = leftFoot;
            footFront = rightFootFront;
            footBack = rightFootBack;
            footRot = rightFootRot;
            footRotDirection = rightFootRotDirection;
        }else{//lock left
            foot = leftFoot;
            otherFoot = rightFoot;
            footFront = leftFootFront;
            footBack = leftFootBack;
            footRot = leftFootRot;
            footRotDirection = leftFootRotDirection;
        }
        float front = 1000;
        float back = 1000;
        try{
            front = (float)Vector3D.Distance((Vector3D)footFront.Raycast(Math.Min(raycastDist,footFront.AvailableScanRange), 0, 0).HitPosition, footFront.GetPosition())-.75f;
            back = (float)Vector3D.Distance((Vector3D)footBack.Raycast(Math.Min(raycastDist,footBack.AvailableScanRange), 0, 0).HitPosition, footBack.GetPosition())-.75f;
        }catch{}
        float diff = back-front;
        footRot.TargetVelocityRPM = footRotDirection*Math.Min(1,Math.Max(-1,diff));
        output+=Math.Round(front*10)+" "+Math.Round(back*10);
        if(diff<threshold){
            if((front+back)/2<distThreshold){
                if(leftFoot.LockMode==LandingGearMode.Locked)rightLegBend+=1/1800f*globalSpeed;
                else leftLegBend+=1/1800f*globalSpeed;
            }
            if(foot.LockMode!=LandingGearMode.Unlocked){
                foot.Lock();
                if(foot.LockMode==LandingGearMode.Locked){
                    otherFoot.Unlock();
                    command = "Turn";
                    return;
                }
            }
        }else if((front+back)/2>distThreshold){
            if(leftFoot.LockMode==LandingGearMode.Locked)rightLegBend-=1/900f*globalSpeed;
            else leftLegBend-=1/900f*globalSpeed;
        }
        rotate(leftUpperLeg, 1-leftLegBend+bodyPitch, leftUpperLegDirection, .5f);
        rotate(leftKnee, 1-leftLegBend, leftKneeDirection, 1);
        rotate(leftFootRot, leftLegBend*80*leftFootRotDirection, .5f);
        rotate(rightUpperLeg, 1-rightLegBend+bodyPitch, rightUpperLegDirection, .5f);
        rotate(rightKnee, 1-rightLegBend, rightKneeDirection, 1);
        rotate(rightFootRot, rightLegBend*80*rightFootRotDirection, .5f);
        ((IMyCockpit)cockpit).GetSurface(0).WriteText(output);
        return;
    }
    if(command=="WLock"){
        IMyLandingGear foot;
        IMyLandingGear otherFoot;
        IMyCameraBlock footFront;
        IMyCameraBlock footBack;
        IMyMotorStator footRot;
        int footRotDirection;
        if(move.X==0){
            command = "Walk";
            return;
        }
        if(leftFoot.LockMode==LandingGearMode.Locked){//lock right
            foot = rightFoot;
            otherFoot = leftFoot;
            footFront = rightFootFront;
            footBack = rightFootBack;
            footRot = rightFootRot;
            footRotDirection = rightFootRotDirection;
        }else{//lock left
            foot = leftFoot;
            otherFoot = rightFoot;
            footFront = leftFootFront;
            footBack = leftFootBack;
            footRot = leftFootRot;
            footRotDirection = leftFootRotDirection;
        }
        float front = 1000;
        float back = 1000;
        try{
            front = (float)Vector3D.Distance((Vector3D)footFront.Raycast(Math.Min(raycastDist,footFront.AvailableScanRange), 0, 0).HitPosition, footFront.GetPosition())-.75f;
            back = (float)Vector3D.Distance((Vector3D)footBack.Raycast(Math.Min(raycastDist,footBack.AvailableScanRange), 0, 0).HitPosition, footBack.GetPosition())-.75f;
        }catch{}
        float diff = back-front;
        footRot.TargetVelocityRPM = footRotDirection*Math.Min(1,Math.Max(-1,diff));
        output+=Math.Round(front*10)+" "+Math.Round(back*10);
        if(diff<threshold){
            if((front+back)/2<distThreshold){
                if(leftFoot.LockMode==LandingGearMode.Locked)rightLegBend+=1/1800f*globalSpeed;
                else leftLegBend+=1/1800f*globalSpeed;
            }
            if(foot.LockMode!=LandingGearMode.Unlocked){
                foot.Lock();
                if(foot.LockMode==LandingGearMode.Locked){
                    otherFoot.Unlock();
                    command = "Walk";
                    return;
                }
            }
        }else if((front+back)/2>distThreshold){
            if(leftFoot.LockMode==LandingGearMode.Locked)rightLegBend-=1/900f*globalSpeed;
            else leftLegBend-=1/900f*globalSpeed;
        }
        rotate(leftUpperLeg, 1-leftLegBend+bodyPitch, leftUpperLegDirection, .5f);
        rotate(leftKnee, 1-leftLegBend, leftKneeDirection, 1);
        rotate(leftFootRot, leftLegBend*80*leftFootRotDirection, .5f);
        rotate(rightUpperLeg, 1-rightLegBend+bodyPitch, rightUpperLegDirection, .5f);
        rotate(rightKnee, 1-rightLegBend, rightKneeDirection, 1);
        rotate(rightFootRot, rightLegBend*80*rightFootRotDirection, .5f);
        ((IMyCockpit)cockpit).GetSurface(0).WriteText(output);
        return;
    }
    if(command=="Sit"){
        rotate(leftUpperLeg, 0, leftUpperLegDirection, .5f);
        rotate(rightUpperLeg, 0, rightUpperLegDirection, .5f);
        rotate(leftKnee, 1, leftKneeDirection, 1);
        rotate(rightKnee, 1, rightKneeDirection, 1);
        rotate(leftFootRot, -90*leftFootRotDirection, .5f);
        rotate(rightFootRot, -90*rightFootRotDirection, .5f);
        rotate(leftLegRot, 0, .5f);
        rotate(rightLegRot, 0, .5f);
        rotate(waist, 0, .5f);
        rotate(leftShoulder, 0, .5f);
        rotate(leftArmRot, 0, .5f);
        rotate(leftElbow, .5f, leftElbowDirection, .5f);
        rotate(leftWrist, 0, .5f);
        rotate(rightShoulder, 0, .5f);
        rotate(rightArmRot, 0, .5f);
        rotate(rightElbow, .5f, rightElbowDirection, .5f);
        rotate(rightWrist, 0, .5f);
        return;
    }
    if(command==null){
        if(move.Y!=0){
            float d = move.Y/Math.Abs(move.Y);
            leftLegBend-=d/1800*globalSpeed;
            rightLegBend-=d/1800*globalSpeed;
        }
        if(rot.X!=0){
            float d = rot.X/Math.Abs(rot.X);
            bodyPitch-=d/1800*globalSpeed*mouseSensitivity;
        }
        if(rot.Y!=0){
            float d = rot.Y/Math.Abs(rot.Y);
            bodyYaw+=d/1800*globalSpeed*mouseSensitivity;
        }
        leftLegBend = Math.Max(0,Math.Min(1,leftLegBend));
        rightLegBend = Math.Max(0,Math.Min(1,rightLegBend));
        bodyPitch = Math.Max(-1,Math.Min(.8f,bodyPitch));
        bodyYaw = Math.Max(-1,Math.Min(1,bodyYaw));
        Echo("Left leg: "+Math.Round(leftLegBend*1000)/10d+"%");
        Echo("Right leg: "+Math.Round(rightLegBend*1000)/10d+"%");
        Echo("Body: "+Math.Round(bodyPitch*1000)/10d+"% "+Math.Round(bodyYaw*1000)/10d+"%");
        if(!leftCockpit.IsUnderControl){
            rotate(leftShoulder, 0, .5f);
            rotate(leftArmRot, 0, .5f);
            rotate(leftElbow, 1f-leftLegBend/2, leftElbowDirection, .5f);
            rotate(leftWrist, 0, .5f);
        }
        if(!rightCockpit.IsUnderControl){
            rotate(rightShoulder, 0, .5f);
            rotate(rightArmRot, 0, .5f);
            rotate(rightElbow, 1f-rightLegBend/2, rightElbowDirection, .5f);
            rotate(rightWrist, 0, .5f);
        }
    }
    if(command=="Turn"){
        leftLegBend = rightLegBend = .3f;
        if(leftFoot.LockMode!=LandingGearMode.Locked||rightFoot.LockMode!=LandingGearMode.Locked){
            float x = move.X==0?0:move.X/Math.Abs(move.X);
            int flip = rightFoot.LockMode==LandingGearMode.Locked?-1:1;
            if(!rotate(leftLegRot, (x+1)/4+.25f, flip*leftLegRotDirection, .5f)||!rotate(rightLegRot, (x+1)/4+.25f, flip*rightLegRotDirection, .5f)){
                if(leftFoot.LockMode==LandingGearMode.Locked)rightLegBend = .5f;
                else leftLegBend = .5f;
            }else{
                if(x!=0)command = "TLock";
                else{rightFoot.Lock();leftFoot.Lock();}
            }
        }else{
            rightFoot.Unlock();
        }
    }
    if(command=="Walk"){
        leftLegBend = rightLegBend = .3f;
        if(leftFoot.LockMode!=LandingGearMode.Locked||rightFoot.LockMode!=LandingGearMode.Locked){
            float x = move.X==0?0:move.X/Math.Abs(move.X);
            int flip = rightFoot.LockMode==LandingGearMode.Locked?-1:1;
            if(!rotate(leftLegRot, (x+1)/4+.25f, flip*leftLegRotDirection, .5f)||!rotate(rightLegRot, (x+1)/4+.25f, flip*rightLegRotDirection, .5f)){
                if(leftFoot.LockMode==LandingGearMode.Locked)rightLegBend = .5f;
                else leftLegBend = .5f;
            }else{
                if(x!=0)command = "WLock";
                else{rightFoot.Lock();leftFoot.Lock();}
            }
        }else{
            rightFoot.Unlock();
        }
    }
    if(command=="Left"||leftCockpit.IsUnderControl){
        float x = move.X==0?0:move.X/Math.Abs(move.X);
        float y = move.Y==0?0:move.Y/Math.Abs(move.Y);
        float z = move.Z==0?0:move.Z/Math.Abs(move.Z);
        float rol = roll==0?0:roll/Math.Abs(roll);
        if(leftCockpit.IsUnderControl){
            x = leftCockpit.MoveIndicator.X==0?0:leftCockpit.MoveIndicator.X/Math.Abs(leftCockpit.MoveIndicator.X);
            y = leftCockpit.MoveIndicator.Y==0?0:leftCockpit.MoveIndicator.Y/Math.Abs(leftCockpit.MoveIndicator.Y);
            z = leftCockpit.MoveIndicator.Z==0?0:leftCockpit.MoveIndicator.Z/Math.Abs(leftCockpit.MoveIndicator.Z);
            rol = leftCockpit.RollIndicator==0?0:leftCockpit.RollIndicator/Math.Abs(leftCockpit.RollIndicator);
        }
        leftShoulder.TargetVelocityRPM = 2*leftShoulderDirection*(-z/2);//-y/2);
        leftElbow.TargetVelocityRPM = 2*leftElbowDirection*(-z/2-y);
        leftArmRot.TargetVelocityRPM = 2*leftArmRotDirection*(-x/2);
        leftWrist.TargetVelocityRPM = -rol*2;
        rightShoulder.TargetVelocityRPM = 0;
        rightElbow.TargetVelocityRPM = 0;
        rightArmRot.TargetVelocityRPM = 0;
        rightWrist.TargetVelocityRPM = 0;
    }
    if(command=="Right"||rightCockpit.IsUnderControl){
        float x = move.X==0?0:move.X/Math.Abs(move.X);
        float y = move.Y==0?0:move.Y/Math.Abs(move.Y);
        float z = move.Z==0?0:move.Z/Math.Abs(move.Z);
        float rol = roll==0?0:roll/Math.Abs(roll);
        if(rightCockpit.IsUnderControl){
            x = rightCockpit.MoveIndicator.X==0?0:rightCockpit.MoveIndicator.X/Math.Abs(rightCockpit.MoveIndicator.X);
            y = rightCockpit.MoveIndicator.Y==0?0:rightCockpit.MoveIndicator.Y/Math.Abs(rightCockpit.MoveIndicator.Y);
            z = rightCockpit.MoveIndicator.Z==0?0:rightCockpit.MoveIndicator.Z/Math.Abs(rightCockpit.MoveIndicator.Z);
            rol = rightCockpit.RollIndicator==0?0:rightCockpit.RollIndicator/Math.Abs(rightCockpit.RollIndicator);
        }
        rightShoulder.TargetVelocityRPM = 2*rightShoulderDirection*(-z/2);//-y/2);
        rightElbow.TargetVelocityRPM = 2*rightElbowDirection*(-z/2-y);
        rightArmRot.TargetVelocityRPM = 2*-rightArmRotDirection*(-x/2);
        rightWrist.TargetVelocityRPM = -rol*2;
        leftShoulder.TargetVelocityRPM = 0;
        leftElbow.TargetVelocityRPM = 0;
        leftArmRot.TargetVelocityRPM = 0;
        leftWrist.TargetVelocityRPM = 0;
    }
    rotate(leftUpperLeg, 1-leftLegBend+bodyPitch, leftUpperLegDirection, .5f);
    rotate(leftKnee, 1-leftLegBend, leftKneeDirection, 1);
    rotate(leftFootRot, leftLegBend*80*leftFootRotDirection, .5f);
    rotate(rightUpperLeg, 1-rightLegBend+bodyPitch, rightUpperLegDirection, .5f);
    rotate(rightKnee, 1-rightLegBend, rightKneeDirection, 1);
    rotate(rightFootRot, rightLegBend*80*rightFootRotDirection, .5f);
    rotate(waist, (bodyYaw+1)/2, waistDirection, .5f);
    ((IMyCockpit)cockpit).GetSurface(0).WriteText(output);
}
bool rotate(IMyMotorStator rotor, float percent, int positiveDirection, float speed){
    float angle = (rotor.UpperLimitDeg-rotor.LowerLimitDeg)*percent;
    if(positiveDirection>=0)angle = rotor.LowerLimitDeg+angle;
    else angle = rotor.UpperLimitDeg-angle;
    return rotate(rotor, angle, speed);
}
bool rotate(IMyMotorStator rotor, float angle, float speed){
    Echo(rotor.CustomData+" "+angle);
    speed*=globalSpeed;
    double rotorAngle = rotor.Angle*180/Math.PI;
    while(rotorAngle>180)rotorAngle-=360;
    while(rotorAngle<-180)rotorAngle+=360;
    while(angle>180)angle-=360;
    while(angle<-180)angle+=360;
    if(rotorAngle+threshold<angle){
        if(rotorAngle+speedThreshold<angle)speed*=speedy;
        rotor.TargetVelocityRPM = speed;
        return false;
    }else if(rotorAngle-threshold>angle){
        if(rotorAngle-speedThreshold>angle)speed*=speedy;
        rotor.TargetVelocityRPM = -speed;
        return false;
    }else{
        rotor.TargetVelocityRPM = 0;
        return true;
    }
}
IMyShipController findController(String name){
    IMyShipController controller = findBlock(name) as IMyShipController;
    if(controller==null){Echo("Can't find controller: "+name+"!");oops = true;}
    return controller;
}
IMyLandingGear findLandingGear(String name){
    IMyLandingGear gear = findBlock(name) as IMyLandingGear;
    if(gear==null){Echo("Can't find landing gear: "+name+"!");oops = true;}
    return gear;
}
IMyMotorStator findRotor(String name, ref int direction){
    IMyMotorStator rotor = findSpinnyRotor(name, ref direction);
    if(rotor==null)return null;
    if(rotor.LowerLimitDeg==float.MinValue){Echo("Rotor "+name+" has no lower limit!");oops = true;return null;}
    if(rotor.UpperLimitDeg==float.MinValue){Echo("Rotor "+name+" has no upper limit!");oops = true;return null;}
    return rotor;
}
IMyMotorStator findSpinnyRotor(String name, ref int direction){
    IMyMotorStator rotor = findBlock(name) as IMyMotorStator;
    if(rotor==null){Echo("Can't find rotor: "+name+"!");oops = true;return null;}
    if(rotor.CustomData.Contains("Reverse"))direction*=-1;
    return rotor;
}
IMyCameraBlock findCamera(String name){
    IMyCameraBlock camera = findBlock(name) as IMyCameraBlock;
    if(camera==null){Echo("Can't find camera: "+name+"!");oops = true;return null;}
    else camera.EnableRaycast = true;
    return camera;
}
IMyTerminalBlock findBlock(String name){
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(blocks);
    foreach(IMyTerminalBlock b in blocks){
        if(!b.IsWorking)continue;
        String data = b.CustomData;
        if(data.Contains("\n"))data = data.Substring(0, data.IndexOf("\n"));
        if(data==name)return b;
    }
    return null;
}