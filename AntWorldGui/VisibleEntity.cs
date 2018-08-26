using System.Collections.Generic;



namespace AntWorldGui
{




    public class VisibleEntityCollection
    {
        public VisibleEntityCollection()
        {
            DataPoints = new List<VisibleEntity>();
        }

        public List<VisibleEntity> DataPoints { set; get; }
    }


    public enum EntityType
    {
        Ant,
        TrailPoint,
        Food,
        Nest,
        Obstacle
    }


    public class VisibleEntity
    {
        public double VariableX { get; set; }
        public double VariableY { get; set; }
        public double Radius { get; set; }
        public EntityType Type { get; set; }
    }
}
