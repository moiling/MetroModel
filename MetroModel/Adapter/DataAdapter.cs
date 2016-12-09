using FindV.MetroModel.Bean;
using FindV.MetroModel.Bean.Json;
using System.Collections.Generic;

namespace FindV.MetroModel.Adapter
{
    public class DataAdapter
    {
        public V v;

        public DataAdapter(V v) { this.v = v; }

        public List<Station> GetAllStations()
        {
            if (v == null) return null;

            List<Station> stations = new List<Station>();

            foreach (Point p in v.point)
                stations.Add(new Station(p.id, p.name, p.x, p.y));

            // Station Id small to large order -> I want 'stations[id]'. Yeah!
            stations.Sort((Station a, Station b) => { return a.Id >= b.Id ? 1 : -1; });

            // Stations was been compared! Complete stations infos.
            foreach (Line l in v.line)
                foreach (int i in l.path)
                    stations[i].PassLineIds.Add(l.id);

            return stations;
        }

        public List<MetroLine> GetMetroLines()
        {
            if (v == null) return null;

            List<MetroLine> metroLines = new List<MetroLine>();
            List<Station> stations = GetAllStations();

            // Add completed stations to lines.
            foreach (Line l in v.line)
            {
                List<Station> lineStations = new List<Station>();
                foreach (int i in l.path)
                {
                    lineStations.Add(stations[i]);
                }
                metroLines.Add(new MetroLine(l.id, l.name, l.color, lineStations));
            }

            return metroLines;
        }
    }
}
