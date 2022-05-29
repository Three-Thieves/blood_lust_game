using System;
using System.Linq;
using Sandbox;
partial class BLInventory : BaseInventory
{
	public BLInventory( BLPawn player ) : base( player )
	{

	}

	public override bool Add( Entity ent, bool makeActive = false )
	{
		var player = Owner as BLPawn;
		var weapon = ent as BLWeaponsBase;
		var notices = !player.SupressPickupNotices;

		if ( weapon == null )
			return false;

		if ( weapon != null && IsCarryingType( ent.GetType() ) )
		{
			/*var ammo = weapon.AmmoClip;
			var ammoType = weapon.AmmoType;

			if ( ammo > 0 )
			{
				var taken = player.GiveAmmo( ammoType, ammo );
				if ( taken == 0 )
					return false;

				if ( notices && taken > 0 )
				{
					Sound.FromWorld( "dm.pickup_ammo", ent.Position );
					PickupFeed.OnPickup( To.Single( player ), $"+{taken} {ammoType}" );
				}
			}

			// Despawn it
			ent.Delete();*/
			return false;
		}

		if ( !base.Add( ent, makeActive ) )
			return false;

		if ( weapon != null && notices )
		{
			PickupFeed.OnPickupWeapon( To.Single( player ), ent.ClassName );
		}

		return true;
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x.IsValid() && x.GetType() == t );
	}
}
