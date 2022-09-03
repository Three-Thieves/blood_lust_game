using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLPawn
{
	AnimatedEntity hat;

	public void SetUpHunter()
	{
		var stake = new HunterStake();
		Backpack.Add( stake );

		hat = new AnimatedEntity("models/citizen_clothes/hat/hat.tophat.vmdl");
		hat.SetParent( this, true );
		hat.EnableHideInFirstPerson = true;
	}

	public void SimulateHunter()
	{

	}
}
