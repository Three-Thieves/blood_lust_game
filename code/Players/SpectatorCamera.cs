using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public class SpectatorCamera : CameraMode
{
	Angles LookAngles;
	Vector3 MoveInput;

	Vector3 TargetPos;
	Rotation TargetRot;

	float MoveSpeed;
	float BaseMoveSpeed = 300.0f;

	/// <summary>
	/// On the camera becoming activated, snap to the current view position
	/// </summary>
	public override void Activated()
	{
		base.Activated();

		TargetPos = CurrentView.Position;
		TargetRot = CurrentView.Rotation;

		Position = TargetPos;
		Rotation = TargetRot;
		LookAngles = Rotation.Angles();
	}

	public override void Deactivated()
	{
		base.Deactivated();
	}

	public override void Update()
	{
		var player = Local.Client;
		if ( player == null ) return;

		Viewer = null;

		FreeMove();
		
	}

	public override void BuildInput( InputBuilder input )
	{
		MoveInput = input.AnalogMove;

		MoveSpeed = 1;
		if ( input.Down( InputButton.Run ) ) MoveSpeed = 5;
		if ( input.Down( InputButton.Duck ) ) MoveSpeed = 0.2f;

		if ( input.Pressed( InputButton.Walk ) )
		{
			var tr = Trace.Ray( Position, Position + Rotation.Forward * 4096 ).Run();
		}

		LookAngles += input.AnalogLook;
		LookAngles.roll = 0;
		
		BaseMoveSpeed += input.MouseWheel * 10.0f;
		BaseMoveSpeed = BaseMoveSpeed.Clamp( 10, 1000 );

		input.Clear();
		input.StopProcessing = true;
	}

	void FreeMove()
	{
		var mv = MoveInput.Normal * BaseMoveSpeed * RealTime.Delta * Rotation * MoveSpeed;

		TargetRot = Rotation.From( LookAngles );
		TargetPos += mv;

		Position = Vector3.Lerp( Position, TargetPos, 10 * RealTime.Delta );
		Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta );
	}

	void PivotMove()
	{
		TargetRot = Rotation.From( LookAngles );
		Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta );

		TargetPos = Rotation.Forward;
		Position = TargetPos;
	}
}
