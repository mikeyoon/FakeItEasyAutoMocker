# Fake It Easy Automocker

## Introduction

This tool is designed to make it easier to create a class under test for unit testing. The automocker will create a concrete class of the given type and fill in all its dependencies using FakeItEasy. It also has a function for retrieving the instance of a faked dependency so it can be setup. One of the major differences between this automocker and others is that FakeItEasy.Automocker will automock concerete dependencies, and not just interfaces. Since it still relies on FakeItEasy, your concrete dependencies still have to have all virtual functions for it to work properly.

Depends on FakeItEasy (https://github.com/FakeItEasy/FakeItEasy), of course.

## Usage

Basic example for now. Should be enough to get started.

    var container = B.AutoMock<Subject>();
    A.CallTo(() => container.GetMock<IDependency>().whatever()).Returns("Faked!!");
    A.CallTo(() => container.GetMock<ADependency>().GetString()).Returns("Faked As Well!!");
    container.Subject.DoStuff();

Property Injection example. On top of doing the constructor injection, it will also inject all public properties that end with "Service". There is also an option to do type matching with the TypeMatches function instead.

    var container = B.AutoMock<Subject>(p => p.NameMatches(name => name.EndsWith("Service")));

## Todo/Issues

1. FakeItEasy can't handle types that don't have usable constructors, so I had it return null for those. In most cases, that shouldn't be a problem as all the dependencies you'd want to setup probably have a public constructor.
2. There is a lot of reflection going on that isn't cached/optimized. I'll hopefully get to this later.
3. There is an option to turn off the automocker's recursive faking and rely instead on FakeItEasy. If you do this, you can only retrieve fakes one level deep in the graph (the subject's direct dependencies), and anything FakeItEasy can't mock properly will throw an error.

##License

LGPL (http://www.gnu.org/licenses/lgpl.html)
