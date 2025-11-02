using System;
using System.Collections.Generic;
using System.Linq;

namespace ScanPlaneMaker
{
    public class StabilityDetector
    {
        private readonly Queue<double> _values = new Queue<double>();
        private readonly int _windowSize;
        private readonly double _tolerance;

        public double _tunnel;

        public StabilityDetector(int windowSize, double tolerance)
        {
            _windowSize = windowSize;
            _tolerance = tolerance;
        }

        public void AddValue(double value)
        {
            _values.Enqueue(value);
            if (_values.Count > _windowSize)
                _values.Dequeue();
        }

        public bool IsStable()
        {
            if (_values.Count < _windowSize)
                return false;

            double min = _values.Min();
            double max = _values.Max();
            _tunnel = (max - min);
            return _tunnel <= (_tolerance * 2); // ± tolerance autour d'une valeur centrale
        }
    }
}