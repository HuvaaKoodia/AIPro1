using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MastersOfPotsDeGeimWorld;

namespace AIpro_FSM.AI
{
    public class G_BB:Blackboard
    {
        public TileInfo[,] Tiles;
        public AreaInfo[,] Areas;
        AI_team team;

        int update_frequency=0;

        int amount_of_friendlies;

        public G_BB(Map map,AI_team team,int update_frequency)
        {
            this.update_frequency = update_frequency;
            this.team = team;
            Areas=new AreaInfo[2,2];
            int w=map.W;
            int h=map.H;
            int w2=map.W/2;
            int h2=map.H/2;
            Areas[0,0] = new AreaInfo(0,0,0,0,w2,h2);
            Areas[1,0] = new AreaInfo(1,0,w2, 0, w, h2);

            Areas[0,1] = new AreaInfo(0,1,0, h2, w2, h);
            Areas[1,1] = new AreaInfo(1,1,w2, h2, w, h);
        }

        /// <summary>
        /// Aggregates global data for a single update loop
        /// </summary>
        /// <param name="map"></param>
        public void Update(Map map) {
            if (map.Turn%update_frequency!=0) return;

            amount_of_friendlies = 0;
            foreach (var u in map.GameEntities) {
                if (u.MyTeam == team) ++amount_of_friendlies;
            }

            //update area infos and calculate intra heurastics
            foreach (var area in Areas) {
                for (int i=area.X;i<area.W;++i){
                    for (int j=area.Y;j<area.H;++j){
                        var t=map.GetTile(i,j);
                        if (t.EntityReference!=null){
                            if (t.EntityReference.MyTeam == team)
                                ++area.amount_of_allies;
                            else
                                ++area.amount_of_enemies;
                        }
                        else if (t.TileType == Tile.Type.diamond) {
                            ++area.amount_of_diamonds;
                        }
                        else if (t.TileType == Tile.Type.food)
                        {
                            ++area.amount_of_food;
                        }
                    }
                }

                area.CalculateIntraHeurastics();
            }

            //calculate inter heurastics and make decisions

            foreach (var area in Areas)
            {
                area.CalculateInterHeurastics(Areas);

                //our territory
                if (area.amount_of_allies > area.amount_of_enemies)
                {
                    area.min_defenders = Math.Max(area.amount_of_enemies, area.value_heurastic / 4);
                    area.troop_offset = area.amount_of_allies-area.min_defenders;
                }
                else{
                    area.min_attackers =(int)(area.amount_of_enemies * 1.5f);
                    area.troop_offset = area.amount_of_allies - area.amount_of_enemies;
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

        public AreaInfo(int ix,int iy,int x,int y,int w, int h) {
            X = x; Y = y; W = w; H = h;
            IX = ix; IY = iy;
        }

        public int troop_offset;//negative=requires troops, positive=has troop excess

        public int min_defenders = 0,min_attackers;

        public int amount_of_allies;
        public int amount_of_enemies;

        public int amount_of_diamonds;
        public int amount_of_food;

        public int value_heurastic, danger_heurastic_intra, danger_heurastic_inter;
        public int DangerHeurasticFull { get { return danger_heurastic_intra + danger_heurastic_inter; } }

        public void CalculateIntraHeurastics()
        {
            value_heurastic = (amount_of_diamonds + amount_of_food);
            danger_heurastic_intra = amount_of_enemies - amount_of_allies;
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
    }

    public class TileInfo {

        public int danger_level;
    }
}
