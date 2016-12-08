using FindV.MetroModel.Bean;
using System.Collections.Generic;
using System.Diagnostics;

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
        /// 最佳遍历
        /// </summary>
        /// <param name="transferTime">换乘所浪费时间，单位(站)</param>
        public List<Station> ThinkGoThrough(int startId, int transferTime)
        {
            List<Path> result = new List<Path>();
            Station start = _metro.Stations.Find((Station s) => { return s.Id == startId; });
            BlindWalk(result, start, start, new HashSet<int>(), 0, null, transferTime);


            result.Sort((Path a, Path b) => {
                if (a.Stations.Count + (a.PassedLines.Count * transferTime) > b.Stations.Count + (b.PassedLines.Count * transferTime))
                    return 1;
                else if (a.Stations.Count < b.Stations.Count)
                    return -1;
                else if (a.PassedLines.Count >= b.PassedLines.Count)
                    return 1;
                else return -1;
            });

            // foreach (Path p in result)
            // {
            //     if (p != null)
            //         Debug.WriteLine(_metro.Stations2Str(p.Stations));
            // }

            return result.Count > 0 ? result[0].Stations : null;
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

        // 寻路递归算法
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

        // 遍历问题的递归瞎走算法(暴力穷举)
        private void BlindWalk(List<Path> result, Station finalStart, Station start, HashSet<int> passedStations,
            int stationsNum, Path lastPath, int transferTime)
        {
            // if (lastPath != null)
            //     Debug.WriteLine(_metro.Stations2Str(lastPath.Stations));
            // Debug.WriteLine("stationsNum:" + stationsNum + " ,passedNum:" + passedStations.Count + " ,allNum:" + _metro.Stations.Count);
            // 结束条件
            if (passedStations.Count == _metro.Stations.Count)
            {
                Path backPath = ThinkAllPath(start.Id, finalStart.Id, transferTime)[0];
                Path temp = new Path(lastPath.Stations, lastPath.PassedLines);
                foreach (Station s in backPath.Stations)
                    temp.Stations.Add(s);
                foreach (int i in backPath.PassedLines)
                    temp.PassedLines.Add(i);
                if (temp.PassedLines != null) result.Add(temp);
                return;
            }
            if (result != null) {
                int currentShortest = int.MaxValue;
                foreach (Path p in result)
                    if (p.Stations.Count < currentShortest)
                        currentShortest = p.Stations.Count;
                if (stationsNum > currentShortest)
                    return;
            }
            if (stationsNum > _metro.Stations.Count * 10)
                return;
            // 无尽的递归
            foreach (int i in start.PassLineIds)
            {
                MetroLine line = _metro.MetroLines.Find((MetroLine l) => l.Id == i);
                if (start.Id != line.Stations[0].Id)
                    BindToNext(result, finalStart, start, passedStations, stationsNum, lastPath, line.Stations[0], i, transferTime);
                if (start.Id != line.Stations[line.Stations.Count - 1].Id)
                    BindToNext(result, finalStart, start, passedStations, stationsNum, lastPath, line.Stations[line.Stations.Count - 1], i, transferTime);
                
                foreach (Station s in line.GetTransferStations())
                {
                    if (s.Id == line.Stations[0].Id || s.Id == line.Stations[line.Stations.Count - 1].Id || s.Id == start.Id)
                        continue; // 跳过起点和终点（算过了）

                    BindToNext(result, finalStart, start, passedStations, stationsNum, lastPath, s, i, transferTime);
                }
            }
        }

        private void BindToNext(List<Path> result, Station finalStart, Station start, HashSet<int> passedStations,
            int stationsNum, Path lastPath, Station nextStation, int currentLine, int transferTime)
        {
            Path tempLastPath;
            HashSet<int> tempPassedStations = new HashSet<int>();
            int tempStationsNum = stationsNum;
            List<Station> toSPath = GetPathBetween(start, nextStation, currentLine);
            toSPath.Add(nextStation);
            if (lastPath != null)
            {
                tempLastPath = new Path(lastPath.Stations, lastPath.PassedLines);
                toSPath.Remove(start);
                foreach (Station station in toSPath)
                    tempLastPath.Stations.Add(station);
                tempLastPath.PassedLines.Add(currentLine);
            }
            else
            {
                List<int> passdLines = new List<int>();
                passdLines.Add(currentLine);
                tempLastPath = new Path(toSPath, passdLines);
            }
            foreach (Station station in toSPath)
            {
                if (passedStations != null)
                    foreach (int j in passedStations)
                        tempPassedStations.Add(j);
                tempPassedStations.Add(station.Id);
            }
            tempStationsNum += toSPath.Count;
            BlindWalk(result, finalStart, nextStation, tempPassedStations, tempStationsNum, tempLastPath, transferTime);
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
            //Debug.Write("\nGetPathBetween：start->" + startStation.Name + ":" + temp.IndexOf(startStation) + ", end->" + endStation.Name + ":" + temp.IndexOf(endStation) + "  ");
            // foreach (Station s in result)
            // {
            //     Debug.Write(s.Name + "->");
            // }
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
