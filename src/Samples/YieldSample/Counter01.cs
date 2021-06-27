using System.Collections;
using System.Collections.Generic;

namespace YieldSample
{
    public class Counter01 : IEnumerator<int>
    {
        private int _end;
        private int _current = 0;

        public Counter01(int end)
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
            this._current++;
            return !(this._current > this._end);
        }

        public void Reset()
        {
            this._current = 0;
        }
    }
}