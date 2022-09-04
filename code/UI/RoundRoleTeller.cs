using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class RoundRoleTeller : Panel
{
	string[] randAdviceHuman = new string[]
	{
		"Be careful of who you trust",
		"Trust the hunter but make sure they trust you",
		"if you know someone is a vampire, make sure you stake their heart",
		"After you killed someone, are you sure they're really dead?",
	};

	string[] randAdviceVampire = new string[]
	{
		"Don't get caught by other humans when feeding",
		"Feed from dead human corpses to regain health and obtain blood points",
		"You can resurrect when downed after some time unless you get staked",
		"Bite your victims from behind, its more effective to strike them",
		"Search for blood bags, they replenish health and gives you blood points"
	};

	string[] randAdviceHunter = new string[]
	{
		"Your stake won't break when striking hearts of dead corpses",
		"Stick with your fellow hunters",
		"Stay sharp, vampires may strike if you are a lone hunter",
	};

	public Panel TellerPnl;
	public Label RoleLbl;
	public Label NumTeams;
	public Label Advice;

	string lastAdvice;

	public RoundRoleTeller()
	{
		StyleSheet.Load( "/UI/Styles/RoundRoleTeller.scss" );

		//TellerPnl = Add.Panel();
		RoleLbl = Add.Label( "???", "roleTeller" );
		NumTeams = Add.Label( "???", "numTeller" );
		Advice = Add.Label( "???", "adviceTeller" );
	}

	public string GetRandomAdvice(BLPawn.BLTeams type)
	{
		if ( string.IsNullOrEmpty(lastAdvice) )
		{
			switch(type)
			{
				case BLPawn.BLTeams.Human:
					lastAdvice = randAdviceHuman[Rand.Int( 0, randAdviceHuman.Length - 1 )];
					break;
				case BLPawn.BLTeams.Vampire:
					lastAdvice = randAdviceVampire[Rand.Int( 0, randAdviceVampire.Length - 1 )];
					break;
				case BLPawn.BLTeams.Hunter:
					lastAdvice = randAdviceHunter[Rand.Int( 0, randAdviceHunter.Length - 1 )];
					break;
			}
		}

		return lastAdvice;
	}

	public override void Tick()
	{
		var player = Local.Pawn as BLPawn;

		if ( player == null )
			return;

		if ( BLGame.CurrentState != BLGame.GameStates.Start )
		{
			SetClass( "showMenu", false );
			lastAdvice = null;
			return;
		}

		int totalVamps = BLGame.Instance.GetTeamMembers(BLPawn.BLTeams.Vampire).Count();
		int totalHunters = BLGame.Instance.GetTeamMembers( BLPawn.BLTeams.Hunter ).Count();

		switch ( player.CurTeam )
		{
			case BLPawn.BLTeams.Human:
				RoleLbl.SetText( $"You are a Human" );
				Advice.SetText( GetRandomAdvice(BLPawn.BLTeams.Human) );
				NumTeams.SetText( $"There are known to be {totalVamps} vampires and {totalHunters} hunters" );
				break;

			case BLPawn.BLTeams.Vampire:
				RoleLbl.SetText( "You are a Vampire" );
				NumTeams.SetText( $"You and {totalVamps-1} vampires are being hunted by {totalHunters} hunters" );
				Advice.SetText( GetRandomAdvice( BLPawn.BLTeams.Vampire ) );
				break;

			case BLPawn.BLTeams.Hunter:
				RoleLbl.SetText( "You are a Hunter" );
				NumTeams.SetText( $"You and {totalHunters - 1} hunters must hunt {totalVamps} vampires" );
				Advice.SetText( GetRandomAdvice( BLPawn.BLTeams.Hunter ) );
				break;
		}

		SetClass( "showMenu", BLGame.CurrentState == BLGame.GameStates.Start );
	}
}

