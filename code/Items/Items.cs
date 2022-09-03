using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using SandboxEditor;

[Library( "bl_bloodbag" )]
[Title( "Blood Bag" ), Description( "A bag of blood" ), Category("Items")]
[EditorModel( "models/items/bloodbag.vmdl" )]
[HammerEntity]
public class Bloodbag : BLItemBase, IUse
{
	public override bool ForUndead => true;
	public override string ModelPath => "models/items/bloodbag.vmdl";

	public override void UseItem( Entity user )
	{
		var player = user as BLPawn;

		player.IncreaseBloodBar( 64.0f );
		player.Health += 25.0f;
		player.Health = player.Health.Clamp( 1, player.MaxHealth );

		base.UseItem( user );
	}
}

