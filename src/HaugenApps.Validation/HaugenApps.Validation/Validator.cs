using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HaugenApps.ChangeTracking;
using HaugenApps.HaugenCore;
using HaugenApps.Validation.BuiltInValidators;

namespace HaugenApps.Validation
{
    public class Validator
    {
        public Validator(Type type)
        {
            this._type = type;
            this._required = new RequiredValidator(this._type);
            this._notNull = new NotNullValidator(this._type);
        }

        private readonly RequiredValidator _required;
        private readonly NotNullValidator _notNull;

        private readonly Type _type;

        private readonly List<CustomValidator> _validators = new List<CustomValidator>();

        public Validator AddValidators(params CustomValidator[] Validator)
        {
            this._validators.AddRange(Validator);

            return this;
        }

        protected Validator Required(IEnumerable<PropertyInfo> Fields, bool VerifyType)
        {
            this._required.Add(Fields, VerifyType);

            return this;
        }
        protected Validator NotNull(IEnumerable<PropertyInfo> Fields, bool VerifyType)
        {
            this._notNull.Add(Fields, VerifyType);

            return this;
        }
        protected Validator StringLength(int? MinimumLength, int? MaximumLength, IEnumerable<PropertyInfo> Fields, bool VerifyType)
        {
            AddValidators(new StringLengthValidator(MinimumLength, MaximumLength, this._type).Add(Fields, VerifyType));

            return this;
        }
        protected Validator DateRange(DateTime? Minimum, DateTime? Maximum, IEnumerable<PropertyInfo> Fields, bool VerifyType)
        {
            AddValidators(new DateRangeValidator(Minimum, Maximum, this._type).Add(Fields, VerifyType));

            return this;
        }
        protected Validator DateRange(Func<DateTime> Minimum, Func<DateTime> Maximum, IEnumerable<PropertyInfo> Fields, bool VerifyType)
        {
            AddValidators(new DateRangeValidator(Minimum, Maximum, this._type).Add(Fields, VerifyType));

            return this;
        }

        public IEnumerable<ValidationError> Validate(PropertyWatcher PropertyWatcher)
        {
            var vals = PropertyWatcher.GetValues().ToDictionary(c => c.Key, c => c.Value);

            return Validate(vals);
        }

        protected virtual IEnumerable<ValidationError> Validate(Dictionary<PropertyInfo, object> vals)
        {
            return this._required.Validate(vals).Concat(this._notNull.Validate(vals)).Concat(this._validators.SelectMany(c => c.Validate(vals)));
        }

        public IEnumerable<ValidationError> Validate(object Instance)
        {
            if (!this._type.IsInstanceOfType(Instance))
                throw new ArgumentException("Instance type must be in validator's type hierarchy.");

            return Validate(new PropertyWatcher(this._type, Instance));
        }
    }
    public class Validator<T> : Validator
    {
        public Validator() : base(typeof(T)) { }

        public new Validator<T> AddValidators(params CustomValidator[] Validator)
        {
            base.AddValidators(Validator);

            return this;
        }

        public Validator<T> RequiredAndNotNull(params Expression<Func<T, object>>[] Fields)
        {
            return Required(Fields).NotNull(Fields);
        }
        public Validator<T> Required(params Expression<Func<T, object>>[] Fields)
        {
            return (Validator<T>)Required(Fields.Select(Reflection.GetPropertyInfo), false);
        }

        public Validator<T> NotNull(params Expression<Func<T, object>>[] Fields)
        {
            return (Validator<T>)NotNull(Fields.Select(Reflection.GetPropertyInfo), false);
        }

        public Validator<T> StringLength(int? MinimumLength, int? MaximumLength, params Expression<Func<T, string>>[] Fields)
        {
            return (Validator<T>)StringLength(MinimumLength, MaximumLength, Fields.Select(Reflection.GetPropertyInfo), false);
        }
        public Validator<T> DateRange(DateTime? Minimum, DateTime? Maximum, params Expression<Func<T, DateTime?>>[] Fields)
        {
            return (Validator<T>)DateRange(Minimum, Maximum, Fields.Select(Reflection.GetPropertyInfo), false);
        }
        public Validator<T> DateRange(Func<DateTime> Minimum, Func<DateTime> Maximum, params Expression<Func<T, DateTime?>>[] Fields)
        {
            return (Validator<T>)DateRange(Minimum, Maximum, Fields.Select(Reflection.GetPropertyInfo), false);
        }

        public IEnumerable<ValidationError> Validate(PropertyWatcher<T> PropertyWatcher)
        {
            return base.Validate(PropertyWatcher);
        }

        public IEnumerable<ValidationError> Validate(T Instance)
        {
            return base.Validate(Instance);
        }
    }
}
