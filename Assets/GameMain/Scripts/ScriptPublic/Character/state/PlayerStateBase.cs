using System;
using GameFramework.Fsm;
using UniRx;


public abstract class PlayerStateBase : FsmState<IPlayerStateController>
{
	
}

public interface IPlayerStateController
{
	
}
