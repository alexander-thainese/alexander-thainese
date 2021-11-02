using System;

namespace CMT.Common
{
    public class ValueRequestEventArgs<T> : EventArgs
    {
        public string Name { get; private set; }
        public T Value { get; set; }


        public ValueRequestEventArgs(string name)
        {
            Name = name;
        }
    }
}
