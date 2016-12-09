using FindV.MetroModel.Bean;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;

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

        /// <summary>
        /// 寻找两点之间最短路径
        /// </summary>
        /// <param name="transferTime">换乘所浪费时间，单位(站)</param>
        public List<Station> ThinkShortestPath(int startId, int endId, int transferTime)
        {
            // [0] is the shortest path.
            return ThinkAllPath(startId, endId, transferTime)[0].Stations;
        }

        /// <summary>
        /// 寻找两点之间所有路径，结果已按时间顺序排序
        /// </summary>
        /// <param name="transferTime">换乘所浪费时间，单位(站)</param>
        /// <returns><seealso cref = "Path" />包含所有经过站点和经过所有线路</returns>
        public List<Path> ThinkAllPath(int startId, int endId, int transferTime)
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
            // 排序 站数 + 换乘抵消数 越少越前，若相等->换乘越少越前
            result.Sort((Path a, Path b) => {
                if (a.Stations.Count + (a.PassedLines.Count * transferTime) > b.Stations.Count + (b.PassedLines.Count * transferTime))
                    return 1;
                else if (a.Stations.Count < b.Stations.Count)
                    return -1;
                else if (a.PassedLines.Count >= b.PassedLines.Count)
                    return 1;
                else return -1;
            });

            return result;
        }

        // 两点之间寻路递归算法
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
                        List<Station> path;
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

        /// <summary>
        /// 最佳遍历
        /// </summary>
        /// <param name="transferTime">换乘所浪费时间，单位(站)</param>
        public List<Station> ThinkGoThrough(int startId, int transferTime)
        {
            List<Station> result = new List<Station>(); // 结果
            List<MetroLine> transferedLines = new List<MetroLine>(); // 经过的所有地铁线，换了多少次，用来算换线浪费的时间
            HashSet<Station> passedStationsSet = new HashSet<Station>(); // 所有经过的站点
            HashSet<Station> totalStationsSet = new HashSet<Station>(_metro.Stations); // 所有站点
            List<int> passedTime = new List<int>(); // 每站经过的次数，位置对应站的id
            // 特殊点
            Station start = _metro.Stations.Find((Station s) => { return s.Id == startId; }); // 起点
            result.Add(start);
            Station current = start;
            HashSet<Station> endpoints = new HashSet<Station>(); // 端点
            HashSet<Station> transfers = new HashSet<Station>(); // 换乘点
            HashSet<Station> specialStations = new HashSet<Station>(); // 所有特殊点

            for (int i = 0; i < _metro.Stations.Count; i++) // 初始化每站都为0
                passedTime.Add(0);

            foreach (MetroLine l in _metro.MetroLines)
            {
                endpoints.Add(l.Stations[0]);
                endpoints.Add(l.Stations[l.Stations.Count - 1]);
                foreach (Station s in l.GetTransferStations())
                    transfers.Add(s);
            }
            specialStations.Add(start);
            specialStations.UnionWith(endpoints);
            specialStations.UnionWith(transfers);
            // 开始计算
            do
            {
                // 考虑情况：
                // 特殊考虑点：起点、端点、换乘点，以下有起点的情况为起点不为端点或换乘点，否则作为断点或换乘点计算
                // 1. 当前(换乘点)的下一特殊点是否能到剩余点所在线路？能 => 剔除不能的下一特殊点。不能 => 遍历剩余点，寻找到当前点最近的点，走
                // 2. 若当前点与(没去过的)端点(非换乘点)相邻，走
                // 3. 下一特殊点(除起点)：(PassedTime = 0) > (PassedTime != 0 但路中有0) > PassedTime > 最近(换线 * 浪费时间 + 长度) 
                // 终止条件：
                // 1. 经过站点数 > 总站数 * 5  => 寻找失败 null
                // 2. 经过站点数(去重) = 总站数 => 正确结果

                passedStationsSet = new HashSet<Station>(result);
                // 终止
                if (passedStationsSet.Count >= totalStationsSet.Count)
                    return AddTail(result, start, current, transferTime);
                if (result.Count >= totalStationsSet.Count * 5)
                    return null;

                HashSet<Station> nexts = GetNexts(current, start);

                HashSet<Station> unPassedStationsSet = new HashSet<Station>(totalStationsSet);
                unPassedStationsSet.ExceptWith(passedStationsSet);
                // 当前(换乘点)的下一特殊点是否能到剩余点所在线路
                HashSet<int> unPassedLineIds = new HashSet<int>();
                foreach (Station s in unPassedStationsSet)
                    foreach (int i in s.PassLineIds)
                        unPassedLineIds.Add(i);
                // 剔除不能直接到达剩余路线的下一特殊点
                foreach (Station s in nexts)
                    foreach (int i in s.PassLineIds)
                        if (!unPassedLineIds.Contains(i))
                            nexts.Remove(s);
                // 没有能直接到剩余路线的 => 遍历剩余(特殊点)，寻找到当前点最近的点，走
                if (nexts.Count == 0)
                {
                    int minPassNum = int.MaxValue;
                    List<Station> minStationList = new List<Station>();
                    foreach (Station s in unPassedStationsSet)
                    {
                        if (!specialStations.Contains(s))
                            continue;
                        List<Station> temp = ThinkShortestPath(current.Id, s.Id, transferTime);
                        if (temp.Count < minPassNum)
                        {
                            minPassNum = temp.Count;
                            minStationList = temp;
                        }
                    }
                    if (minStationList.Count == 0) // 尴尬，所有特殊点都走过了，但是还没走完
                    {
                        foreach (Station s in unPassedStationsSet)
                        {
                            List<Station> temp = ThinkShortestPath(current.Id, s.Id, transferTime);
                            if (temp.Count < minPassNum)
                            {
                                minPassNum = temp.Count;
                                minStationList = temp;
                            }
                        }
                    }
                    minStationList.RemoveAt(0);
                    result.AddRange(minStationList);
                    foreach (Station s in minStationList)
                        passedTime[s.Id]++;
                    current = minStationList[minStationList.Count - 1];
                    continue;
                }
                // 若当前点与(没去过的)端点(非换乘点)相邻，走
                bool shouldContinue = false;
                foreach (Station s in nexts)
                {
                    if (endpoints.Contains(s) && passedTime[s.Id] == 0
                        && (!transfers.Contains(s) || (transfers.Contains(s) && s.PassLineIds.Count == 2 && GetNexts(s, start).Count == 2)))
                    {
                        HashSet<int> tempSameLine = new HashSet<int>(current.PassLineIds);
                        tempSameLine.IntersectWith(s.PassLineIds);
                        List<Station> temp = GetPathBetween(current, s, tempSameLine.ToList()[0]); // 这里没考虑到换线
                        temp.Add(s);
                        temp.RemoveAt(0);
                        result.AddRange(temp);
                        foreach (Station station in temp)
                            passedTime[station.Id]++;
                        current = s;
                        shouldContinue = true;
                        break;
                    }
                }
                if (shouldContinue)
                    continue;
                // 下一特殊点(除起点)：(PassedTime = 0) > (PassedTime != 0 但路中有0) > PassedTime > 最近(换线 * 浪费时间 + 长度)
                List<Station> tempNexts = new List<Station>(nexts);
                List<Station> sortNexts = new List<Station>(nexts);
                // 这些点不要去！
                foreach (Station s in tempNexts)
                {
                    if (endpoints.Contains(s) && passedTime[s.Id] != 0 
                        && (!transfers.Contains(s) || (transfers.Contains(s) && s.PassLineIds.Count == 2 && GetNexts(s, start).Count == 2)))
                        sortNexts.Remove(s);
                }
                sortNexts.Remove(start);
                sortNexts.Sort((a, b) => {
                    if (passedTime[a.Id] == 0 && passedTime[b.Id] != 0)
                        return -1;
                    if (passedTime[a.Id] != 0 && passedTime[b.Id] == 0)
                        return  1;

                    HashSet<int> aSameLine = new HashSet<int>(current.PassLineIds);
                    HashSet<int> bSameLine = new HashSet<int>(current.PassLineIds);
                    HashSet<int> aLine = new HashSet<int>(a.PassLineIds);
                    HashSet<int> bLine = new HashSet<int>(b.PassLineIds);
                    aSameLine.IntersectWith(aLine);
                    bSameLine.IntersectWith(bLine);
                    List<Station> aStaions = GetPathBetween(current, a, aSameLine.ToList()[0]);
                    List<Station> bStaions = GetPathBetween(current, b, bSameLine.ToList()[0]);

                    if (passedTime[a.Id] == 0 && passedTime[b.Id] == 0)
                        return aStaions.Count > bStaions.Count ? 1 : -1;

                    if (passedTime[a.Id] != 0 && passedTime[b.Id] != 0)
                    {
                        bool aHasUnPassed = false, bHasUnPassed = false;

                        foreach (Station s in aStaions)
                            if (passedTime[s.Id] == 0)
                            {
                                aHasUnPassed = true;
                                break;
                            }

                        foreach (Station s in bStaions)
                            if (passedTime[s.Id] == 0)
                            {
                                bHasUnPassed = true;
                                break;
                            }

                        if (aHasUnPassed && bHasUnPassed)
                            return aStaions.Count > bStaions.Count ? 1 : -1;
                        if (aHasUnPassed && !bHasUnPassed)
                            return -1;
                        if (!aHasUnPassed && bHasUnPassed)
                            return 1;
                        if (!aHasUnPassed && !bHasUnPassed)
                        {
                            if (passedTime[a.Id] > passedTime[b.Id])
                                return 1;
                            if (passedTime[a.Id] < passedTime[b.Id])
                                return -1;
                            return aStaions.Count > bStaions.Count ? 1 : -1;
                        }
                    }
                    return 0;
                });
                HashSet<int> sameLine = new HashSet<int>(current.PassLineIds);
                if (sortNexts.Count == 0)
                    sortNexts = tempNexts;
                sameLine.IntersectWith(sortNexts[0].PassLineIds);
                List<Station> tempList = GetPathBetween(current, sortNexts[0], sameLine.ToList()[0]); // 这里没考虑到换线
                tempList.RemoveAt(0);
                tempList.Add(sortNexts[0]);
                result.AddRange(tempList);
                foreach (Station station in tempList)
                    passedTime[station.Id]++;
                current = sortNexts[0];

            } while (true);

        }

        private List<Station> AddTail(List<Station> result, Station start, Station current, int transferTime)
        {
            if (current.Id == start.Id)
            {
                Debug.WriteLine("tail: " + _metro.Stations2Str(result));
                return result;
            }
            else
            {
                result.AddRange(ThinkShortestPath(current.Id, start.Id, transferTime));
                Debug.WriteLine("tail: " + _metro.Stations2Str(result));
                return result;
            }
        }

        /// <summary>
        /// 获取当前站点的下一个可直达特殊点
        /// </summary>
        private HashSet<Station> GetNexts(Station current, Station start)
        {
            HashSet<Station> nexts = new HashSet<Station>();
            foreach (int i in current.PassLineIds)
            {
                MetroLine line = _metro.MetroLines.Find((MetroLine l) => l.Id == i);
                int currentPosition = line.Stations.IndexOf(current);
                HashSet<int> specialPositionsSet = new HashSet<int>();

                specialPositionsSet.Add(0);
                specialPositionsSet.Add(line.Stations.Count - 1);
                if (line.Stations.Contains(start))
                    specialPositionsSet.Add(line.Stations.IndexOf(start));
                foreach (Station s in line.GetTransferStations())
                    specialPositionsSet.Add(line.Stations.IndexOf(s));
                specialPositionsSet.Add(line.Stations.IndexOf(current));
                // 排序
                List<int> specialPositions = new List<int>(specialPositionsSet);
                specialPositions.Sort((a, b) => a > b ? 1 : -1);
                // 寻找下一个可达特殊点
                for (int j = 0; j < specialPositions.Count; j++)
                {
                    if (j == 0)
                    {
                        if (currentPosition == specialPositions[j] && j + 1 <= specialPositions.Count - 1)
                            nexts.Add(line.Stations[specialPositions[j + 1]]);
                    }
                    else if (j == specialPositions.Count - 1)
                    {
                        if (currentPosition == specialPositions[j] && j - 1 >= 0)
                            nexts.Add(line.Stations[specialPositions[j - 1]]);
                    }
                    else if (currentPosition > specialPositions[j - 1])
                    {
                        if (currentPosition == specialPositions[j])
                        {
                            nexts.Add(line.Stations[specialPositions[j - 1]]);
                            nexts.Add(line.Stations[specialPositions[j + 1]]);
                        }
                        else if (currentPosition < specialPositions[j])
                        {
                            nexts.Add(line.Stations[specialPositions[j - 1]]);
                            nexts.Add(line.Stations[specialPositions[j]]);
                        }
                    }
                }
            }
            return nexts;
        }

        /// <summary>
        /// 得到两站中途经过的所有站！注意，终点不包括在数组内！
        /// </summary>
        /// <param name="startLine">开始点所在的线路，由于开始点可能是换乘点，所以要特别指定</param>
        /// <returns>结果已按起点到终点顺序排序，！注意，终点不包括在数组内！</returns>
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

            return result;
        }

        public class Path
        {
            public List<Station> Stations = new List<Station>();
            public List<int> PassedLines = new List<int>();

            public Path(List<Station> stations, List<int> passedLines)
            {
                if (stations != null)
                    foreach (Station s in stations)
                        this.Stations.Add(s);
                if (passedLines != null)
                    foreach (int i in passedLines)
                        this.PassedLines.Add(i);
            }
        }
    }
}
