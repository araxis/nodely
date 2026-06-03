#if !NET6_0_OR_GREATER
namespace System.Collections.Generic
{
    /// <summary>
    /// Minimal binary min-heap <c>PriorityQueue&lt;TElement, TPriority&gt;</c> polyfill for netstandard2.0
    /// (the BCL type ships with .NET 6+). Only the members Nodely uses are implemented.
    /// </summary>
    internal sealed class PriorityQueue<TElement, TPriority>
    {
        private readonly List<(TElement Element, TPriority Priority)> _nodes = new List<(TElement, TPriority)>();
        private readonly IComparer<TPriority> _comparer = Comparer<TPriority>.Default;

        public int Count => _nodes.Count;

        public void Enqueue(TElement element, TPriority priority)
        {
            _nodes.Add((element, priority));
            SiftUp(_nodes.Count - 1);
        }

        public TElement Dequeue()
        {
            if (_nodes.Count == 0)
                throw new InvalidOperationException("The priority queue is empty.");

            var root = _nodes[0].Element;
            var lastIndex = _nodes.Count - 1;
            _nodes[0] = _nodes[lastIndex];
            _nodes.RemoveAt(lastIndex);

            if (_nodes.Count > 0)
                SiftDown(0);

            return root;
        }

        private void SiftUp(int i)
        {
            while (i > 0)
            {
                var parent = (i - 1) / 2;
                if (_comparer.Compare(_nodes[i].Priority, _nodes[parent].Priority) >= 0)
                    break;

                (_nodes[i], _nodes[parent]) = (_nodes[parent], _nodes[i]);
                i = parent;
            }
        }

        private void SiftDown(int i)
        {
            var count = _nodes.Count;
            while (true)
            {
                var left = 2 * i + 1;
                var right = 2 * i + 2;
                var smallest = i;

                if (left < count && _comparer.Compare(_nodes[left].Priority, _nodes[smallest].Priority) < 0)
                    smallest = left;
                if (right < count && _comparer.Compare(_nodes[right].Priority, _nodes[smallest].Priority) < 0)
                    smallest = right;
                if (smallest == i)
                    break;

                (_nodes[i], _nodes[smallest]) = (_nodes[smallest], _nodes[i]);
                i = smallest;
            }
        }
    }
}
#endif
