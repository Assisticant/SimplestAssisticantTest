using System;
using Assisticant.Fields;

namespace SimplestAssisticantTest
{
    public class TestModel
    {
        private Observable<string> _Name = new Observable<string>();
        private Observable<int> _Count = new Observable<int>();

        public string Name { get { return _Name; } set { _Name.Value = value; } }
        public int Count { get { return _Count; } set { _Count.Value = value; } }


        public string NameAndCount
        {
            get
            {
                return Name + " - " + Count.ToString();
            }
        }
        public void IncCount()
        {
            Count++;
        }
    }
}
