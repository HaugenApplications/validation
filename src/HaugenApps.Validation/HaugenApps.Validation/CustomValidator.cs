using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HaugenApps.ChangeTracking;
using HaugenApps.HaugenCore;

namespace HaugenApps.Validation
{
    public abstract class CustomValidator
    {
        protected readonly Type Type;

        public CustomValidator(Type type)
        {
            this.Type = type;
        }

        public abstract IEnumerable<ValidationError> Validate(Dictionary<PropertyInfo, object> vals);

        public IEnumerable<ValidationError> Validate(object Instance)
        {
            if (!this.Type.IsInstanceOfType(Instance))
                throw new ArgumentException("Instance type must be in validator's type hierarchy.");

            return Validate(new PropertyWatcher(this.Type, Instance));
        }
        public IEnumerable<ValidationError> Validate(PropertyWatcher PropertyWatcher)
        {
            var vals = PropertyWatcher.GetValues().ToDictionary(c => c.Key, c => c.Value);

            return Validate(vals);
        }
    }

    public abstract class ListCustomValidator : CustomValidator
    {
        public ListCustomValidator(Type type) : base(type) { }

        protected readonly HashSet<PropertyInfo> Properties = new HashSet<PropertyInfo>();

        private IEnumerable<PropertyInfo> VerifyType(IEnumerable<PropertyInfo> Fields)
        {
            foreach (var v in Fields)
            {
                if (this.Type.IsAssignableFrom(v.DeclaringType))
                    yield return v;
                else
                    throw new ArgumentException("All arguments must be declared in the validator's type hierarchy.");
            }
        }
        internal ListCustomValidator Add(IEnumerable<PropertyInfo> Fields, bool VerifyType)
        {
            Fields = VerifyType ? this.VerifyType(Fields) : Fields;
            foreach (var v in Fields)
            {
                this.Properties.Add(v);
            }

            return this;
        }

        public ListCustomValidator Add(params PropertyInfo[] Fields)
        {
            return Add(Fields.AsEnumerable(), true);
        }
        public ListCustomValidator Add(params string[] Fields)
        {
            return Add(Fields.Select(this.Type.GetProperty), false);
        }
    }
    public abstract class ListCustomValidator<T, TPropertyType> : ListCustomValidator
    {
        public ListCustomValidator() : base(typeof(T)) { }

        public ListCustomValidator<T, TPropertyType> Add(params Expression<Func<T, TPropertyType>>[] Fields)
        {
            Add(Fields.Select(Reflection.GetPropertyInfo), false);

            return this;
        }
        public new ListCustomValidator<T, TPropertyType> Add(params PropertyInfo[] Fields)
        {
            base.Add(Fields.AsEnumerable(), true);

            return this;
        }
        public new ListCustomValidator<T, TPropertyType> Add(params string[] Fields)
        {
            base.Add(Fields.Select(this.Type.GetProperty), false);

            return this;
        }
    }
    public abstract class IndividualFieldCustomValidator<TPropertyType> : ListCustomValidator
    {
        public IndividualFieldCustomValidator(Type type)
            : base(type)
        {
        }

        public override IEnumerable<ValidationError> Validate(Dictionary<PropertyInfo, object> vals)
        {
            return this.Properties.Join(vals, a => a, b => b.Key, (a, b) => b).SelectMany(c => Validate(c.Key, (TPropertyType)c.Value));
        }

        public abstract IEnumerable<ValidationError> Validate(PropertyInfo Property, TPropertyType Value);

    }
}
