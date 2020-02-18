using System.Linq;
using Xamarin.Forms;
using XF.Material.Forms.UI;

namespace IndoorKonfiguration.Utilities
{
    public class NumericValidationBehavior : Behavior<MaterialTextField>
    {
        protected override void OnAttachedTo(MaterialTextField entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(MaterialTextField entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private static void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {

            if (!string.IsNullOrWhiteSpace(args.NewTextValue))
            {
                bool isValid = args.NewTextValue.ToCharArray().All(x => char.IsDigit(x)); //Make sure all characters are numbers

                (sender as MaterialTextField).Text = isValid ? args.NewTextValue : args.NewTextValue.Remove(args.NewTextValue.Length - 1);
            }
        }


    }
}
