using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piceon.Models
{
    public class EditableTextBlockTextChangedEventArgs : EventArgs
    {
        public string Text { get; }

        public EditableTextBlockTextChangedEventArgs(string txt) => Text = txt;
    }
}
