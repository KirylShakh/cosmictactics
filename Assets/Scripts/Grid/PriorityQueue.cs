using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PriorityQueue<T>
{
    public class Tuple<X, Y>
    {
        public X first;
        public Y last;

        public Tuple(X first, Y last) => (this.first, this.last) = (first, last);
    }

    private List<Tuple<T, float>> Elements = new List<Tuple<T, float>>();

    public int Count() => Elements.Count;

    public void Enqueue(T item, float priority) => Elements.Add(new Tuple<T, float>(item, priority));

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 0; i < Elements.Count; i++)
        {
            if (Elements[i].last < Elements[bestIndex].last)
            {
                bestIndex = i;
            }
        }

        T bestItem = Elements[bestIndex].first;
        Elements.RemoveAt(bestIndex);
        return bestItem;
    }
}
