using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T> {

    public class Tuple<X, Y> {

        public X first;
        public Y last;

        public Tuple(X first, Y last) {
            this.first = first;
            this.last = last;
        }
    }

    private List<Tuple<T, float>> elements = new List<Tuple<T, float>>();

    public int Count() {
        return elements.Count;
    }

    public void Enqueue(T item, float priority) {
        elements.Add(new Tuple<T, float>(item, priority));
    }

    public T Dequeue() {
        int bestIndex = 0;
        for (int i = 0; i < elements.Count; i++) {
            if (elements[i].last < elements[bestIndex].last) {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].first;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}
