using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Component;

public partial class BLPawn
{
	[Net]
	public float BloodBar { get; set; }

	[Net]
	public float MaxBlood { get; set; } = 256.0f;

	[Net]
	public int BloodSkillPoints { get; set; }

	[Net]
	public int BloodLevel { get; set; }

	[Net]
	public List<BLBaseSkill> Skills { get; set; } = new List<BLBaseSkill>();

	public void SetUpVampire()
	{
		MaxHealth = 100.0f;

		BloodBar = 128.0f;
		BloodSkillPoints = 0;
		BloodLevel = 1;

		Backpack.Add( new Fangs() );
		Skills.Clear();
	}

	public void SimulateVampire()
	{

	}

	public void IncreaseBloodBar(float addBlood)
	{
		BloodBar += addBlood;

		if ( BloodBar >= MaxBlood )
			BloodLevelUp();
	}

	public void BloodLevelUp()
	{
		BloodSkillPoints++;
		BloodLevel++;
		BloodBar = 0.0f;
	}

	public void Staked()
	{
		if ( (Corpse as BLRagdoll).IsStaked )
			return;

		var stakedBody = Corpse as BLRagdoll;

		CameraMode = new SpectatorCamera();

		stakedBody.IsStaked = true;
		CurTeam = BLTeams.Spectator;

		stakedBody.CorpseOwner.LifeState = LifeState.Dead;

		BLGame.GameCurrent.CheckRoundStatus();

		if (Identity == IdentityEnum.Male)
			stakedBody.PlaySound( "vampire_deathscream" );
		else if (Identity == IdentityEnum.Female)
			stakedBody.PlaySound( "vampire_deathscream_female" );
	}
}

