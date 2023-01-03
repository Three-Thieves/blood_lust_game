using Sandbox;
using BloodLust.Player;
using System;

namespace BloodLust.Weapons;

[Title( "Weapon" ), Icon( "track_changes" )]
public partial class BloodWeapon : AnimatedEntity
{
	public virtual string ViewModelPath => "";
	public virtual string WorldModelPath => "";
	public Model WorldModel => Model.Load( WorldModelPath );
	public AnimatedEntity EffectEntity => ViewModelEntity.IsValid() ? ViewModelEntity : this;
	public WeaponViewModel ViewModelEntity { get; protected set; }
	public BloodPawn Player => Owner as BloodPawn;

	public override void Spawn()
	{
		Model = WorldModel;

		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = false;
	}

	/// <summary>
	/// Can we holster the weapon right now? Reasons to reject this could be that we're reloading the weapon..
	/// </summary>
	/// <returns></returns>
	public bool CanHolster( BloodPawn player )
	{
		return true;
	}

	/// <summary>
	/// Called when the weapon gets holstered.
	/// </summary>
	public void OnHolster( BloodPawn player )
	{
		EnableDrawing = false;
	}

	/// <summary>
	/// Can we deploy this weapon? Reasons to reject this could be that we're performing an action.
	/// </summary>
	/// <returns></returns>
	public bool CanDeploy( BloodPawn player )
	{
		return true;
	}

	/// <summary>
	/// Called when the weapon gets deployed.
	/// </summary>
	public void OnDeploy( BloodPawn player )
	{
		SetParent( player, true );
		Owner = player;

		EnableDrawing = true;

		if ( Game.IsServer )
			CreateViewModel( To.Single( player ) );
	}

	[ClientRpc]
	public void CreateViewModel()
	{
		var vm = new WeaponViewModel( this );
		vm.Model = Model.Load( ViewModelPath );
		ViewModelEntity = vm;
	}

	public override void Simulate( IClient cl )
	{
		//SimulateComponents( cl );
	}

	protected override void OnDestroy()
	{
		ViewModelEntity?.Delete();
	}
}
