using System.Collections;
using System.Collections.Generic;

public interface IStateMachine 
{
    IState CurrentState { get; }
    void SwitchState(IState nState);
}
