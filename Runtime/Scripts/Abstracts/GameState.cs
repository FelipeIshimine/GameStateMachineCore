using System;
using UnityEngine;
namespace GameStateMachineCore
{
    public abstract class GameState : IState, IStateMachine
    {
        private IState currentState;
        public IState CurrentState => currentState;

        public abstract void Enter();

        public virtual void Exit()
        {
            if (currentState != this)
                currentState?.Exit();
        }

        public virtual void SwitchState(IState nState)
        {
            Debug.Log($" <color=blue> {this.GetType().Name }: </color> <Color=blue> {currentState?.GetType().Name} </color> => <Color=blue> {nState.GetType().Name} </color>");
            if (currentState != this)
                currentState?.Exit();
            currentState = nState;
            currentState?.Enter();
        }

    }
}