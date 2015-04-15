using System;
using System.Collections.Generic;
using System.Text;

namespace NumUtils.NelderMeadSimplex
{
    public sealed class RegressionResult
    {
        private TerminationReason _terminationReason;
        private double[] _constants;
        private double _errorValue;
        private int _evaluationCount;

        public RegressionResult(TerminationReason terminationReason, double[] constants, double errorValue, int evaluationCount)
        {
            _terminationReason = terminationReason;
            _constants = constants;
            _errorValue = errorValue;
            _evaluationCount = evaluationCount;
        }

        public TerminationReason TerminationReason
        {
            get { return _terminationReason; }
        }

        public double[] Constants
        {
            get { return _constants; }
        }

        public double ErrorValue
        {
            get { return _errorValue; }
        }

        public int EvaluationCount
        {
            get { return _evaluationCount; }
        }
    }

    public enum TerminationReason
    {
        MaxFunctionEvaluations,
        Converged,
        Unspecified
    }

}
