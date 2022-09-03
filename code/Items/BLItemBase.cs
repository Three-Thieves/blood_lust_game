using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using SandboxEditor;

public partial class BLItemBase : ModelEntity, IUse
{
	public virtual bool RepelUndead => false;
	public virtual bool ForUndead => false;
	public virtual string ModelPath => "";

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "blitem" );

		SetModel( ModelPath );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	}

	public virtual void UseItem( Entity user )
	{
		//Do stuff
		Delete();
	}

	public bool IsUsable( Entity user )
	{
		var player = user as BLPawn;

		if ( player.BLCurTeam == BLPawn.BLTeams.Vampire && RepelUndead )
			return false;

		if ( (player.BLCurTeam == BLPawn.BLTeams.Human || player.BLCurTeam == BLPawn.BLTeams.Hunter) && ForUndead )
			return false;

		return true;
	}
	public bool OnUse( Entity user )
	{
		if ( !IsUsable( user ) ) return false;

		UseItem(user);

		return false;
	}
}

