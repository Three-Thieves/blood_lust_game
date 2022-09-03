using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using SandboxEditor;

[Library("bl_ammo_pistol")]
[Title("Pistol Ammo"), Category("Ammo")]
[EditorModel( "models/ammo/handgun.vmdl" )]
[HammerEntity]
public class PistolBox : BLAmmoBase
{
	public override string ModelPath => "models/ammo/handgun.vmdl";
	public override AmmoType AmmoTypeGiven => AmmoType.Pistol;
	public override int AmmoAmount => 12;
}

[Library( "bl_ammo_shotgun" )]
[Title( "Shotgun Ammo" ), Category( "Ammo" )]
[EditorModel( "models/ammo/buckshot.vmdl" )]
[HammerEntity]
public class ShotgunBox : BLAmmoBase
{
	public override string ModelPath => "models/ammo/buckshot.vmdl";
	public override AmmoType AmmoTypeGiven => AmmoType.Buckshot;
	public override int AmmoAmount => 8;
}

[Library( "bl_ammo_rifle" )]
[Title( "Rifle Ammo" ), Category( "Ammo" )]
[EditorModel( "models/ammo/rifle.vmdl" )]
[HammerEntity]
public class RifleBox : BLAmmoBase
{
	public override string ModelPath => "models/ammo/rifle.vmdl";
	public override AmmoType AmmoTypeGiven => AmmoType.Rifle;
	public override int AmmoAmount => 12;
}
