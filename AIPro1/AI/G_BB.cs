using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MastersOfPotsDeGeimWorld;

namespace AIpro_FSM.AI
{
    public class G_BB:Blackboard
    {
        //public TileInfo[,] Tiles;
        public AreaInfo[,] Areas;
        AI_team team;

        int update_frequency=0;

        
        int amount_of_allies, amount_of_diamonds, amount_of_food, amount_of_enemies;

        public G_BB(Map map,AI_team team,int update_frequency)
        {
            this.update_frequency = update_frequency;
            this.team = team;
            Areas=new AreaInfo[2,2];
            int w=map.W;
            int h=map.H;
            int w2=map.W/2;
            int h2=map.H/2;
            Areas[0,0] = new AreaInfo(map,0,0,0,0,w2,h2);
            Areas[1, 0] = new AreaInfo(map,1, 0, w2, 0, w, h2);

            Areas[0, 1] = new AreaInfo(map,0, 1, 0, h2, w2, h);
            Areas[1, 1] = new AreaInfo(map,1, 1, w2, h2, w, h);
        }

        /// <summary>
        /// Aggregates global data for a single update loop
        /// </summary>
        /// <param name="map"></param>
        public void Update(Map map) {
            if (map.Turn%update_frequency!=0) return;

            amount_of_enemies=amount_of_diamonds=amount_of_allies=amount_of_food=0;

            //update area infos and calculate intra heurastics
            foreach (var area in Areas) {
                area.ClearTurnInfo();
                for (int i=area.X;i<area.W;++i){
                    for (int j=area.Y;j<area.H;++j){
                        var t=map.GetTile(i,j);
                        if (t.EntityReference!=null){
                            if (t.EntityReference.MyTeam == team)
                            {
                                ++area.amount_of_allies;
                                ++amount_of_allies;
                                area.Allies.Add((AIEntity)t.EntityReference);
                            }
                            else
                            {
                                ++area.amount_of_enemies;
                                ++amount_of_enemies;
                            }
                        }
                        else if (t.TileType == Tile.Type.diamond) {
                            ++area.amount_of_diamonds;
                            ++amount_of_diamonds;
                        }
                        else if (t.TileType == Tile.Type.food)
                        {
                            ++area.amount_of_food_tiles;
                            area.amount_of_food_index += t.Amount;
                            amount_of_food+=t.Amount;
                        }
                    }
                }

                area.CalculateIntraHeurastics();
            }

            //calculate global heurastics
            //int max_spawn_amount_index=(int)((amount_of_food*Entity.EnergyGainFromEating)/Entity.CloneEnergyCost);


            //min_amount_of_spawners = Math.Min(max_spawn_amount_index,Math.Max(0,amount_of_diamonds-amount_of_allies) + Math.Max(0,amount_of_enemies-amount_of_allies));

            if (amount_of_food == 0) {//emergency mode
                foreach (var a in team.TeamMembers) {
                    var aie = (AIEntity)a;
                    aie.NOFOODLEFT=true;
                }
            }

            //calculate inter heurastics

            foreach (var area in Areas)
            {
                area.CalculateInterHeurastics(Areas);

                if (area.amount_of_allies > area.amount_of_enemies)
                {
                    //our territory
                    area.min_defenders = Math.Max(area.amount_of_enemies, Math.Min(area.amount_of_enemies*2,area.value_heurastic / 4));
                    area.troop_offset = area.amount_of_allies-area.min_defenders;
                }
                else{
                    //their territory
                    area.min_attackers =(int)(area.amount_of_enemies * 1.5f);
                    area.troop_offset = area.amount_of_allies - area.amount_of_enemies;
                }
            }

            //make decisions

            foreach (var area in Areas)
            {
                area.CalculateInterHeurastics(Areas);

                if (area.amount_of_allies == 0) continue;

                if (area.amount_of_allies > area.amount_of_enemies)
                {
                    //our territory
                    area.min_defenders = Math.Max(area.amount_of_enemies*2, area.value_heurastic);
                    area.troop_offset = area.amount_of_allies - area.min_defenders;

                    int area_min_miners = area.amount_of_diamonds;
                    

                    if (area.troop_offset > 0)//more troops than required
                    {
                            //send attackers somewhere else
                            int currently_attacking = 0;
                            var non_attack_list = new List<AIEntity>();
                            foreach (var e in area.Allies)
                            {
                                if (e.IsCommandAttacking())
                                {
                                    ++currently_attacking;
                                }
                                else
                                {
                                    non_attack_list.Add(e);
                                }
                            }

                            if (currently_attacking < area.troop_offset)
                            {
                                int amount_to_attack = Math.Min(area.troop_offset - currently_attacking, non_attack_list.Count);
                                for (int i = 0; i < amount_to_attack; ++i)
                                {
                                    non_attack_list[i].SetCommandAttack();
                                }
                            }
                    }
                    else {//less troops than required
                        //assign spawners and miners
                        int required_spawner_amount = Math.Max(1,Math.Min(area.max_spawner_amount, Math.Min(-area.troop_offset, area.Allies.Count)));

                        if (area.amount_of_food_tiles == 0) required_spawner_amount = 0;

                        var spawners=new List<AIEntity>();
                        var non_spawners=new List<AIEntity>();

                        foreach(var a in area.Allies){
                            if (a.IsCommandCloning())
                                spawners.Add(a);
                            else
                                non_spawners.Add(a);
                        }
                        int spawner_offset=spawners.Count - required_spawner_amount;
                        
                        if (spawner_offset>0)//more spawners than required
                        {
                            for (int i = 0; i < spawner_offset; ++i)
                            {
                                var a = spawners[i];
                                if (area.amount_of_diamonds > 0)
                                {
                                    a.SetCommandMine();
                                }
                                else
                                    a.SetCommandAttack();
                            }
                        }
                        else //less spawners than required
                        {
                            for (int i = 0; i < Math.Min(non_spawners.Count,-spawner_offset); ++i)
                            {
                                var a = non_spawners[i];
                                a.SetCommandClone();
                            }
                        }
                    }
                }
                else
                {
                    //their territory
                    area.min_attackers = (int)(area.amount_of_enemies * 1.5f);
                    area.troop_offset = area.amount_of_enemies- area.amount_of_allies;

                    //actions
                    int required_spawner_amount = (int)Math.Ceiling(Math.Min(area.troop_offset, area.Allies.Count)*0.4f);
                    for (int i = 0; i < area.Allies.Count; ++i)
                    {
                        var a = area.Allies[i];
                        if (i < required_spawner_amount)
                            a.SetCommandClone();
                        else
                            a.SetCommandAttack();
                    }
                }
            }
        }
    }

    public class AreaInfo {

        public int IX { get; private set; }
        public int IY { get; private set; }
        public int X{get;private set;}
        public int Y{get;private set;}
        public int W{get;private set;}
        public int H{get;private set;}

        public List<AIEntity> Allies;
        public List<Tile> Tiles;

        public AreaInfo(Map map,int ix,int iy,int x,int y,int w, int h) {
            X = x; Y = y; W = w; H = h;
            IX = ix; IY = iy;
            Allies = new List<AIEntity>();

            Tiles = new List<Tile>();
            for (int i = X; i < W; ++i)
            {
                for (int j = Y; j < H; ++j)
                {
                    var t = map.GetTile(i, j);
                    Tiles.Add(t);
                }
            }
        }

        public int troop_offset;//negative=requires troops, positive=has troop excess

        public int min_defenders = 0,min_attackers;

        public int amount_of_allies;
        public int amount_of_enemies;

        public int amount_of_diamonds, max_spawner_amount;
        public int amount_of_food_tiles,amount_of_food_index;

        public int value_heurastic, danger_heurastic_intra, danger_heurastic_inter;
        public int DangerHeurasticFull { get { return danger_heurastic_intra + danger_heurastic_inter; } }

        public void CalculateIntraHeurastics()
        {
            value_heurastic = (amount_of_diamonds + amount_of_food_tiles);
            danger_heurastic_intra = amount_of_enemies - amount_of_allies;

            max_spawner_amount = (int)Math.Ceiling(((amount_of_food_index * Entity.EnergyGainFromEating) / Entity.CloneEnergyCost)*0.2f);
        }
        public void CalculateInterHeurastics(AreaInfo[,] areas)
        {
            foreach (var s in PathFinder.surrounding) {
                int ix=IX+s.Point.X;
                int iy=IY+s.Point.Y;
                
                if (ix<0||iy<0||ix>=areas.GetLength(0)||iy>=areas.GetLength(1)) continue;
                var area = areas[ix, iy];
                danger_heurastic_inter += area.danger_heurastic_intra / 2;
            }
        }

        public void ClearTurnInfo()
        {
            Allies.Clear();

            troop_offset=0;
            min_defenders = min_attackers=0;
            amount_of_allies=amount_of_enemies=0;
            amount_of_diamonds=max_spawner_amount=amount_of_food_tiles=amount_of_food_index=0;
        }
    }

    /*
    public class TileInfo {

        public int danger_level;
    }
    */
}
