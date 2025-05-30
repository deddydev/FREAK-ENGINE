﻿using Extensions;
using System.Numerics;
using XREngine.Data.Core;

namespace XREngine.Components.Animation
{
    public partial class IKSolverVR
    {
        [Serializable]
        public class VirtualBone : XRBase
        {
            private Vector3 _readPosition;
            public Vector3 ReadPosition
            {
                get => _readPosition;
                set => _readPosition = value;
            }

            private Quaternion _readRotation;
            public Quaternion ReadRotation
            {
                get => _readRotation;
                set => _readRotation = value;
            }

            private Vector3 _solverPosition;
            public Vector3 SolverPosition
            {
                get => _solverPosition;
                set => _solverPosition = value;
            }

            private Quaternion _solverRotation;
            public Quaternion SolverRotation
            {
                get => _solverRotation;
                set => _solverRotation = value;
            }

            private float _length;
            public float Length
            {
                get => _length;
                set => _length = value;
            }

            private float _lengthSquared;
            public float LengthSquared
            {
                get => _lengthSquared;
                set => _lengthSquared = value;
            }

            private Vector3 _axis;
            public Vector3 Axis
            {
                get => _axis;
                set => _axis = value;
            }

            public VirtualBone(Vector3 position, Quaternion rotation)
                => Read(position, rotation);

            public void Read(Vector3 position, Quaternion rotation)
            {
                _readPosition = position;
                _readRotation = rotation;
                SolverPosition = position;
                SolverRotation = rotation;
            }

            public static void SwingRotation(
                VirtualBone[] bones,
                int index,
                Vector3 swingTarget,
                float weight = 1.0f)
            {
                if (weight <= 0.0f)
                    return;

                Quaternion r = XRMath.RotationBetweenVectors(
                    bones[index].SolverRotation.Rotate(bones[index].Axis), 
                    swingTarget - bones[index].SolverPosition);

                if (weight < 1.0f)
                    r = Quaternion.Lerp(Quaternion.Identity, r, weight);

                for (int i = index; i < bones.Length; i++)
                    bones[i].SolverRotation = r * bones[i].SolverRotation;
            }

            // Calculates bone lengths and axes, returns the length of the entire chain
            public static float PreSolve(ref VirtualBone[] bones)
            {
                float length = 0;

                for (int i = 0; i < bones.Length; i++)
                {
                    if (i < bones.Length - 1)
                    {
                        bones[i]._lengthSquared = (bones[i + 1].SolverPosition - bones[i].SolverPosition).LengthSquared();
                        bones[i]._length = MathF.Sqrt(bones[i]._lengthSquared);
                        length += bones[i]._length;

                        bones[i].Axis = Quaternion.Inverse(bones[i].SolverRotation).Rotate(bones[i + 1].SolverPosition - bones[i].SolverPosition);
                    }
                    else
                    {
                        bones[i]._lengthSquared = 0.0f;
                        bones[i]._length = 0.0f;
                    }
                }

                return length;
            }

            public static void RotateAroundPoint(
                VirtualBone[] bones,
                int index,
                Vector3 point,
                Quaternion rotation)
            {
                for (int i = index; i < bones.Length; i++)
                {
                    if (bones[i] is null)
                        continue;
                    
                    Vector3 dir = bones[i].SolverPosition - point;
                    bones[i].SolverPosition = point + rotation.Rotate(dir);
                    bones[i].SolverRotation = rotation * bones[i].SolverRotation;
                }
            }

            public static void RotateBy(VirtualBone[] bones, int index, Quaternion rotation)
            {
                for (int i = index; i < bones.Length; i++)
                {
                    if (bones[i] is null)
                        continue;
                    
                    Vector3 dir = bones[i].SolverPosition - bones[index].SolverPosition;
                    bones[i].SolverPosition = bones[index].SolverPosition + rotation.Rotate(dir);
                    bones[i].SolverRotation = rotation * bones[i].SolverRotation;
                }
            }

            public static void RotateBy(VirtualBone[] bones, Quaternion rotation)
            {
                for (int i = 0; i < bones.Length; i++)
                {
                    if (bones[i] is null)
                        continue;
                    
                    if (i > 0)
                        bones[i].SolverPosition = bones[0].SolverPosition + rotation.Rotate(bones[i].SolverPosition - bones[0].SolverPosition);
                    
                    bones[i].SolverRotation = rotation * bones[i].SolverRotation;
                }
            }

            public static void RotateTo(VirtualBone[] bones, int index, Quaternion rotation)
            {
                Quaternion q = XRMath.FromToRotation(bones[index].SolverRotation, rotation);
                RotateAroundPoint(bones, index, bones[index].SolverPosition, q);
            }

            // TODO Move to IKSolverTrigonometric
            /// <summary>
            /// Solve the bone chain virtually using both solverPositions and SolverRotations. This will work the same as IKSolverTrigonometric.Solve.
            /// </summary>
            public static void SolveTrigonometric(VirtualBone[] bones, int first, int second, int third, Vector3 targetPosition, Vector3 bendNormal, float weight)
            {
                if (weight <= 0.0f)
                    return;

                // Direction of the limb in solver
                targetPosition = Vector3.Lerp(bones[third].SolverPosition, targetPosition, weight);

                Vector3 dir = targetPosition - bones[first].SolverPosition;

                // Distance between the first and the last transform solver positions
                float sqrMag = dir.LengthSquared();
                if (sqrMag == 0.0f)
                    return;
                float length = MathF.Sqrt(sqrMag);

                float sqrMag1 = (bones[second].SolverPosition - bones[first].SolverPosition).LengthSquared();
                float sqrMag2 = (bones[third].SolverPosition - bones[second].SolverPosition).LengthSquared();

                // Get the general world space bending direction
                Vector3 bendDir = Vector3.Cross(dir, bendNormal);

                // Get the direction to the trigonometrically solved position of the second transform
                Vector3 toBendPoint = GetDirectionToBendPoint(dir, length, bendDir, sqrMag1, sqrMag2);

                // Position the second transform
                Quaternion q1 = XRMath.RotationBetweenVectors(
                    bones[second].SolverPosition - bones[first].SolverPosition,
                    toBendPoint);

                if (weight < 1.0f)
                    q1 = Quaternion.Lerp(Quaternion.Identity, q1, weight);

                RotateAroundPoint(bones, first, bones[first].SolverPosition, q1);

                Quaternion q2 = XRMath.RotationBetweenVectors(
                    bones[third].SolverPosition - bones[second].SolverPosition,
                    targetPosition - bones[second].SolverPosition);

                if (weight < 1.0f)
                    q2 = Quaternion.Lerp(Quaternion.Identity, q2, weight);

                RotateAroundPoint(bones, second, bones[second].SolverPosition, q2);
            }

            //Calculates the bend direction based on the law of cosines. NB! Magnitude of the returned vector does not equal to the length of the first bone!
            private static Vector3 GetDirectionToBendPoint(Vector3 direction, float directionMag, Vector3 bendDirection, float sqrMag1, float sqrMag2)
            {
                if (direction == Vector3.Zero)
                    return Vector3.Zero;

                float x = ((directionMag * directionMag) + (sqrMag1 - sqrMag2)) / 2.0f / directionMag;
                return XRMath.LookRotation(direction, bendDirection).Rotate(new Vector3(
                        0.0f,
                        (float)Math.Sqrt((sqrMag1 - x * x).Clamp(0.0f, float.PositiveInfinity)),
                        x));
            }

            // TODO Move to IKSolverFABRIK
            // Solves a simple FABRIK pass for a bone hierarchy, not using rotation limits or singularity breaking here
            public static void SolveFABRIK(VirtualBone[] bones, Vector3 startPosition, Vector3 targetPosition, float weight, float minNormalizedTargetDistance, int iterations, float length, Vector3 startOffset)
            {
                if (weight <= 0.0f)
                    return;

                if (minNormalizedTargetDistance > 0.0f)
                {
                    Vector3 targetDirection = targetPosition - startPosition;
                    float targetLength = targetDirection.Length();
                    Vector3 tP = startPosition + (targetDirection / targetLength) * MathF.Max(length * minNormalizedTargetDistance, targetLength);
                    targetPosition = Vector3.Lerp(targetPosition, tP, weight);
                }

                // Iterating the solver
                for (int iteration = 0; iteration < iterations; iteration++)
                {
                    // Stage 1
                    bones[^1].SolverPosition = Vector3.Lerp(bones[^1].SolverPosition, targetPosition, weight);

                    // Finding joint positions
                    for (int i = bones.Length - 2; i > -1; i--)
                        bones[i].SolverPosition = SolveFABRIKJoint(bones[i].SolverPosition, bones[i + 1].SolverPosition, bones[i]._length);
                    
                    // Stage 2
                    if (iteration == 0)
                        foreach (VirtualBone bone in bones)
                            bone.SolverPosition += startOffset;
                    
                    bones[0].SolverPosition = startPosition;

                    for (int i = 1; i < bones.Length; i++)
                        bones[i].SolverPosition = SolveFABRIKJoint(bones[i].SolverPosition, bones[i - 1].SolverPosition, bones[i - 1]._length);
                }

                for (int i = 0; i < bones.Length - 1; i++)
                    SwingRotation(bones, i, bones[i + 1].SolverPosition);
            }

            // Solves a FABRIK joint between two bones.
            private static Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length)
                => pos2 + (pos1 - pos2).Normalized() * length;

            public static void SolveCCD(VirtualBone[] bones, Vector3 targetPosition, float weight, int iterations)
            {
                if (weight <= 0.0f)
                    return;

                // Iterating the solver
                for (int iteration = 0; iteration < iterations; iteration++)
                {
                    for (int i = bones.Length - 2; i > -1; i--)
                    {
                        Vector3 toLastBone = bones[^1].SolverPosition - bones[i].SolverPosition;
                        Vector3 toTarget = targetPosition - bones[i].SolverPosition;
                        Quaternion rotation = XRMath.RotationBetweenVectors(toLastBone, toTarget);
                        RotateBy(bones, i, weight >= 1 ? rotation : Quaternion.Lerp(Quaternion.Identity, rotation, weight));
                    }
                }
            }
        }
    }
}
