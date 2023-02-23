using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace projekat.Model
{
    public class ShapeEntity
    {
        private Shape shape;

        public Shape Shape { get => shape; set => shape = value; }

        public ShapeEntity()
        {
        }

        public ShapeEntity(Shape shape)
        {
            Shape = shape;
        }
    }
}
