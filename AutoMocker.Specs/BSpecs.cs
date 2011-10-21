using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machine.Specifications;
using FakeItEasy;

namespace AutoMocker.Specs
{
    public class when_doing
    {
        static AutoMocker.MockContainer<ASubject> graph;

        Establish context = () =>
        {
            graph = B.AutoMock<ASubject>();
            A.CallTo(() => graph.GetMock<IDependency>().whatever()).Returns("Faked!!");
            A.CallTo(() => graph.GetMock<ADependency>().GetString()).Returns("Faked As Well!!");
        };

        Because of = () => { };

        It should_return_fake_whatever = () => {
            graph.Subject.GetSomethingElse().ShouldEqual("Faked!!");
        };

        It should_return_fake_getstring = () =>
        {
            graph.Subject.GetSomething().ShouldEqual("Faked As Well!!");
        };
    }

    public class ADependency
    {
        public virtual string GetString()
        {
            return "Stuff";
        }
    }

    public interface IDependency
    {
        string whatever();
    }

    public class ASubject
    {
        public ASubject(ADependency dep, IDependency dep2)
        {
            this.dep = dep;
            this.dep2 = dep2;
        }

        ADependency dep;
        IDependency dep2;

        public string GetSomething()
        {
            return dep.GetString();
        }

        public string GetSomethingElse()
        {
            return dep2.whatever();
        }
    }
}
