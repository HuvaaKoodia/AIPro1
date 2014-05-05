using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MastersOfPotsDeGeimWorld;

namespace AIpro_FSM.AI
{
    public class AI_team:Team
    {
        public G_BB Commander;

        Map map;

        public AI_team(string name,ConsoleColor color,Map map):base(name,1,color){
            Commander = new G_BB(map,this,1);
            this.map = map;
        }

        public AIEntity AddUnit(int x,int y) {
            var unit = new AIEntity(map, this);
            unit.SetPosition(x, y);
            return unit;
        }
    }
}
