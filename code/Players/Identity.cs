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

	public List<ModelEntity> oldClothing;

	public static string[] maleNames = new string[]
	{
		"John",
		"Bill",
		"Adam",
		"David",
		"Ben",
		"Jonathan",
		"Terry",
		"Gabe",
		"Joe",
		"Michael",
		"Mike",
		"Henry",
		"Issac",
		"Ciaran"
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
		"Samantha",
		"Catherine",
		"Elisa",
		"Zoey",
		"Rochelle"
	};

	static string[] hunterNames = new string[]
	{
		"Van Helsing", "Lincoln", "Buffy"
	};

	static string[] femaleHair = new string[]
	{
		"models/citizen_clothes/hair/hair_femalebun.blonde.vmdl",
		"models/citizen_clothes/hair/hair_femalebun.brown.vmdl",
		"models/citizen_clothes/hair/hair_bun/models/hair_bun.vmdl",
		"models/citizen_clothes/hair/hair_longcurly/models/hair_longcurly.vmdl"
	};

	static string[] femaleClothingTop = new string[]
	{
		"models/citizen_clothes/dress/dress.kneelength.vmdl",
		"models/citizen_clothes/dress/posh_dress/models/posh_dress.vmdl",
	};

	static string[] maleClothingTop = new string[]
	{
		"models/citizen_clothes/jacket/jacket.red.vmdl",
		"models/citizen_clothes/jacket/hoodie/models/hoodie.vmdl",
	};

	static string[] maleClothingBottom = new string[]
	{
		"models/citizen_clothes/trousers/trousers.jeans.vmdl",
	};

	static string[] shoes = new string[]
	{
		"models/citizen_clothes/shoes/trainers/trainers.vmdl",
		"models/citizen_clothes/shoes/sneakers/models/sneakers.vmdl",
	};

	public void SetIdentity()
	{
		bool canTakeName = false;
		int attempts = 3;

		if( oldClothing.Count() > 0)
		{
			foreach ( var item in oldClothing )
			{
				item.Delete();
			}

			oldClothing.Clear();
		}

		if ( CurTeam == BLTeams.Hunter )
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

			ModelEntity hunterOutfit = new ModelEntity( "models/citizen_clothes/shirt/priest_shirt/models/priest_shirt.vmdl" );
			hunterOutfit.SetParent( this, true );
			hunterOutfit.EnableHideInFirstPerson = true;

			ModelEntity hunterbottom = new ModelEntity( "models/citizen_clothes/trousers/trousers.smart.vmdl" );
			hunterbottom.SetParent( this, true );
			hunterbottom.EnableHideInFirstPerson = true;
			
			ModelEntity hunterBoots = new ModelEntity( "models/citizen_clothes/shoes/smartshoes/smartshoes.vmdl" );
			hunterBoots.SetParent( this, true );
			hunterBoots.EnableHideInFirstPerson = true;

			oldClothing.Add( hunterOutfit );
			oldClothing.Add( hunterbottom );
			oldClothing.Add( hunterBoots );
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

			ModelEntity topOutfit = new ModelEntity( maleClothingTop[Rand.Int( 0, maleClothingTop.Length - 1 )] );
			topOutfit.SetParent( this, true );
			topOutfit.EnableHideInFirstPerson = true;

			ModelEntity bottomOutfit = new ModelEntity( maleClothingBottom[Rand.Int( 0, maleClothingBottom.Length - 1 )] );
			bottomOutfit.SetParent( this, true );
			bottomOutfit.EnableHideInFirstPerson = true;

			oldClothing.Add( topOutfit );
			oldClothing.Add( bottomOutfit );
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

			ModelEntity femHair = new ModelEntity( femaleHair[Rand.Int( 0, femaleHair.Length - 1 )] );
			femHair.SetParent( this, true );
			femHair.EnableHideInFirstPerson = true;

			ModelEntity femTop = new ModelEntity( femaleClothingTop[Rand.Int( 0, femaleClothingTop.Length - 1 )] );
			femTop.SetParent( this, true );
			femTop.EnableHideInFirstPerson = true;

			oldClothing.Add( femHair );
			oldClothing.Add( femTop );
		}

		ModelEntity boots = new ModelEntity();
		boots.SetModel( shoes[Rand.Int( 0, shoes.Length - 1 )] );
		boots.SetParent( this, true );
		boots.EnableHideInFirstPerson = true;

		oldClothing.Add( boots );
	}
}

