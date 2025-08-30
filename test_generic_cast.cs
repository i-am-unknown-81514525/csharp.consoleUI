using System;

public class A { }
public class B : A { }
public class D : A { }
public class E : B { }

public class C<T> where T : A
{
    public T fn()
    {
        try
        {
            T result = (T)(new B() as T);
            if (result is null)
            {
                throw new NotImplementedException("");
            }
            return result;
        }
        catch (InvalidCastException)
        {
            throw new NotImplementedException("");
        }
    }
}

// C<A> a = new C<A>();
// a.fn()
// C<B> b = new C<B>();
// b.fn()
// C<D> d = new C<D>();
// d.fn()
// C<E> e = new C<E>();
// e.fn()
