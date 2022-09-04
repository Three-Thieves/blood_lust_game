using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class DeathNotice : Panel
{
	public Panel DeathPnl;
	public Label DeathNote;

	public DeathNotice()
	{
		StyleSheet.Load( "ui/styles/deathnotice.scss" );

		DeathPnl = Add.Panel();
		DeathNote = DeathPnl.Add.Label( "???", "finalNote" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( BLGame.CurrentState != BLGame.GameStates.Active )
		{
			SetClass( "show", false );
			return;
		}

		var player = Local.Pawn as BLPawn;

		if ( player == null )
			return;

		if( player.CurTeam == BLPawn.BLTeams.Vampire )
		{
			if( player.TimeUntilResurrect > 0)
				DeathNote.SetText( $"You can resurrect in {Math.Round(player.TimeUntilResurrect)}" );
			else
				DeathNote.SetText( $"'Primary Fire' to resurrect" );

			SetClass( "show", player.Health <= 0 );
		}
	}
}

