using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Piceon.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Piceon.TemplateSelectors
{
    public class DirectoryTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return GetTemplate(item) ?? base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return GetTemplate(item) ?? base.SelectTemplateCore(item, container);
        }

        private DataTemplate GetTemplate(object item)
        {
            switch (item)
            {
                case DirectoryItem directoryItem:
                    return DirectoryTemplate;
            }

            return null;
        }
    }
}
