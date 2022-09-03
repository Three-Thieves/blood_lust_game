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
	public Label PlayerDeathStement;

	public PlayerInfo()
	{
		StyleSheet.Load( "ui/styles/playerinfo.scss" );

		PlayerPnl = Add.Panel( "panel" );
		PlayerIdentity = PlayerPnl.Add.Label( "???", "playerInfo" );
		PlayerDeathStement = PlayerPnl.Add.Label( "", "stement" );
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

		TraceResult clTr;

		if ( pawn.CurTeam == BLPawn.BLTeams.Vampire )
			clTr = Trace.Ray( pawn.EyePosition, pawn.EyePosition + pawn.EyeRotation.Forward * 1000 )
			.Ignore( pawn )
			.Run();
		else
			clTr = Trace.Ray( pawn.EyePosition, pawn.EyePosition + pawn.EyeRotation.Forward * 150 )
			.Ignore( pawn )
			.Run();

		if ( clTr.Entity is BLPawn player )
		{
			string team = "";

			if ( player.CurTeam == BLPawn.BLTeams.Vampire && pawn.CurTeam == BLPawn.BLTeams.Vampire )
				team = "\nVampire";
			else if ( player.CurTeam == BLPawn.BLTeams.Hunter )
				team = "\nHunter";

			PlayerIdentity.SetText( player.PlayerIdentity );
			PlayerDeathStement.SetText( $"{team}" );
		}
		else if ( clTr.Entity is BLRagdoll corpse )
		{
			string isStaked = "";
			string team = "";

			if ( corpse.IsStaked )
				isStaked = "they appear to have been staked in the heart";
			else
				isStaked = "this body is clear of any chest stab wounds";

			if ( corpse.CorpseTeam == BLPawn.BLTeams.Vampire && corpse.IsStaked )
				team = ",\nthey were once a vampire";
			else if ( corpse.CorpseTeam == BLPawn.BLTeams.Human && corpse.IsStaked )
				team = ",\nthey were human";
			else if ( corpse.CorpseTeam == BLPawn.BLTeams.Hunter )
				team = ",\nthey served humanity well";

			PlayerIdentity.SetText( $"Here lies {corpse.CorpseName}" );
			PlayerDeathStement.SetText( $"{isStaked}{team} " );
		}
		SetClass( "playerHover", clTr.Entity is BLPawn || clTr.Entity is BLRagdoll);

	}
}

