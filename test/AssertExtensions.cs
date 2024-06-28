using NUnit.Framework.Constraints;

namespace test;

public static class AssertExtensions
{
    // public static R AssertAndReturnType<T, R>(T actual, Type expression)
    // {
    //     ArgumentNullException.ThrowIfNull(actual);
    //     if (actual is R a)
    //         return a;
    //     else
    //     {
    //         Assert.That(actual, Is.TypeOf(expression));
    //         throw new Exception();
    //     }

    // }
    public static void AssertReturnType<T, R>(T? actual, out R value)
        where T : class
    {
        Assert.That(actual, Is.Not.Null);
        if (actual is R a)
            value = a;
        else
        {
            throw new Exception();
        }
    }
}
