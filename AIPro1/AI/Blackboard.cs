using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIpro_FSM.AI
{
    public class Blackboard
    {
        public Dictionary<string, float> Values { get; private set;}

        float _value;//Dev. micro opt

        public Blackboard() {
            Values = new Dictionary<string, float>();
        }

        public void AddValue(string variable,float value){
            if (Values.ContainsKey(variable)) {
                Console.WriteLine("Variable already in blackboard: " + variable);
                return;
            }
            Values.Add(variable, value);
        }

        public void AddOrSetValue(string variable, float value)
        {
            if (Values.ContainsKey(variable))
            {
                Values[variable] = value;
                return;
            }
            else {
                Values.Add(variable, value);
            }
        }

        public void AddOrSetValue(string variable, bool value)
        {
            AddOrSetValue(variable, ParseBool(value));
        }

        public void SetValue(string variable, float value) {
            if (Values.ContainsKey(variable))
            {
                Values[variable] = value;
                return;
            }
            Console.WriteLine("Variable not found in blackboard: " + variable);
        }

        public void SetValue(string variable, bool value)
        {
            SetValue(variable, ParseBool(value));
        }

        private int ParseBool(bool value) {
            return value ? 1 : 0;
        }

        public float GetValue(string variable)
        {
            if (Values.TryGetValue(variable, out _value)) {
                return _value;
            }
            Console.WriteLine("Variable "+variable+" not found.");
            return 0;
        }
    }
}
