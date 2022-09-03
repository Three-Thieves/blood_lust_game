using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLPawn
{
	[Net]
	public string PlayerIdentity { get; private set; } = "";

	public enum IdentityEnum
	{
		Male,
		Female,
		Hunter
	}

	[Net]
	public IdentityEnum Identity { get; private set; }


	public static string[] maleNames = new string[]
	{
		"John",
		"Bill",
		"Adam",
		"David",
		"Ben",
		"Jonathan",
		"Terry",
		"Gabe"
	};

	public static string[] femaleNames = new string[]
	{
		"Kate",
		"Katie", 
		"Jenny", 
		"Jennifer", 
		"Lisa", 
		"Mina",
		"Susan", 
		"Samantha"
	};

	public static string[] hunterNames = new string[]
	{
		"Van Helsing", "Lincoln", "Buffy"
	};

	public void SetIdentity()
	{
		bool canTakeName = false;
		int attempts = 3;

		if( BLCurTeam == BLTeams.Hunter )
		{
			Identity = IdentityEnum.Hunter;
			while ( !canTakeName )
			{
				string name = hunterNames[Rand.Int( 0, hunterNames.Length - 1 )];
				attempts--;

				if ( !BLGame.Instance.takenHunterNames.Contains( name ) )
				{
					BLGame.Instance.takenHunterNames.Add( name );
					PlayerIdentity = name;
					canTakeName = true;
				}

				if ( attempts <= 0 )
				{
					PlayerIdentity = name;
					break;
				}
			}

			return;
		}
		
		int randInt = Rand.Int( 1, 2 );

		if(randInt == 1)
		{
			Identity = IdentityEnum.Male;

			while( !canTakeName )
			{
				string name = maleNames[Rand.Int( 0, maleNames.Length - 1 )];
				attempts--;

				if ( !BLGame.Instance.takenMaleNames.Contains( name ) )
				{
					BLGame.Instance.takenMaleNames.Add( name );
					PlayerIdentity = name;
					canTakeName = true;
				}

				if ( attempts <= 0 )
				{
					PlayerIdentity = name;
					break;
				}
			}
		} 
		else if (randInt == 2)
		{
			Identity = IdentityEnum.Female;
			while ( !canTakeName )
			{
				string name = femaleNames[Rand.Int( 0, femaleNames.Length - 1 )];
				attempts--;

				if ( !BLGame.Instance.takenFemaleNames.Contains( name ) )
				{
					BLGame.Instance.takenFemaleNames.Add( name );
					PlayerIdentity = name;
					canTakeName = true;
				}

				if ( attempts <= 0 )
				{
					PlayerIdentity = name;
					break;
				}
			}
		}

	}
}

