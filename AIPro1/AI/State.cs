using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIpro_FSM.AI
{
    public class State
    {
        public bool IsSelector = false;
        public int Priority;
        public List<Transition> Transitions { get; private set; }

        public State ChildState, ParentState;

        public State() {
            Transitions = new List<Transition>();
        }

        public bool UpdateTransitions(AI ai)
        {
            for (int i=0;i<Transitions.Count;++i) {
                var t= Transitions[i];
                if (t.Check(ai.BB)){
                    ai.SetState(t.Target);
                    return true;
                }
            }
            return false;
        }

        public void UpdateFull(AI ai) {
            if (IsSelector) {
                UpdateTransitions(ai);
                return;
            }
            Update(ai); 
        }

        public virtual void Start(AI ai) {}
        public virtual void Update(AI ai){}
        public virtual void End(AI ai) {}

        public void SetPriority(int priority)
        {
            Priority = priority;
        }

        public void AddTransition(params Transition[] transitions) {
            foreach (var t in transitions) Transitions.Add(t);
        }
    }
}
