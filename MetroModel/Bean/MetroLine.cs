using System.Collections.Generic;

namespace FindV.MetroModel.Bean
{
    public class MetroLine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public List<Station> Stations { get; set; }

        public MetroLine(int id, string name, string color, List<Station> stations)
        {
            this.Id = id;
            this.Name = name;
            this.Color = color;
            this.Stations = stations;
        }

        /// <summary>
        /// 得到这条线路上所有换乘点
        /// </summary>
        /// <returns>换乘点数组</returns>
        public List<Station> GetTransferStations()
        {
            List<Station> result = new List<Station>();
            if (Stations != null)
                foreach (Station s in Stations)            
                    if (s.IsTransfer)
                        result.Add(s);
            return result;
        }
    }
}
