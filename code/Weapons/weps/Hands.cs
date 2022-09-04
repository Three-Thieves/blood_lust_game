using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

[Library("bl_hands")]
[HideInEditor]
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
	public override int BucketWeight => 0;
	public override bool IsDroppable => false;

	public override void Spawn()
	{
		
	}

	public override void AttackPrimary()
	{

	}
	public override void AttackSecondary()
	{

	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 0 );
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
		anim.SetAnimParameter( "holdtype_handedness", 0 );
	}
}

