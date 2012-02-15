using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FakeItEasy;
using System.Reflection;
using FakeItEasy.AutoMocker;
using System.Linq.Expressions;

namespace FakeItEasy
{
    public static class B
    {
        /// <summary>
        /// Creates a concrete instance of the given class with its entire dependency tree faked. If 
        /// recursive is false, it will rely on FakeItEasy to recursively automock all dependencies, which
        /// can fail easily if any dependency has no public constructor or a "bad" constructor.
        /// </summary>
        /// <remarks>
        /// Uses the greediest constructors
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static MockContainer<T> AutoMock<T>(bool recursive = true) where T : class
        {
            ObjectGraph graph = BuildGraph(typeof(T));

            var fakes = new Dictionary<Type, object>();
            var subject = CreateSubject<T>(graph, fakes, recursive);

            return new MockContainer<T>(subject, fakes);
        }

        static ObjectGraph BuildGraph(Type type)
        {
            var constructors = type.GetConstructors();
            var constructor = constructors.OrderByDescending(p => p.GetParameters().Length).FirstOrDefault();

            ObjectGraph node = new ObjectGraph();
            node.Type = type;
            node.HasDefaultConstructor = true;

            if (constructor != null)
            {
                //Fill in dependencies in constructor
                var parameters = constructor.GetParameters();
                foreach (var param in parameters)
                {
                    //Playing it safe, recursively mock dependencies
                    if (param.GetType().IsClass || param.GetType().IsInterface)
                    {
                        node.Dependencies.Add(BuildGraph(param.ParameterType));
                    }
                }
            }
            else if (type.IsClass)
            {
                node.HasDefaultConstructor = false;
            }

            return node;
        }

        static T CreateSubject<T>(ObjectGraph graph, Dictionary<Type, object> fakes, bool recursive) where T : class
        {
            List<object> parameters = new List<object>();

            if (graph.Dependencies.Any())
            {
                foreach (var dep in graph.Dependencies)
                {
                    if (recursive)
                        parameters.Add(CreateDependency(dep, fakes));
                    else
                    {
                        var types = dep.Dependencies.Select(p => p.Type).ToList();
                        var fake = CreateFake(dep.Type, types);
                        fakes[dep.Type] = fake;
                        parameters.Add(fake);
                    }
                }
            }

            return Activator.CreateInstance(typeof(T), parameters.ToArray()) as T;
            //TODO: After constructing, check for Properties that are null and fill those in if they have virtual stuff
        }

        static object CreateDependency(ObjectGraph graph, Dictionary<Type, object> fakes)
        {
            List<object> parameters = new List<object>();

            if (!graph.HasDefaultConstructor)
                return null;

            if (fakes.ContainsKey(graph.Type))
                return fakes[graph.Type];

            if (graph.Dependencies.Any())
            {
                foreach (var dep in graph.Dependencies)
                {
                    parameters.Add(CreateDependency(dep, fakes));
                }

                var fake = CreateFake(graph.Type, parameters);
                fakes[graph.Type] = fake;
                return fake;
            }
            else
            {
                var fake = CreateFake(graph.Type);
                fakes[graph.Type] = fake;
                return fake;
            }
        }

        static object CreateFake(Type type)
        {
            var mockOpenType = typeof(A);
            var fakeMethod = mockOpenType.GetMethod("Fake", System.Type.EmptyTypes);

            MethodInfo createMethod = fakeMethod.MakeGenericMethod(new[] { type });
            try
            {
                return createMethod.Invoke(null, null);
            }
            catch
            {
                return null;
            }
        }

        static object CreateFake(Type type, List<Type> args)
        {
            var mockOpenType = typeof(A);
            var fakeMethod = mockOpenType.GetMethod("Fake", System.Type.EmptyTypes);
            var fakeOptionsBuilderType = typeof(Creation.IFakeOptionsBuilder<>);

            var optionsBuilderGeneric = fakeOptionsBuilderType.MakeGenericType(new[] { type });
            var optionsBuilderWrapped = fakeOptionsBuilderType.MakeGenericType(new[] {
                    typeof(Action<>).MakeGenericType(new[] {
                        optionsBuilderGeneric
                    })
                });
            var actionTypeWrapped = typeof(Action<>).MakeGenericType(new[] { optionsBuilderWrapped });

            var actionType = typeof(Action<>).MakeGenericType(new[] { 
                optionsBuilderGeneric
            });

            var fakeWithArgsMethod = mockOpenType.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(p => p.Name == "Fake"
                && (args.Any() && p.GetParameters().Count() > 0 || !args.Any() && p.GetParameters().Count() == 0)).FirstOrDefault();
            var fakeWithArgsMethodGeneric = fakeWithArgsMethod.MakeGenericMethod(new[] { type });

            LambdaExpression lambda = null;

            if (args.Any())
            {
                var constructor = type.GetConstructors().OrderByDescending(p => p.GetParameters().Length).First();
                var econ = LambdaExpression.New(constructor, args.Select(p => Expression.Default(p)));
                var param = Expression.Parameter(optionsBuilderGeneric, "var1");

                lambda = Expression.Lambda(actionType, econ, param);
            }

            var fake = fakeWithArgsMethodGeneric.Invoke(null, lambda != null ? new object[] { lambda.Compile() } : null);

            return fake;
        }

        static object CreateFake(Type type, List<object> args)
        {
            var mockOpenType = typeof(A);
            var fakeMethod = mockOpenType.GetMethod("Fake", System.Type.EmptyTypes);
            var fakeOptionsBuilderType = typeof(Creation.IFakeOptionsBuilder<>);

            var optionsBuilderGeneric = fakeOptionsBuilderType.MakeGenericType(new[] { type });
            var optionsBuilderWrapped = fakeOptionsBuilderType.MakeGenericType(new[] {
                    typeof(Action<>).MakeGenericType(new[] {
                        optionsBuilderGeneric
                    })
                });
            var actionTypeWrapped = typeof(Action<>).MakeGenericType(new[] { optionsBuilderWrapped });

            var actionType = typeof(Action<>).MakeGenericType(new[] { 
                optionsBuilderGeneric
            });

            var fakeWithArgsMethod = mockOpenType.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(p => p.Name == "Fake" 
                && (args.Any() && p.GetParameters().Count() > 0 || !args.Any() && p.GetParameters().Count() == 0)).FirstOrDefault();
            var fakeWithArgsMethodGeneric = fakeWithArgsMethod.MakeGenericMethod(new[] { type });

            LambdaExpression lambda = null;

            if (args.Any())
            {
                var withArgsMethod = optionsBuilderGeneric.GetMethod("WithArgumentsForConstructor", new Type[] { typeof(IEnumerable<object>) });

                var paramValues = Expression.Constant(args, typeof(List<object>));
                var genType = Expression.Parameter(type);
                var param = Expression.Parameter(optionsBuilderGeneric, "var1");
                var arguments = Expression.Parameter(args.GetType(), "var2");
                var invoke = Expression.Call(param, withArgsMethod, paramValues);
                lambda = Expression.Lambda(actionType, invoke, param);
            }
            
            var fake = fakeWithArgsMethodGeneric.Invoke(null, lambda != null ? new object[] { lambda.Compile() } : null);

            return fake;
        }
    }
}
