using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piceon.Models
{
    public class StateMessage
    {
        private static int IdCntr = 0;

        public int Id { get; private set; }
        public bool IsLoading { get; private set; }
        public string Text { get; private set; }

        public StateMessage(bool isLoading, string text)
        {
            Id = IdCntr++;
            IsLoading = isLoading;
            Text = text;
        }

        public override bool Equals(object obj)
        {
            return obj is StateMessage message &&
                   Id == message.Id &&
                   Text == message.Text;
        }

        public override int GetHashCode()
        {
            int hashCode = -1144598946;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            return hashCode;
        }

        public static bool operator ==(StateMessage m1, StateMessage m2)
        {
            if ((m1 is object && m2 is null) ||
                (m1 is null && m2 is object))
                return false;

            if (m1 is null && m2 is null)
                return true;

            return (m1.Id == m2.Id &&
                m1.Text == m2.Text);
        }

        public static bool operator !=(StateMessage m1, StateMessage m2)
        {
            if ((m1 is object && m2 is null) ||
                (m1 is null && m2 is object))
                return true;

            if (m1 is null && m2 is null)
                return false;

            return !(m1.Id == m2.Id &&
                m1.Text == m2.Text);
        }
    }
}
