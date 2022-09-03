using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class PlayerInfo : Panel
{
	public Panel PlayerPnl;
	public Label PlayerIdentity;

	public PlayerInfo()
	{
		StyleSheet.Load( "ui/styles/playerinfo.scss" );

		PlayerPnl = Add.Panel( "panel" );
		PlayerIdentity = PlayerPnl.Add.Label( "???", "playerInfo" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( BLGame.CurrentState != BLGame.GameStates.Active )
		{
			SetClass( "playerHover", false );
			return;
		}

		var pawn = Local.Pawn as BLPawn;

		if ( pawn == null )
			return;

		if( pawn.Health <= 0 )
		{
			SetClass( "playerHover", false );
			return;
		}

		var clTr = Trace.Ray( pawn.EyePosition, pawn.EyePosition + pawn.EyeRotation.Forward * 150 )
			.Ignore( pawn )
			.Run();

		if ( clTr.Entity is BLPawn player )
		{
			string team = "";

			if ( player.BLCurTeam == BLPawn.BLTeams.Vampire && pawn.BLCurTeam == BLPawn.BLTeams.Vampire )
				team = "\nVampire";
			else if ( player.BLCurTeam == BLPawn.BLTeams.Hunter )
				team = "\nHunter";

			PlayerIdentity.SetText( player.PlayerIdentity + team );
		}
		else if ( clTr.Entity is BLRagdoll corpse )
		{
			string isStaked = "";
			string team = "";

			if ( corpse.IsStaked )
				isStaked = "\nthey appear to have been staked in the heart";
			else
				isStaked = "\nthis body is clear of any chest stab wounds";

			if ( corpse.CorpseTeam == BLPawn.BLTeams.Vampire && corpse.IsStaked )
				team = ",\nthey were once a vampire";
			else if ( corpse.CorpseTeam == BLPawn.BLTeams.Human && corpse.IsStaked )
				team = ",\nthey were human";
			else if ( corpse.CorpseTeam == BLPawn.BLTeams.Hunter )
				team = ",\nthey served humanity well";

			PlayerIdentity.SetText( $"Here lies {corpse.CorpseName}, {isStaked}{team} " );
		}
		SetClass( "playerHover", clTr.Entity is BLPawn || clTr.Entity is BLRagdoll);

	}
}

