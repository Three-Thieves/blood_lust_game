using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public class BLAmmoBase : ModelEntity, IUse
{
	public virtual string ModelPath => "";
	public virtual AmmoType AmmoTypeGiven => AmmoType.None;
	public virtual int AmmoAmount => 1;

	int ammoRemaining;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( ModelPath );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		ammoRemaining = AmmoAmount;
	}

	public bool IsUsable( Entity user )
	{
		return BLGame.CurrentState == BLGame.GameStates.Active;
	}

	public bool OnUse( Entity user )
	{
		if ( !IsUsable( user ) )
			return false;

		if(user is BLPawn player)
			ammoRemaining -= player.GiveAmmo( AmmoTypeGiven, ammoRemaining );

		if ( ammoRemaining <= 0 )
			Delete();

		return false;
	}
}
