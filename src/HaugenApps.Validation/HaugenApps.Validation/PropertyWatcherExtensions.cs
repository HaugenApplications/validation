using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HaugenApps.ChangeTracking;

namespace HaugenApps.Validation
{
    public static class PropertyWatcherExtensions
    {
        public static IEnumerable<ValidationError> Validate<T>(this PropertyWatcher<T> Saved, Func<Validator<T>, Validator<T>> Validate)
        {
            var ret = Validate(new Validator<T>());

            return ret.Validate(Saved);
        }
    }
}
