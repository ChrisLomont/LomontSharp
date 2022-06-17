using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Numerical
{
    /// <summary>
    /// Represent a generic distance function
    /// </summary>
    /// <typeparam name="TSelf"></typeparam>
    /// <typeparam name="TOther"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IDistance<TSelf, TOther, TResult>
    {
        static abstract TResult Distance(TSelf a, TOther b);
    }
}
