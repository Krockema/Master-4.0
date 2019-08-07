using System;

namespace Zpp.WrappersForPrimitives
{
    public class DueTime : IComparable<DueTime>,IComparable 
    {
        private int _dueTime;

        public DueTime(int dueTime)
        {
            _dueTime = dueTime;
        }

        public int GetValue()
        {
            return _dueTime;
        }
        
        public int CompareTo(DueTime that)
        {
                return _dueTime.CompareTo(that.GetValue());
        }

        public int CompareTo(object obj)
        {
            DueTime otherDueTime = (DueTime)obj;
            return _dueTime.CompareTo(otherDueTime.GetValue());
        }

        public override bool Equals(object obj)
        {
            DueTime otherDueTime = (DueTime) obj;
            return _dueTime.Equals(otherDueTime._dueTime);
        }

        public override int GetHashCode()
        {
            return _dueTime.GetHashCode();
        }

        public override string ToString()
        {
            return _dueTime.ToString();
        }

        public bool IsNull()
        {
            return _dueTime.Equals(0);
        }
    }
}