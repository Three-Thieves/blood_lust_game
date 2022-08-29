using System;
using System.Linq;
using Sandbox;
public partial class BLInventory : BaseInventory
{
	public BLInventory( BLPawn player ) : base( player )
	{

	}

	public bool StripWeapon(Entity ent)
	{
		var player = Owner as BLPawn;
		var weapon = ent as BLWeaponsBase;

		if ( weapon == null )
			return false;

		foreach ( var item in List.ToArray() )
		{
			if( item == weapon )
			{
				List.Remove( item );
			}
		}

		return true;
	}

	public override bool Add( Entity ent, bool makeActive = false )
	{
		var player = Owner as BLPawn;
		var weapon = ent as BLWeaponsBase;

		if ( weapon == null )
			return false;

		if ( weapon != null && IsCarryingType( ent.GetType() ) )
			return false;

		if ( !base.Add( ent, makeActive ) )
			return false;

		return true;
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x.IsValid() && x.GetType() == t );
	}
}
