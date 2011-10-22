using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FakeItEasy.AutoMocker
{
    internal class ObjectGraph
    {
        public ObjectGraph()
        {
            Dependencies = new List<ObjectGraph>();
        }

        public bool HasDefaultConstructor { get; set; }

        public Type Type { get; set; }

        public List<ObjectGraph> Dependencies { get; set; }
    }
}
