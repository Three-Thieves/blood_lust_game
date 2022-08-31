using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class Hands : BLWeaponsBase
{
	public static Model WorldModel = null;
	public override string ViewModelPath => "";
	public override float PrimaryRate => 0.0f;
	public override float SecondaryRate => 0.0f;
	public override float ReloadTime => 0.0f;
	public override AmmoType AmmoType => AmmoType.None;
	public override int ClipSize => 0;
	public override int Bucket => 0;
}

