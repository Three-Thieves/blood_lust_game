using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public class HealthIncrease : BLBaseSkill
{
	public override string SkillName => "Health Boost";
	public override string SkillDesc => "Increases your health";
	public override string SkillIcon => "ui/healthboost.jpg";
	public override bool StartLocked => false;
	public override BLBaseSkill RequiredSkill => null;
	public override int TierLevel => 0;

	public override void OnActivate( BLPawn player )
	{
		player.MaxHealth += 30.0f;

		base.OnActivate( player );
	}
}

