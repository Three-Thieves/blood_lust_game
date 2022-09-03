using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

public class BackpackColumn : Panel
{
	public int Column;
	public bool IsSelected;
	public Label Header;
	public int SelectedIndex;

	internal List<BackpackIcon> Icons = new();

	public BackpackColumn( int i, Panel parent )
	{
		Parent = parent;
		Column = i;
		Header = Add.Label( $"{i + 1}", "slot-number" );
	}

	internal void UpdateWeapon( BLWeaponsBase weapon )
	{
		var icon = ChildrenOfType<BackpackIcon>().FirstOrDefault( x => x.Weapon == weapon );
		if ( icon == null )
		{
			icon = new BackpackIcon( weapon );
			icon.Parent = this;
			Icons.Add( icon );
		}


	}

	internal void TickSelection( BLWeaponsBase selectedWeapon )
	{
		SetClass( "active", selectedWeapon?.Bucket == Column );

		for ( int i = 0; i < Icons.Count; i++ )
		{
			Icons[i].TickSelection( selectedWeapon );
		}

		SortChildren( p =>
		{
			if ( p is BackpackIcon icon )
			{
				return icon.Weapon?.BucketWeight ?? 0;
			}

			return 0;
		} );

	}
}
