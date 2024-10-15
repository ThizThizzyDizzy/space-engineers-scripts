int maxAmmo = 1000;
int minAmmo = 100;
public Program(){
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}
public void Main(){
    List<IMyTerminalBlock> lst = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(lst);
    foreach(IMyTerminalBlock b in lst){
        IMyLargeInteriorTurret turret = b as IMyLargeInteriorTurret;
        if(b!=null){
            int vol = turret.getInventory().CurrentVolume.ToIntSafe();
        }
    }
}
void display(String text){
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