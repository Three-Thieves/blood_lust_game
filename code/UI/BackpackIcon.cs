using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

class BackpackIcon : Panel
{
	public BLWeaponsBase Weapon;
	public Panel Icon;

	public BackpackIcon( BLWeaponsBase weapon )
	{
		Weapon = weapon;
		Icon = Add.Panel( "icon" );

		AddClass( weapon.ClassName );
	}

	internal void TickSelection( BLWeaponsBase selectedWeapon )
	{
		SetClass( "active", selectedWeapon == Weapon );
		SetClass( "empty", !Weapon?.IsUsable() ?? true );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Weapon.IsValid() || Weapon.Owner != Local.Pawn )
			Delete( true );
	}
}
