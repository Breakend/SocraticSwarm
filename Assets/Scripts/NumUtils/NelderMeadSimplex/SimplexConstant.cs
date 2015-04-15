using System;
using System.Collections.Generic;
using System.Text;

namespace NumUtils.NelderMeadSimplex
{
    public sealed class SimplexConstant
    {
        private double _value;
        private double _initialPerturbation;

        public SimplexConstant(double value, double initialPerturbation)
        {
            _value = value;
            _initialPerturbation = initialPerturbation;
        }

        /// <summary>
        /// The value of the constant
        /// </summary>
        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }

        // The size of the initial perturbation
        public double InitialPerturbation
        {
            get { return _initialPerturbation; }
            set { _initialPerturbation = value; }
        }
    }
}
