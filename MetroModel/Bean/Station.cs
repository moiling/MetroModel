using System.Collections.Generic;

namespace FindV.MetroModel.Bean
{
    public class Station
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public List<int> PassLineIds { get; set; }
        // Auto judge. Shouldn't be set!
        public bool IsTransfer { get { return PassLineIds.Count > 1; } }

        public Station(int id, string name, double x, double y)
        {
            this.Id = id;
            this.Name = name;
            this.X = x;
            this.Y = y;
            this.PassLineIds = new List<int>();
        }
    }
}
