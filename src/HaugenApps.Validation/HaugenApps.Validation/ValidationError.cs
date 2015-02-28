using System.Reflection;

namespace HaugenApps.Validation
{
    public abstract class ValidationError
    {
        public ValidationError(PropertyInfo Property)
        {
            this.Property = Property;
        }

        public PropertyInfo Property { get; set; }

        protected abstract string ErrorDescription { get; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", this.Property.Name, this.ErrorDescription);
        }
    }
}
