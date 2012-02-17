using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FakeItEasy.AutoMocker
{
    public class FakeSetterConvention
    {
        private List<Predicate<string>> nameMatchRules;
        private List<Predicate<Type>> typeMatchRules;

        public FakeSetterConvention()
        {
            nameMatchRules = new List<Predicate<string>>();
            typeMatchRules = new List<Predicate<Type>>();
        }

        public void NameMatches(Predicate<string> rule)
        {
            nameMatchRules.Add(rule);
        }

        public void TypeMatches(Predicate<Type> rule)
        {
            typeMatchRules.Add(rule);
        }

        internal bool Match(PropertyInfo info)
        {
            return nameMatchRules.TrueForAll(p => p(info.Name)) && typeMatchRules.TrueForAll(p => p(info.PropertyType));
        }
    }
}
