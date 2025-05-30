﻿using Extensions;
using System.Drawing;
using System.Numerics;
using XREngine.Data.Core;
using static System.MathF;

namespace XREngine.Data
{
    /// <summary>
    /// Provides tools pertaining to interpolation.
    /// </summary>
    public static class Interp
    {
        #region Polynomials
        public static float EvaluatePolynomial(float third, float second, float first, float zero, float x)
        {
            float x2 = x * x;
            return third * x2 * x + second * x2 + first * x + zero;
        }
        public static Vector2 EvaluatePolynomial(Vector2 third, Vector2 second, Vector2 first, Vector2 zero, float x)
        {
            Vector2 x2 = new(x * x);
            return third * x2 * x + second * x2 + first * x + zero;
        }
        public static Vector3 EvaluatePolynomial(Vector3 third, Vector3 second, Vector3 first, Vector3 zero, float x)
        {
            Vector3 x2 = new(x * x);
            return third * x2 * x + second * x2 + first * x + zero;
        }
        public static Vector4 EvaluatePolynomial(Vector4 third, Vector4 second, Vector4 first, Vector4 zero, float x)
        {
            Vector4 x2 = new(x * x);
            return third * x2 * x + second * x2 + first * x + zero;
        }
        public static float EvaluatePolynomial(float second, float first, float zero, float x)
        {
            float x2 = x * x;
            return second * x2 + first * x + zero;
        }
        public static Vector2 EvaluatePolynomial(Vector2 second, Vector2 first, Vector2 zero, float x)
        {
            Vector2 x2 = new(x * x);
            return second * x2 + first * x + zero;
        }
        public static Vector3 EvaluatePolynomial(Vector3 second, Vector3 first, Vector3 zero, float x)
        {
            Vector3 x2 = new(x * x);
            return second * x2 + first * x + zero;
        }
        public static Vector4 EvaluatePolynomial(Vector4 second, Vector4 first, Vector4 zero, float x)
        {
            Vector4 x2 = new(x * x);
            return second * x2 + first * x + zero;
        }
        public static float EvaluatePolynomial(float first, float zero, float x)
            => first * x + zero;
        public static Vector2 EvaluatePolynomial(Vector2 first, Vector2 zero, float x)
            => first * x + zero;
        public static Vector3 EvaluatePolynomial(Vector3 first, Vector3 zero, float x)
            => first * x + zero;
        public static Vector4 EvaluatePolynomial(Vector4 first, Vector4 zero, float x)
            => first * x + zero;
        #endregion

        #region Bezier

        #region Point Approximation
        public static Vector2[] GetBezierPoints(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int pointCount, out float length)
        {
            Vector2[] points = new Vector2[pointCount];

            // var q is the change in t between successive evaluations.
            float q = 1.0f / (pointCount - 1); // q is dependent on the number of GAPS = POINTS-1
            float q2 = q * q;
            float q3 = q2 * q;

            // coefficients of the cubic polynomial that we're FDing -
            Vector2 a = p0;
            Vector2 b = 3 * (p1 - p0);
            Vector2 c = 3 * (p2 - 2 * p1 + p0);
            Vector2 d = p3 - 3 * p2 + 3 * p1 - p0;

            // initial values of the poly and the 3 diffs -
            Vector2 s = a;                          // the poly value
            Vector2 u = b * q + c * q2 + d * q3;    // 1st order diff (quadratic)
            Vector2 v = 2 * c * q2 + 6 * d * q3;    // 2nd order diff (linear)
            Vector2 w = 6 * d * q3;                 // 3rd order diff (constant)

            length = 0.0f;
            points[0] = p0;

            for (int i = 1; i < pointCount; ++i)
            {
                Vector2 prevPos = s;

                s += u;
                u += v;
                v += w;

                points[i] = s;
                length += (s - prevPos).Length();
            }
            return points;
        }
        public static Vector2[] GetBezierPoints(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int pointCount)
        {
            if (pointCount < 2)
                throw new InvalidOperationException();

            Vector2[] points = new Vector2[pointCount];
            float timeDelta = 1.0f / (pointCount - 1);
            for (int i = 0; i < pointCount; ++i)
                points[i] = CubicBezier(p0, p1, p2, p3, timeDelta * i);

            return points;
        }
        #endregion

        #region Coefficients

        #region Position
        public static void CubicBezierCoefs(
            float p0, float t0, float t1, float p1,
            out float third, out float second, out float first, out float zero)
        {
            third = -p0 + 3.0f * t0 - 3.0f * t1 + p1;
            second = 3.0f * p0 - 6.0f * t0 + 3.0f * t1;
            first = -3.0f * p0 + 3.0f * t0;
            zero = p0;
        }
        public static void CubicBezierCoefs(
            Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1,
            out Vector2 third, out Vector2 second, out Vector2 first, out Vector2 zero)
        {
            third = -p0 + 3.0f * t0 - 3.0f * t1 + p1;
            second = 3.0f * p0 - 6.0f * t0 + 3.0f * t1;
            first = -3.0f * p0 + 3.0f * t0;
            zero = p0;
        }
        public static void CubicBezierCoefs(
            Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1,
            out Vector3 third, out Vector3 second, out Vector3 first, out Vector3 zero)
        {
            third = -p0 + 3.0f * t0 - 3.0f * t1 + p1;
            second = 3.0f * p0 - 6.0f * t0 + 3.0f * t1;
            first = -3.0f * p0 + 3.0f * t0;
            zero = p0;
        }
        public static void CubicBezierCoefs(
            Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1,
            out Vector4 third, out Vector4 second, out Vector4 first, out Vector4 zero)
        {
            third = -p0 + 3.0f * t0 - 3.0f * t1 + p1;
            second = 3.0f * p0 - 6.0f * t0 + 3.0f * t1;
            first = -3.0f * p0 + 3.0f * t0;
            zero = p0;
        }
        #endregion

        #region Velocity
        public static void CubicBezierVelocityCoefs(
            float p0, float t0, float t1, float p1,
            out float second, out float first, out float zero)
        {
            second = 3.0f * (-p0 + 3.0f * t0 - 3.0f * t1 + p1);
            first = 2.0f * (3.0f * p0 - 6.0f * t0 + 3.0f * t1);
            zero = -3.0f * p0 + 3.0f * t0;
        }
        public static void CubicBezierVelocityCoefs(
            Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1,
            out Vector2 second, out Vector2 first, out Vector2 zero)
        {
            second = 3.0f * (-p0 + 3.0f * t0 - 3.0f * t1 + p1);
            first = 2.0f * (3.0f * p0 - 6.0f * t0 + 3.0f * t1);
            zero = -3.0f * p0 + 3.0f * t0;
        }
        public static void CubicBezierVelocityCoefs(
            Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1,
            out Vector3 second, out Vector3 first, out Vector3 zero)
        {
            second = 3.0f * (-p0 + 3.0f * t0 - 3.0f * t1 + p1);
            first = 2.0f * (3.0f * p0 - 6.0f * t0 + 3.0f * t1);
            zero = -3.0f * p0 + 3.0f * t0;
        }
        public static void CubicBezierVelocityCoefs(
            Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1,
            out Vector4 second, out Vector4 first, out Vector4 zero)
        {
            second = 3.0f * (-p0 + 3.0f * t0 - 3.0f * t1 + p1);
            first = 2.0f * (3.0f * p0 - 6.0f * t0 + 3.0f * t1);
            zero = -3.0f * p0 + 3.0f * t0;
        }
        #endregion

        #region Acceleration
        public static void CubicBezierAccelerationCoefs(
           float p0, float t0, float t1, float p1,
           out float first, out float zero)
        {
            first = 6.0f * (-p0 + 3.0f * t0 - 3.0f * t1 + p1);
            zero = 2.0f * (3.0f * p0 - 6.0f * t0 + 3.0f * t1);
        }
        public static void CubicBezierAccelerationCoefs(
           Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1,
           out Vector2 first, out Vector2 zero)
        {
            first = 6.0f * (-p0 + 3.0f * t0 - 3.0f * t1 + p1);
            zero = 2.0f * (3.0f * p0 - 6.0f * t0 + 3.0f * t1);
        }
        public static void CubicBezierAccelerationCoefs(
           Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1,
           out Vector3 first, out Vector3 zero)
        {
            first = 6.0f * (-p0 + 3.0f * t0 - 3.0f * t1 + p1);
            zero = 2.0f * (3.0f * p0 - 6.0f * t0 + 3.0f * t1);
        }
        public static void CubicBezierAccelerationCoefs(
           Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1,
           out Vector4 first, out Vector4 zero)
        {
            first = 6.0f * (-p0 + 3.0f * t0 - 3.0f * t1 + p1);
            zero = 2.0f * (3.0f * p0 - 6.0f * t0 + 3.0f * t1);
        }
        #endregion

        #endregion

        #region Position
        public static float CubicBezier(float p0, float t0, float t1, float p1, float time)
        {
            CubicBezierCoefs(p0, t0, t1, p1, out float third, out float second, out float first, out float zero);
            return EvaluatePolynomial(third, second, first, zero, time);
        }
        public static Vector2 CubicBezier(Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1, float time)
        {
            CubicBezierCoefs(p0, t0, t1, p1, out Vector2 third, out Vector2 second, out Vector2 first, out Vector2 zero);
            return EvaluatePolynomial(third, second, first, zero, time);
        }
        public static Vector3 CubicBezier(Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1, float time)
        {
            CubicBezierCoefs(p0, t0, t1, p1, out Vector3 third, out Vector3 second, out Vector3 first, out Vector3 zero);
            return EvaluatePolynomial(third, second, first, zero, time);
        }
        public static Vector4 CubicBezier(Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1, float time)
        {
            CubicBezierCoefs(p0, t0, t1, p1, out Vector4 third, out Vector4 second, out Vector4 first, out Vector4 zero);
            return EvaluatePolynomial(third, second, first, zero, time);
        }
        #endregion

        #region Velocity
        public static float CubicBezierVelocity(float p0, float t0, float t1, float p1, float time)
        {
            CubicBezierVelocityCoefs(p0, t0, t1, p1, out float second, out float first, out float zero);
            return EvaluatePolynomial(second, first, zero, time);
        }
        public static Vector2 CubicBezierVelocity(Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1, float time)
        {
            CubicBezierVelocityCoefs(p0, t0, t1, p1, out Vector2 second, out Vector2 first, out Vector2 zero);
            return EvaluatePolynomial(second, first, zero, time);
        }
        public static Vector3 CubicBezierVelocity(Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1, float time)
        {
            CubicBezierVelocityCoefs(p0, t0, t1, p1, out Vector3 second, out Vector3 first, out Vector3 zero);
            return EvaluatePolynomial(second, first, zero, time);
        }
        public static Vector4 CubicBezierVelocity(Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1, float time)
        {
            CubicBezierVelocityCoefs(p0, t0, t1, p1, out Vector4 second, out Vector4 first, out Vector4 zero);
            return EvaluatePolynomial(second, first, zero, time);
        }
        #endregion

        #region Acceleration
        public static float CubicBezierAcceleration(float p0, float t0, float t1, float p1, float time)
        {
            CubicBezierAccelerationCoefs(p0, t0, t1, p1, out float first, out float zero);
            return EvaluatePolynomial(first, zero, time);
        }
        public static Vector2 CubicBezierAcceleration(Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1, float time)
        {
            CubicBezierAccelerationCoefs(p0, t0, t1, p1, out Vector2 first, out Vector2 zero);
            return EvaluatePolynomial(first, zero, time);
        }
        public static Vector3 CubicBezierAcceleration(Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1, float time)
        {
            CubicBezierAccelerationCoefs(p0, t0, t1, p1, out Vector3 first, out Vector3 zero);
            return EvaluatePolynomial(first, zero, time);
        }
        public static Vector4 CubicBezierAcceleration(Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1, float time)
        {
            CubicBezierAccelerationCoefs(p0, t0, t1, p1, out Vector4 first, out Vector4 zero);
            return EvaluatePolynomial(first, zero, time);
        }
        #endregion

        #endregion

        #region Hermite

        #region Coefficients

        #region Position
        public static void CubicHermiteCoefs(
            float p0, float t0, float t1, float p1,
            out float third, out float second, out float first, out float zero)
        {
            third = (2.0f * p0 + t0 - 2.0f * p1 + t1);
            second = (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
            first = t0;
            zero = p0;
        }
        public static void CubicHermiteCoefs(
            Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1,
            out Vector2 third, out Vector2 second, out Vector2 first, out Vector2 zero)
        {
            third = (2.0f * p0 + t0 - 2.0f * p1 + t1);
            second = (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
            first = t0;
            zero = p0;
        }
        public static void CubicHermiteCoefs(
            Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1,
            out Vector3 third, out Vector3 second, out Vector3 first, out Vector3 zero)
        {
            third = (2.0f * p0 + t0 - 2.0f * p1 + t1);
            second = (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
            first = t0;
            zero = p0;
        }
        public static void CubicHermiteCoefs(
            Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1,
            out Vector4 third, out Vector4 second, out Vector4 first, out Vector4 zero)
        {
            third = (2.0f * p0 + t0 - 2.0f * p1 + t1);
            second = (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
            first = t0;
            zero = p0;
        }
        #endregion

        #region Velocity
        public static void CubicHermiteVelocityCoefs(
           float p0, float t0, float t1, float p1,
           out float second, out float first, out float zero)
        {
            second = 3.0f * (2.0f * p0 + t0 - 2.0f * p1 + t1);
            first = 2.0f * (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
            zero = t0;
        }
        public static void CubicHermiteVelocityCoefs(
           Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1,
           out Vector2 second, out Vector2 first, out Vector2 zero)
        {
            second = 3.0f * (2.0f * p0 + t0 - 2.0f * p1 + t1);
            first = 2.0f * (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
            zero = t0;
        }
        public static void CubicHermiteVelocityCoefs(
           Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1,
           out Vector3 second, out Vector3 first, out Vector3 zero)
        {
            second = 3.0f * (2.0f * p0 + t0 - 2.0f * p1 + t1);
            first = 2.0f * (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
            zero = t0;
        }
        public static void CubicHermiteVelocityCoefs(
           Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1,
           out Vector4 second, out Vector4 first, out Vector4 zero)
        {
            second = 3.0f * (2.0f * p0 + t0 - 2.0f * p1 + t1);
            first = 2.0f * (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
            zero = t0;
        }
        #endregion

        #region Acceleration
        public static void CubicHermiteAccelerationCoefs(
           float p0, float t0, float t1, float p1,
           out float first, out float zero)
        {
            first = 6.0f * (2.0f * p0 + t0 - 2.0f * p1 + t1);
            zero = 2.0f * (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
        }
        public static void CubicHermiteAccelerationCoefs(
           Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1,
           out Vector2 first, out Vector2 zero)
        {
            first = 6.0f * (2.0f * p0 + t0 - 2.0f * p1 + t1);
            zero = 2.0f * (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
        }
        public static void CubicHermiteAccelerationCoefs(
           Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1,
           out Vector3 first, out Vector3 zero)
        {
            first = 6.0f * (2.0f * p0 + t0 - 2.0f * p1 + t1);
            zero = 2.0f * (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
        }
        public static void CubicHermiteAccelerationCoefs(
           Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1,
           out Vector4 first, out Vector4 zero)
        {
            first = 6.0f * (2.0f * p0 + t0 - 2.0f * p1 + t1);
            zero = 2.0f * (-3.0f * p0 - 2.0f * t0 + 3.0f * p1 - t1);
        }
        #endregion

        #endregion

        #region Position
        public static float CubicHermite(float p0, float t0, float t1, float p1, float time)
        {
            CubicHermiteCoefs(p0, t0, t1, p1, out float third, out float second, out float first, out float zero);
            return EvaluatePolynomial(third, second, first, zero, time);
        }
        public static Vector2 CubicHermite(Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1, float time)
        {
            CubicHermiteCoefs(p0, t0, t1, p1, out Vector2 third, out Vector2 second, out Vector2 first, out Vector2 zero);
            return EvaluatePolynomial(third, second, first, zero, time);
        }
        public static Vector3 CubicHermite(Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1, float time)
        {
            CubicHermiteCoefs(p0, t0, t1, p1, out Vector3 third, out Vector3 second, out Vector3 first, out Vector3 zero);
            return EvaluatePolynomial(third, second, first, zero, time);
        }
        public static Vector4 CubicHermite(Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1, float time)
        {
            CubicHermiteCoefs(p0, t0, t1, p1, out Vector4 third, out Vector4 second, out Vector4 first, out Vector4 zero);
            return EvaluatePolynomial(third, second, first, zero, time);
        }
        #endregion

        #region Velocity
        public static float CubicHermiteVelocity(float p0, float t0, float t1, float p1, float time)
        {
            CubicHermiteVelocityCoefs(p0, t0, t1, p1, out float second, out float first, out float zero);
            return EvaluatePolynomial(second, first, zero, time);
        }
        public static Vector2 CubicHermiteVelocity(Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1, float time)
        {
            CubicHermiteVelocityCoefs(p0, t0, t1, p1, out Vector2 second, out Vector2 first, out Vector2 zero);
            return EvaluatePolynomial(second, first, zero, time);
        }
        public static Vector3 CubicHermiteVelocity(Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1, float time)
        {
            CubicHermiteVelocityCoefs(p0, t0, t1, p1, out Vector3 second, out Vector3 first, out Vector3 zero);
            return EvaluatePolynomial(second, first, zero, time);
        }
        public static Vector4 CubicHermiteVelocity(Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1, float time)
        {
            CubicHermiteVelocityCoefs(p0, t0, t1, p1, out Vector4 second, out Vector4 first, out Vector4 zero);
            return EvaluatePolynomial(second, first, zero, time);
        }
        #endregion

        #region Acceleration
        public static float CubicHermiteAcceleration(float p0, float t0, float t1, float p1, float time)
        {
            CubicHermiteAccelerationCoefs(p0, t0, t1, p1, out float first, out float zero);
            return EvaluatePolynomial(first, zero, time);
        }
        public static Vector2 CubicHermiteAcceleration(Vector2 p0, Vector2 t0, Vector2 t1, Vector2 p1, float time)
        {
            CubicHermiteAccelerationCoefs(p0, t0, t1, p1, out Vector2 first, out Vector2 zero);
            return EvaluatePolynomial(first, zero, time);
        }
        public static Vector3 CubicHermiteAcceleration(Vector3 p0, Vector3 t0, Vector3 t1, Vector3 p1, float time)
        {
            CubicHermiteAccelerationCoefs(p0, t0, t1, p1, out Vector3 first, out Vector3 zero);
            return EvaluatePolynomial(first, zero, time);
        }
        public static Vector4 CubicHermiteAcceleration(Vector4 p0, Vector4 t0, Vector4 t1, Vector4 p1, float time)
        {
            CubicHermiteAccelerationCoefs(p0, t0, t1, p1, out Vector4 first, out Vector4 zero);
            return EvaluatePolynomial(first, zero, time);
        }
        #endregion

        #endregion

        #region Lerp

        public static float Lerp(float start, float end, float time)
            => start + (end - start) * time;
        public static float Lerp(float start, float end, float time, float speed)
            => Lerp(start, end, time * speed);

        public static Vector2 Lerp(Vector2 start, Vector2 end, float time)
            => start + (end - start) * time;
        public static Vector2 Lerp(Vector2 start, Vector2 end, float time, float speed)
            => Lerp(start, end, time * speed);

        public static Vector3 Lerp(Vector3 start, Vector3 end, float time)
            => start + (end - start) * time;
        public static Vector3 Lerp(Vector3 start, Vector3 end, float time, float speed)
            => Lerp(start, end, time * speed);

        public static Vector4 Lerp(Vector4 start, Vector4 end, float time)
            => start + (end - start) * time;
        public static Vector4 Lerp(Vector4 start, Vector4 end, float time, float speed)
            => Lerp(start, end, time * speed);

        public static Vector2 Lerp(Vector2 start, Vector2 end, Vector2 time)
            => start + (end - start) * time;
        public static Vector2 Lerp(Vector2 start, Vector2 end, Vector2 time, float speed)
            => Lerp(start, end, time * speed);

        public static Vector3 Lerp(Vector3 start, Vector3 end, Vector3 time)
            => start + (end - start) * time;
        public static Vector3 Lerp(Vector3 start, Vector3 end, Vector3 time, float speed)
            => Lerp(start, end, time * speed);

        public static Vector4 Lerp(Vector4 start, Vector4 end, Vector4 time)
            => start + (end - start) * time;
        public static Vector4 Lerp(Vector4 start, Vector4 end, Vector4 time, float speed)
            => Lerp(start, end, time * speed);

        public static Quaternion Slerp(Quaternion q1, Quaternion q2, float blend)
        {
            // if either input is zero, return the other.
            if (q1.LengthSquared() == 0.0f)
            {
                if (q2.LengthSquared() == 0.0f)
                    return Quaternion.Identity;

                return q2;
            }
            else if (q2.LengthSquared() == 0.0f)
                return q1;

            float cosHalfAngle = Quaternion.Dot(q1, q2);
            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
            {
                // angle = 0.0f, so just return one input.
                return q1;
            }
            else if (cosHalfAngle < 0.0f)
            {
                q2 = -q2;
                cosHalfAngle = -cosHalfAngle;
            }

            float blendA;
            float blendB;
            if (cosHalfAngle < 0.99f)
            {
                // do proper slerp for big angles
                float halfAngle = (float)Acos(cosHalfAngle);
                float sinHalfAngle = (float)Sin(halfAngle);
                float oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = (float)Sin(halfAngle * (1.0f - blend)) * oneOverSinHalfAngle;
                blendB = (float)Sin(halfAngle * blend) * oneOverSinHalfAngle;
            }
            else
            {
                // do lerp if angle is really small.
                blendA = 1.0f - blend;
                blendB = blend;
            }

            Quaternion result = q1 * blendA + q2 * blendB;

            return result.LengthSquared() > 0.0f ? Quaternion.Normalize(result) : Quaternion.Identity;
        }

        #endregion

        #region Time Modifiers

        /// <summary>
        /// Maps a linear time value from 0.0f to 1.0f to a time value that bounces back a specified amount of times after hitting 1.0f.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="bounces"></param>
        /// <param name="bounceFalloff"></param>
        /// <returns></returns>
        public static float BounceTimeModifier(float time, int bounces, float bounceFalloff)
            => 1.0f - (Pow(E, -bounceFalloff * time) * Abs(Cos(PI * (0.5f + bounces) * time)));

        /// <summary>
        /// Maps a linear time value from 0.0f to 1.0f to a cosine time value that eases in and out.
        /// </summary>
        public static float CosineTimeModifier(float time)
            => (1.0f - (float)Cos(time * PI)) * 0.5f;

        #endregion

        #region Special Interps

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <param name="speed"></param>
        /// <param name="exponent"></param>
        /// <returns></returns>
        public static float QuadraticEaseEnd(float start, float end, float time, float speed = 1.0f, float exponent = 2.0f)
            => Lerp(start, end, 1.0f - (float)Pow(1.0f - (time * speed), exponent));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <param name="speed"></param>
        /// <param name="exponent"></param>
        /// <returns></returns>
        public static float QuadraticEaseStart(float start, float end, float time, float speed = 1.0f, float exponent = 2.0f)
            => Lerp(start, end, (float)Pow(time * speed, exponent));

        /// <summary>
        /// Smoothed interpolation between two points. Eases in and out.
        /// </summary>
        public static float Cosine(float start, float end, float time, float speed = 1.0f)
            => Lerp(start, end, CosineTimeModifier(time * speed));

        /// <summary>
        /// Smoothed interpolation between two points. Eases in and out.
        /// </summary>
        public static Vector2 Cosine(Vector2 start, Vector2 end, float time, float speed = 1.0f)
            => Lerp(start, end, CosineTimeModifier(time * speed));

        /// <summary>
        /// Smoothed interpolation between two points. Eases in and out.
        /// </summary>
        public static Vector3 CosineTo(Vector3 start, Vector3 end, float time, float speed = 1.0f)
            => Lerp(start, end, CosineTimeModifier(time * speed));

        /// <summary>
        /// Smoothed interpolation between two points. Eases in and out.
        /// </summary>
        public static Vector4 CosineTo(Vector4 start, Vector4 end, float time, float speed = 1.0f)
            => Lerp(start, end, CosineTimeModifier(time * speed));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <param name="deltaTime"></param>
        /// <param name="radPerSec"></param>
        /// <returns></returns>
        public static Vector3 VInterpNormalRotationTo(Vector3 current, Vector3 target, float deltaTime, float radPerSec, float rotationThreshold = 0.01f)
        {
            XRMath.AxisAngleBetween(current, target, out Vector3 axis, out float totalRads);
            if (totalRads < rotationThreshold)
                return target;

            float deltaRads = radPerSec * deltaTime;
            if (deltaRads > totalRads)
                deltaRads = totalRads;

            return System.Numerics.Vector3.Transform(current, Quaternion.CreateFromAxisAngle(axis, deltaRads));
        }

        #endregion

        #region Misc Lerps

        public static Quaternion SCubic(Quaternion p1, Quaternion p2, Quaternion p3, Quaternion p4, float time)
        {
            Quaternion q1 = Slerp(p1, p2, time);
            Quaternion q2 = Slerp(p2, p3, time);
            Quaternion q3 = Slerp(p3, p4, time);
            return Squad(q1, q2, q3, time);
        }
        public static Quaternion Squad(Quaternion q1, Quaternion q2, Quaternion q3, float time)
        {
            Quaternion r1 = Slerp(q1, q2, time);
            Quaternion r2 = Slerp(q2, q3, time);
            return Slerp(r1, r2, time);
        }

        public static Color Lerp(
            Color startColor,
            Color endColor,
            float time)
        {
            time = time.Clamp(0.0f, 1.0f);
            Color color = Color.FromArgb(
                (byte)(startColor.A + ((float)endColor.A - startColor.A) * time + 0.5f),
                (byte)(startColor.R + ((float)endColor.R - startColor.R) * time + 0.5f),
                (byte)(startColor.G + ((float)endColor.G - startColor.G) * time + 0.5f),
                (byte)(startColor.B + ((float)endColor.B - startColor.B) * time + 0.5f));
            return color;
        }
        public static Point Lerp(
            Point start,
            Point end,
            float time)
        {
            time = time.Clamp(0.0f, 1.0f);
            return new Point(
                (int)(start.X + ((float)end.X - start.X) * time + 0.5f),
                (int)(start.Y + ((float)end.Y - start.Y) * time + 0.5f));
        }
        public static PointF Lerp(
            PointF start,
            PointF end,
            float time)
        {
            time = time.Clamp(0.0f, 1.0f);
            return new PointF(
                start.X + (end.X - start.X) * time,
                start.Y + (end.Y - start.Y) * time);
        }
        #endregion

        #region Generate Samples

        public static float[] GenerateSamplesNormalized(int count, Func<float, float> normalizedTimeGenerator)
            => GenerateSamples(0.0f, 1.0f, count, normalizedTimeGenerator);
        public static float[] GenerateSamples(float start, float end, int count, Func<float, float> generator)
        {
            float[] samples = new float[count];
            float step = (end - start) / (count - 1);
            for (int i = 0; i < count; ++i)
                samples[i] = generator(start + step * i);
            return samples;
        }

        public static Vector2[] GenerateSamplesNormalized(int count, Func<float, Vector2> normalizedTimeGenerator)
            => GenerateSamples(0.0f, 1.0f, count, normalizedTimeGenerator);
        public static Vector2[] GenerateSamples(float start, float end, int count, Func<float, Vector2> generator)
        {
            Vector2[] samples = new Vector2[count];
            float step = (end - start) / (count - 1);
            for (int i = 0; i < count; ++i)
                samples[i] = generator(start + step * i);
            return samples;
        }

        public static Vector3[] GenerateSamplesNormalized(int count, Func<float, Vector3> normalizedTimeGenerator)
            => GenerateSamples(0.0f, 1.0f, count, normalizedTimeGenerator);
        public static Vector3[] GenerateSamples(float start, float end, int count, Func<float, Vector3> generator)
        {
            Vector3[] samples = new Vector3[count];
            float step = (end - start) / (count - 1);
            for (int i = 0; i < count; ++i)
                samples[i] = generator(start + step * i);
            return samples;
        }

        public static Vector4[] GenerateSamplesNormalized(int count, Func<float, Vector4> normalizedTimeGenerator)
            => GenerateSamples(0.0f, 1.0f, count, normalizedTimeGenerator);
        public static Vector4[] GenerateSamples(float startTime, float endTime, int count, Func<float, Vector4> generator)
        {
            Vector4[] samples = new Vector4[count];
            float step = (endTime - startTime) / (count - 1);
            for (int i = 0; i < count; ++i)
                samples[i] = generator(startTime + step * i);
            return samples;
        }

        #endregion

        public static float Float(float t, EFloatInterpolationMode mode) => mode switch
        {
            EFloatInterpolationMode.Linear => Linear(t, 0, 1),
            EFloatInterpolationMode.InOutCubic => InOutCubic(t, 0, 1),
            EFloatInterpolationMode.InOutQuintic => InOutQuintic(t, 0, 1),
            EFloatInterpolationMode.InQuintic => InQuintic(t, 0, 1),
            EFloatInterpolationMode.InQuartic => InQuartic(t, 0, 1),
            EFloatInterpolationMode.InCubic => InCubic(t, 0, 1),
            EFloatInterpolationMode.InQuadratic => InQuadratic(t, 0, 1),
            EFloatInterpolationMode.OutQuintic => OutQuintic(t, 0, 1),
            EFloatInterpolationMode.OutQuartic => OutQuartic(t, 0, 1),
            EFloatInterpolationMode.OutCubic => OutCubic(t, 0, 1),
            EFloatInterpolationMode.OutInCubic => OutInCubic(t, 0, 1),
            EFloatInterpolationMode.OutInQuartic => OutInQuartic(t, 0, 1),
            EFloatInterpolationMode.BackInCubic => BackInCubic(t, 0, 1),
            EFloatInterpolationMode.BackInQuartic => BackInQuartic(t, 0, 1),
            EFloatInterpolationMode.OutBackCubic => OutBackCubic(t, 0, 1),
            EFloatInterpolationMode.OutBackQuartic => OutBackQuartic(t, 0, 1),
            EFloatInterpolationMode.OutElasticSmall => OutElasticSmall(t, 0, 1),
            EFloatInterpolationMode.OutElasticBig => OutElasticBig(t, 0, 1),
            EFloatInterpolationMode.InElasticSmall => InElasticSmall(t, 0, 1),
            EFloatInterpolationMode.InElasticBig => InElasticBig(t, 0, 1),
            EFloatInterpolationMode.InSine => InSine(t, 0, 1),
            EFloatInterpolationMode.OutSine => OutSine(t, 0, 1),
            EFloatInterpolationMode.InOutSine => InOutSine(t, 0, 1),
            EFloatInterpolationMode.InElastic => InElastic(t, 0, 1),
            EFloatInterpolationMode.OutElastic => OutElastic(t, 0, 1),
            EFloatInterpolationMode.InBack => InBack(t, 0, 1),
            EFloatInterpolationMode.OutBack => OutBack(t, 0, 1),
            _ => 0,
        };

        public static Vector2 Vector2(Vector2 start, Vector2 end, float t, EFloatInterpolationMode mode)
        {
            float interpT = Float(t, mode);
            return ((1 - interpT) * start) + (interpT * end);
        }

        public static Vector3 Vector3(Vector3 start, Vector3 end, float t, EFloatInterpolationMode mode)
        {
            float interpT = Float(t, mode);
            return ((1 - interpT) * start) + (interpT * end);
        }

        public static Vector4 Vector4(Vector4 start, Vector4 end, float t, EFloatInterpolationMode mode)
        {
            float interpT = Float(t, mode);
            return ((1 - interpT) * start) + (interpT * end);
        }

        public static float LerpDistance(float value, float target, float increaseSpeed, float decreaseSpeed, float dt)
            => value == target
                ? target
                : value < target
                    ? (value + dt * increaseSpeed).Clamp(float.NegativeInfinity, target)
                    : (value - dt * decreaseSpeed).Clamp(target, float.PositiveInfinity);

        private static float Linear(float t, float start, float end)
            => start + end * t;

        private static void GetTimes(float t, out float t2, out float t3)
        {
            t2 = t * t;
            t3 = t2 * t;
        }

        private static float InOutCubic(float t, float b, float c)
        {
            GetTimes(t, out float t2, out float t3);
            return b + c * (-2 * t3 + 3 * t2);
        }

        private static float InOutQuintic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (6 * tc * ts + -15 * ts * ts + 10 * tc);
        }

        private static float InQuintic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (tc * ts);
        }

        private static float InQuartic(float t, float b, float c)
            => b + c * (t * t * t * t);
        private static float InCubic(float t, float b, float c)
            => b + c * (t * t * t);
        private static float InQuadratic(float t, float b, float c)
            => b + c * (t * t);

        private static float OutQuintic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (tc * ts + -5 * ts * ts + 10 * tc + -10 * ts + 5 * t);
        }
        private static float OutQuartic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (-1 * ts * ts + 4 * tc + -6 * ts + 4 * t);
        }
        private static float OutCubic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (tc + -3 * ts + 3 * t);
        }

        private static float OutInCubic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (4 * tc + -6 * ts + 3 * t);
        }

        private static float OutInQuartic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (6 * tc + -9 * ts + 4 * t);
        }

        private static float BackInCubic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (4 * tc + -3 * ts);
        }

        private static float BackInQuartic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (2 * ts * ts + 2 * tc + -3 * ts);
        }

        private static float OutBackCubic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (4 * tc + -9 * ts + 6 * t);
        }

        private static float OutBackQuartic(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (-2 * ts * ts + 10 * tc + -15 * ts + 8 * t);
        }

        private static float OutElasticSmall(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (33 * tc * ts + -106 * ts * ts + 126 * tc + -67 * ts + 15 * t);
        }

        private static float OutElasticBig(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (56 * tc * ts + -175 * ts * ts + 200 * tc + -100 * ts + 20 * t);
        }

        private static float InElasticSmall(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (33 * tc * ts + -59 * ts * ts + 32 * tc + -5 * ts);
        }

        private static float InElasticBig(float t, float b, float c)
        {
            GetTimes(t, out float ts, out float tc);
            return b + c * (56 * tc * ts + -105 * ts * ts + 60 * tc + -10 * ts);
        }

        private static float InSine(float t, float b, float c)
        {
            c -= b;
            return -c * Cos(t / 1 * (PI / 2)) + c + b;
        }

        private static float OutSine(float t, float b, float c)
        {
            c -= b;
            return c * Sin(t / 1 * (PI / 2)) + b;
        }

        private static float InOutSine(float t, float b, float c)
        {
            c -= b;
            return -c / 2 * (Cos(PI * t / 1) - 1) + b;
        }

        private static float InElastic(float t, float b, float c)
        {
            c -= b;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (t == 0)
                return b;
            if ((t /= d) == 1)
                return b + c;

            if (a == 0f || a < Abs(c))
            {
                a = c;
                s = p / 4;
            }
            else
            {
                s = p / (2 * PI) * Asin(c / a);
            }
            return -(a * Pow(2, 10 * (t -= 1)) * Sin((t * d - s) * (2 * PI) / p)) + b;
        }
        private static float OutElastic(float t, float b, float c)
        {
            c -= b;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (t == 0)
                return b;
            if ((t /= d) == 1)
                return b + c;

            if (a == 0f || a < Abs(c))
            {
                a = c;
                s = p / 4;
            }
            else
            {
                s = p / (2 * PI) * Asin(c / a);
            }
            return (a * Pow(2, -10 * t) * Sin((t * d - s) * (2 * PI) / p) + c + b);
        }

        private static float InBack(float t, float b, float c)
        {
            c -= b;
            t /= 1;
            float s = 1.70158f;
            return c * (t) * t * ((s + 1) * t - s) + b;
        }

        private static float OutBack(float t, float b, float c)
        {
            float s = 1.70158f;
            c -= b;
            t = (t / 1) - 1;
            return c * ((t) * t * ((s + 1) * t + s) + 1) + b;
        }

        public static float WeightedLerp(float f1, float f2, float f3, float w1, float w2, float w3)
        {
            float total = w1 + w2 + w3;
            return (f1 * w1 + f2 * w2 + f3 * w3) / total;
        }
        public static Vector2 WeightedLerp(Vector2 f1, Vector2 f2, Vector2 f3, float w1, float w2, float w3)
        {
            float total = w1 + w2 + w3;
            return (f1 * w1 + f2 * w2 + f3 * w3) / total;
        }
        public static Vector3 WeightedLerp(Vector3 f1, Vector3 f2, Vector3 f3, float w1, float w2, float w3)
        {
            float total = w1 + w2 + w3;
            return (f1 * w1 + f2 * w2 + f3 * w3) / total;
        }
        public static Vector4 WeightedLerp(Vector4 f1, Vector4 f2, Vector4 f3, float w1, float w2, float w3)
        {
            float total = w1 + w2 + w3;
            return (f1 * w1 + f2 * w2 + f3 * w3) / total;
        }
        public static Quaternion WeightedSlerp(Quaternion f1, Quaternion f2, Quaternion f3, float w1, float w2, float w3)
        {
            Quaternion q1 = f1 * w1;
            Quaternion q2 = f2 * w2;
            Quaternion q3 = f3 * w3;
            Quaternion result = q1 + q2 + q3;
            if (result.LengthSquared() > 0.0f)
                return Quaternion.Normalize(result);
            else
                return Quaternion.Identity;
        }

        public static float WeightedLerp(float f1, float f2, float f3, float f4, float w1, float w2, float w3, float w4)
        {
            float total = w1 + w2 + w3 + w4;
            return (f1 * w1 + f2 * w2 + f3 * w3 + f4 * w4) / total;
        }
        public static Vector2 WeightedLerp(Vector2 f1, Vector2 f2, Vector2 f3, Vector2 f4, float w1, float w2, float w3, float w4)
        {
            float total = w1 + w2 + w3 + w4;
            return (f1 * w1 + f2 * w2 + f3 * w3 + f4 * w4) / total;
        }
        public static Vector3 WeightedLerp(Vector3 f1, Vector3 f2, Vector3 f3, Vector3 f4, float w1, float w2, float w3, float w4)
        {
            float total = w1 + w2 + w3 + w4;
            return (f1 * w1 + f2 * w2 + f3 * w3 + f4 * w4) / total;
        }
        public static Vector4 WeightedLerp(Vector4 f1, Vector4 f2, Vector4 f3, Vector4 f4, float w1, float w2, float w3, float w4)
        {
            float total = w1 + w2 + w3 + w4;
            return (f1 * w1 + f2 * w2 + f3 * w3 + f4 * w4) / total;
        }
        public static Quaternion WeightedSlerp(Quaternion f1, Quaternion f2, Quaternion f3, Quaternion f4, float w1, float w2, float w3, float w4)
        {
            Quaternion q1 = f1 * w1;
            Quaternion q2 = f2 * w2;
            Quaternion q3 = f3 * w3;
            Quaternion q4 = f4 * w4;
            Quaternion result = q1 + q2 + q3 + q4;
            if (result.LengthSquared() > 0.0f)
                return Quaternion.Normalize(result);
            else
                return Quaternion.Identity;
        }
    }
    public enum EFloatInterpolationMode
    {
        Linear,
        InOutCubic,
        InOutQuintic,
        InQuintic,
        InQuartic,
        InCubic,
        InQuadratic,
        OutQuintic,
        OutQuartic,
        OutCubic,
        OutInCubic,
        OutInQuartic,
        BackInCubic,
        BackInQuartic,
        OutBackCubic,
        OutBackQuartic,
        OutElasticSmall,
        OutElasticBig,
        InElasticSmall,
        InElasticBig,
        InSine,
        OutSine,
        InOutSine,
        InElastic,
        OutElastic,
        InBack,
        OutBack
    }
}
