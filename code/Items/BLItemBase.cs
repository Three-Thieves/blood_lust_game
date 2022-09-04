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
		if ( BLGame.CurrentState != BLGame.GameStates.Active )
			return false;

		var player = user as BLPawn;

		if ( player.CurTeam == BLPawn.BLTeams.Vampire && RepelUndead )
			return false;

		if ( (player.CurTeam == BLPawn.BLTeams.Human || player.CurTeam == BLPawn.BLTeams.Hunter) && ForUndead )
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

