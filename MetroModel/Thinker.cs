﻿using FindV.MetroModel.Bean;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindV.MetroModel
{
    /// <summary>
    /// 思考者，整天就想一些和地铁有关的莫名其妙的问题。
    /// </summary>
    class Thinker
    {
        private MetroModel _metro;

        public Thinker(MetroModel metro)
        {
            this._metro = metro;
        }

        public List<Station> ThinkShortestPath(int startId, int endId)
        {
            // [0] is the shortest path.
            return ThinkAllPath(startId, endId)[0].Stations;
        }

        public List<Path> ThinkAllPath(int startId, int endId)
        {
            List<Path> result = new List<Path>();
            Station startStation = _metro.Stations.Find((Station s) => { return s.Id == startId; });
            Station endStation = _metro.Stations.Find((Station s) => { return s.Id == endId; });

            List<int> startPassLines = startStation.PassLineIds;
            List<int> endPassLines = endStation.PassLineIds;

            // 循环遍历所有路线
            foreach (int i in startPassLines)
                foreach (int j in endPassLines)
                   FindPath(result, i, j, startStation, endStation, null, null);
            // 排序 站数越少越前，若相等->换乘越少越前
            result.Sort((Path a, Path b) => {
                if (a.Stations.Count > b.Stations.Count)
                    return 1;
                else if (a.Stations.Count < b.Stations.Count)
                    return -1;
                else if (a.PassedLines.Count >= b.PassedLines.Count)
                    return 1;
                else return -1;
            });

            foreach (Path ss in result)
            {
                Debug.WriteLine("\n结果：");
                foreach (Station s in ss.Stations)
                {
                    Debug.Write(s.Name + "->");
                }
            }

            return result;
        }

        private void FindPath(List<Path> result, int startLine, int endLine, Station startStation, Station endStation,
            List<Station> list, List<int> alreadyPassLines)
        {
            if (alreadyPassLines == null)
                alreadyPassLines = new List<int>();

            if (startStation.Id == endStation.Id)
                return;

            // 到同一路线上了，将结果加到list中
            if (startLine == endLine)
            {
                List<Station> temp = GetPathBetween(startStation, endStation, startLine);
                // 最后一次使用list了，直接用
                if (list == null) // 为null表示一开始就是同路，直接赋值
                    list = temp;
                else // 不为null表示之前有换线前的数据，在末尾添加
                    foreach (Station s in temp)
                        list.Add(s);
                list.Add(endStation);
                result.Add(new Path(list, alreadyPassLines));
            }

            // 还没在同一条路线上，寻找下一个换乘点
            alreadyPassLines.Add(startLine); // 当前路算已走过了，不要再回来了
            
            MetroLine line = _metro.MetroLines.Find((MetroLine l) => { return l.Id == startLine; });

            foreach (Station s in line.GetTransferStations())
                foreach (int i in s.PassLineIds)
                    if (!alreadyPassLines.Contains(i))
                    {
                        Debug.Write("\n搜路： 当前线->" + line.Name + "，去的换乘点id->" + s.Name + "，换成线路->" + i);
                        List <Station> path;
                        List<Station> temp = GetPathBetween(startStation, s, startLine);
                        // 因为每个循环都会用到之前的list，所以这里不能更改list，传变量path
                        if (list == null) // 为null表示第一次，直接赋值
                            path = temp;
                        else // 不为null表示之前有路径，添加到末尾
                        {
                            path = new List<Station>();
                            foreach (Station st in list)
                                path.Add(st);
                            foreach (Station st in temp)
                                path.Add(st);
                        }
                        List<int> tempAlreadyLines = new List<int>();
                        foreach (int j in alreadyPassLines)
                            tempAlreadyLines.Add(j);
                        FindPath(result, i, endLine, s, endStation, path, tempAlreadyLines); // 递归查找子路径
                    }
        }

        private List<Station> GetPathBetween(Station startStation, Station endStation, int startLine)
        {
            List<Station> temp = _metro.MetroLines.Find((MetroLine l) => { return l.Id == startLine; }).Stations;
            List<Station> result = new List<Station>();
            
            bool add = false;
            // 如果起始点在终点后，先倒序
            if (temp.IndexOf(temp.Find((Station s) => s.Id == startStation.Id)) > temp.IndexOf(temp.Find((Station s) => s.Id == endStation.Id)))
                temp.Reverse();
            // 剔除头尾不需要的站点
            foreach (Station s in temp)
            {
                if (s.Id == startStation.Id || s.Id == endStation.Id)
                    add = !add;
                if (add)
                    result.Add(s);
            }
            Debug.Write("\n一次选择：start->" + startStation.Name + ":" + temp.IndexOf(startStation) + ", end->" + endStation.Name + ":" + temp.IndexOf(endStation) + "  ");
            foreach (Station s in result)
            {
                Debug.Write(s.Name + "->");
            }
            return result;
        }

        public class Path
        {
            public List<Station> Stations;
            public List<int> PassedLines;

            public Path(List<Station> stations, List<int> passedLines)
            {
                this.Stations = stations;
                this.PassedLines = passedLines;
            }
        }
    }
}
