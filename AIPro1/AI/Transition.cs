using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIpro_FSM.AI
{
    public class Transition
    {
        public List<CriterionData> Criteria { get; private set; }
        public State Target;

        public Transition(State target){
            Criteria = new List<CriterionData>();
            Target = target;        
        }

        public void AddCriterion(string vari, float val, Comparison com) {
            Criteria.Add(new CriterionData(vari, val, com));
        }

        public bool Check(Blackboard blackboard)
        {
            for (int i = 0; i < Criteria.Count; ++i) {
                var c = Criteria[i];
                var v=blackboard.GetValue(c.Name);
                if (!c.Check(v)) return false;
            }
            return true;
        }
    }
}
