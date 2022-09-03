using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

public class Vitals : Panel
{
	public Panel HealthBar;
	public Panel Bar;
	public Panel Border;
	public Panel Blood;
	public Panel BloodBorder;
	public Panel BloodBar;
	public Label Identity;
	public Vitals()
	{
		StyleSheet.Load( "UI/Styles/vitalbar.scss" );

		HealthBar = Add.Panel("HealthBar");
		Bar = HealthBar.Add.Panel( "bar" );
		Border = Add.Panel( "border" );

		Blood = Add.Panel( "bloodbar" );
		BloodBar = Blood.Add.Panel( "blood" );
		var border = Blood.Add.Panel( "border" );
		Identity = Add.Label( "???", "ident" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as BLPawn;
		if ( player == null ) return;

		if ( BLGame.CurrentState != BLGame.GameStates.Active )
			Identity.SetText( "" );
		else
			Identity.SetText( player.PlayerIdentity );

		Bar.SetClass( "human", player.BLCurTeam == BLPawn.BLTeams.Human || player.BLCurTeam == BLPawn.BLTeams.Hunter );
		Bar.SetClass( "vampire", player.BLCurTeam == BLPawn.BLTeams.Vampire );

		Border.SetClass( "human", player.BLCurTeam == BLPawn.BLTeams.Human || player.BLCurTeam == BLPawn.BLTeams.Hunter );
		Border.SetClass( "vampire", player.BLCurTeam == BLPawn.BLTeams.Vampire );

		Bar.Style.Height = Length.Percent( player.Health.CeilToInt() );

		Blood.SetClass( "isVampire", player.BLCurTeam == BLPawn.BLTeams.Vampire );
		BloodBar.Style.Height = Length.Pixels( player.BloodBar );
	}
}
