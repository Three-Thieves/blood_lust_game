using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public abstract partial class BLBaseSkill : EntityComponent<BLPawn>
{
	public virtual string SkillName => "Base skill";
	public virtual string SkillDesc => "Base description";
	public virtual string SkillIcon => "";
	public virtual bool StartLocked => true;
	public virtual BLBaseSkill RequiredSkill => null;
	public virtual int TierLevel => 0;

	public virtual void Simulate(Client cl)
	{
		
	}

	public virtual void OnActivate(BLPawn player)
	{
		player.BloodSkillPoints--;

		player.Skills.Add( this );
	}

	public void SkillClick( BLPawn player )
	{
		if ( !CanObtain( player ) )
			return;

		OnActivate( player );
	}

	public bool CanObtain( BLPawn player )
	{
		if ( player.BloodSkillPoints <= 0 )
			return false;

		if ( RequiredSkill != null && !player.Skills.Contains( RequiredSkill ) )
			return false;

		if ( player.Skills.Contains( this ) )
			return false;

		return true;

	}
}

