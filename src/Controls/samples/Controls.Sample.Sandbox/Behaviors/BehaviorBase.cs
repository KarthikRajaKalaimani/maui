#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Behaviors
{
    public abstract class BehaviorBase<T> : Behavior<T> where T : BindableObject
    {

        protected T AssociatedObject { get; private set; }
        protected override void OnAttachedTo(T bindable)
        {
            if (this.AssociatedObject != null && this.AssociatedObject != bindable)
                throw new InvalidOperationException(String.Format(System.Globalization.CultureInfo.CurrentCulture, "The behaviour {0} can only be used once per associated object. Do not use this behaviour in styles or triggers where Xamarin Forms attempts to share instances, or attempt to reuse an instance on multiple objects yourself.", this.GetType().FullName));
            base.OnAttachedTo(bindable);
            this.AssociatedObject = bindable;
            if (bindable == null)
                this.BindingContext = null;
            else
            {
                bindable.BindingContextChanged += associatedObject_BindingContextChanged;
                this.BindingContext = bindable.BindingContext;
            }
        }

        protected override void OnDetachingFrom(T bindable)
        {
            base.OnDetachingFrom(bindable);
            if (bindable != null)
                bindable.BindingContextChanged -= this.associatedObject_BindingContextChanged;
            this.AssociatedObject = null;
        }
        private void associatedObject_BindingContextChanged(object sender, EventArgs e)
        {
            var associatedObject = this.AssociatedObject;
            if (associatedObject != null)
                this.BindingContext = associatedObject.BindingContext;
        }
    }
}
