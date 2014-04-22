using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MastersOfPotsDeGeimWorld;

namespace AIpro_FSM.AI
{
    /// <summary>
    /// Dev. hardcoded data from one AI type for testing.
    /// </summary>
    public class AI
    {
        //ai data
        public AI_BB BB { get; private set; }
        public List<State> StateStack { get; private set; }

        int current_state_priority=0;

        public AIEntity MasterEntity;
        public Map world;

        public AI(){
            BB = new AI_BB();
            StateStack = new List<State>();
        }

        public void UpdateAI() {

            //transition states
            for (int i = StateStack.Count - 1; i >= 0;--i)
            {
                var s = StateStack[i];
                if (s.Priority == current_state_priority)
                {
                    if (s.UpdateTransitions(this)) {
                        break;
                    }
                }
                else break;
            }
            //update
            for (int i = StateStack.Count - 1; i >= 0; --i)
            {
                var s = StateStack[i];
                if (s.Priority == current_state_priority)
                {
                    s.UpdateFull(this);
                    if (s.IsSelector) {
                        //call the new state updates right away
                        i = StateStack.Count;
                    }
                }
                else break;
            }
        }
        /// <summary>
        /// Haxy hax of HaxHax
        /// </summary>
        /// <param name="state"></param>
        public void SetState(State state)
        {
            bool is_superstate = state.ChildState != null;

            current_state_priority = 0;
            if (StateStack.Count == 0)
            {
                PushToStack(state);
            }   
            else
            {
                current_state_priority = StateStack.Last().Priority;
                if (state.Priority>current_state_priority)
                {
                    PushToStack(state);
                }
                else
                {
                    //remove states from stack based on priority and state type
                    for (int i=StateStack.Count-1;i>=0;--i){
                        var s=StateStack[i];
                        bool remove = false;

                        if (is_superstate) 
                            remove = s.Priority > state.Priority;
                        else 
                            remove= s.Priority >= state.Priority;

                        if (remove) StateStack.Remove(s);
                    }
                    if (!is_superstate) PushToStack(state);
                }
            }
            current_state_priority=state.Priority;
        }

        private void PushToStack(State state)
        {
            if (state.ChildState != null) StateStack.Add(state.ChildState);
            StateStack.Add(state);
            if (state.ParentState != null) StateStack.Add(state.ParentState);
        }
    }
}
