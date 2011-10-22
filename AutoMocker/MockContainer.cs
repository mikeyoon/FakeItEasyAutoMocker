using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FakeItEasy;

namespace FakeItEasy
{
    public class MockContainer<T> where T : class
    {
        internal MockContainer(T sub, Dictionary<Type, object> fakes)
        {
            Subject = sub;
            this.fakes = fakes;
        }

        Dictionary<Type, object> fakes;

        /// <summary>
        /// The concrete class that is under test
        /// </summary>
        public T Subject
        {
            get;
            private set;
        }

        /// <summary>
        /// Retrieves the faked instance of a dependency for setups
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public U GetMock<U>() where U : class
        {
            if (fakes.ContainsKey(typeof(U)))
                return fakes[typeof(U)] as U;

            return default(U);
        }
    }
}
