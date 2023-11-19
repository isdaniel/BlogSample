using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace YieldSample
{
    public class Counter02 : IEnumerator<int>
    {
        private int _end;
        private int _current = 0;

        public Counter02(int end)
        {
            this._end = end;
            this.Reset();
        }

        public int Current => this._current;

        public void Dispose()
        {
        }

        object IEnumerator.Current => this._current;

        public bool MoveNext()
        {
            do
            {
                _current++;
            } while (_current %2 !=0);
            
            return !(this._current > this._end);
        }

        public void Reset()
        {
           this._current = 0;
        }
    }

    public class CounterEnumerable02 : IEnumerable<int>
    {
        private int end;
        public CounterEnumerable02(int end)
        {
            this.end = end;
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new Counter02(end);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}