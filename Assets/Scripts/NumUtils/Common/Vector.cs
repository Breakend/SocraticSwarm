using System;
using System.Collections.Generic;
using System.Text;

namespace NumUtils.Common
{
    public class Vector
    {
        private double[] _components;
        private int _nDimensions;
        public Vector(int dimensions)
        {
            _components = new double[dimensions];
            _nDimensions = dimensions;
        }

        public int NDimensions
        {
            get { return _nDimensions; }
        }

        public double this[int index]
        {
            get { return _components[index]; }
            set { _components[index] = value; }
        }

        public double[] Components
        {
            get { return _components; }
        }

        /// <summary>
        /// Add another vector to this one
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector Add(Vector v)
        {
            if (v.NDimensions != this.NDimensions)
                throw new ArgumentException("Can only add vectors of the same dimensionality");

            Vector vector = new Vector(v.NDimensions);
            for (int i = 0; i < v.NDimensions; i++)
            {
                vector[i] = this[i] + v[i];
            }
            return vector;
        }

        /// <summary>
        /// Subtract another vector from this one
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector Subtract(Vector v)
        {
            if (v.NDimensions != this.NDimensions)
                throw new ArgumentException("Can only subtract vectors of the same dimensionality");

            Vector vector = new Vector(v.NDimensions);
            for (int i = 0; i < v.NDimensions; i++)
            {
                vector[i] = this[i] - v[i];
            }
            return vector;
        }

        /// <summary>
        /// Multiply this vector by a scalar value
        /// </summary>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public Vector Multiply(double scalar)
        {
            Vector scaledVector = new Vector(this.NDimensions);
            for (int i = 0; i < this.NDimensions; i++)
            {
                scaledVector[i] = this[i] * scalar;
            }
            return scaledVector;
        }

        /// <summary>
        /// Compute the dot product of this vector and the given vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double DotProduct(Vector v)
        {
            if (v.NDimensions != this.NDimensions)
                throw new ArgumentException("Can only compute dot product for vectors of the same dimensionality");

            double sum = 0;
            for (int i = 0; i < v.NDimensions; i++)
            {
                sum += this[i] * v[i];
            }
            return sum;
        }

        public override string ToString()
        {
            string[] components = new string[_components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                components[i] = _components[i].ToString();
            }
            return "[ " + string.Join(", ", components) + " ]";
        }
    }
}
