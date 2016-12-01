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
    }
}
