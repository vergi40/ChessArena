using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace vergiBlueDesktop
{
    /// <summary>
    /// Inherit each class using notifyproperties from this
    /// </summary>
    public abstract class NotifyPropertyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged(string propertyName)
        {

            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));

        }

        public void OnPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression unaryExpression)
            {
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }
            OnPropertyChanged(memberExpression.Member.Name);
        }

    }
}