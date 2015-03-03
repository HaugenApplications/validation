using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace HaugenApps.Validation.BuiltInValidators
{
    public class RequiredValidator : ListCustomValidator
    {
        public RequiredValidator(Type type) : base(type) { }

        public override IEnumerable<ValidationError> Validate(Dictionary<PropertyInfo, object> vals)
        {
            foreach (var v in this.Properties.Except(vals.Keys))
            {
                yield return new RequiredFieldOmitted(v);
            }
        }

        public class RequiredFieldOmitted : ValidationError
        {
            public RequiredFieldOmitted(PropertyInfo Property) : base(Property) { }

            protected override string ErrorDescription
            {
                get { return "Required field was omitted."; }
            }
        }
    }
    
    public class NotNullValidator : IndividualFieldCustomValidator<object>
    {
        public NotNullValidator(Type type)
            : base(type)
        {
        }

        public override IEnumerable<ValidationError> Validate(PropertyInfo Property, object Value)
        {
            if (Value == null)
                return new[] { new NullFieldDetected(Property) };
            return Enumerable.Empty<ValidationError>();
        }

        public class NullFieldDetected : ValidationError
        {
            public NullFieldDetected(PropertyInfo Property) : base(Property) { }

            protected override string ErrorDescription
            {
                get { return "Non-nullable field was null."; }
            }
        }
    }

    public class StringLengthValidator : IndividualFieldCustomValidator<string>
    {
        public StringLengthValidator(int? MinLength, int? MaxLength, Type type)
            : base(type)
        {
            this.MinLength = MinLength;
            this.MaxLength = MaxLength;
        }

        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }

        public override IEnumerable<ValidationError> Validate(PropertyInfo Property, string Value)
        {
            if (Value != null && ((this.MinLength.HasValue && this.MinLength > Value.Length) || (this.MaxLength.HasValue && this.MaxLength.Value < Value.Length)))
                return new[] { new MinLengthNotFulfilled(Property) };
            return Enumerable.Empty<ValidationError>();
        }

        public class MinLengthNotFulfilled : ValidationError
        {
            public MinLengthNotFulfilled(PropertyInfo Property) : base(Property) { }

            protected override string ErrorDescription
            {
                get { return "Minimum length was not fulfilled."; }
            }
        }
    }
    
    public class DateRangeValidator : IndividualFieldCustomValidator<DateTime?>
    {
        public DateRangeValidator(DateTime? Minimum, DateTime? Maximum, Type type)
            : base(type)
        {
            this.Minimum = Minimum.HasValue ? (() => Minimum.Value) : (Func<DateTime>)null;
            this.Maximum = Maximum.HasValue ? (() => Maximum.Value) : (Func<DateTime>)null;
        }
        public DateRangeValidator(Func<DateTime> Minimum, Func<DateTime> Maximum, Type type)
            : base(type)
        {
            this.Minimum = Minimum;
            this.Maximum = Maximum;
        }

        public Func<DateTime> Minimum { get; set; }
        public Func<DateTime> Maximum { get; set; }

        public override IEnumerable<ValidationError> Validate(PropertyInfo Property, DateTime? Value)
        {
            if (Value.HasValue && ((this.Minimum != null && this.Minimum() > Value) || (this.Maximum != null && this.Maximum() < Value)))
                return new[] { new DateOutOfRange(Property) };

            return Enumerable.Empty<ValidationError>();
        }

        public class DateOutOfRange : ValidationError
        {
            public DateOutOfRange(PropertyInfo Property) : base(Property) { }

            protected override string ErrorDescription
            {
                get { return "Date was out of the expected range."; }
            }
        }
    }

    public class DataAnnotationValidator : IndividualFieldCustomValidator<object>
    {
        public DataAnnotationValidator(Type type) : base(type)
        {
        }

        public override IEnumerable<ValidationError> Validate(PropertyInfo Property, object Value)
        {
            return Property.GetCustomAttributes<ValidationAttribute>().Where(c => !c.IsValid(Value)).Select(c => new DataAnnotationValidationError(c, Property));
        }

        public class DataAnnotationValidationError : ValidationError
        {
            public DataAnnotationValidationError(ValidationAttribute attribute, PropertyInfo Property)
                : base(Property)
            {
                this.Attribute = attribute;
            }

            protected override string ErrorDescription
            {
                get { return this.Attribute.FormatErrorMessage(this.Property.Name); }
            }

            public ValidationAttribute Attribute { get; private set; }
        }
    }
}
