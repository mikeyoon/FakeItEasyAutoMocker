using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoMocker
{
    internal class ObjectGraph
    {
        public ObjectGraph()
        {
            Dependencies = new List<ObjectGraph>();
        }

        public Type Type { get; set; }

        public List<ObjectGraph> Dependencies { get; set; }
    }
}
