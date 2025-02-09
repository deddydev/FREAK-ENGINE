﻿using Extensions;
using System.Text;
using XREngine.Data.Rendering;
using XREngine.Rendering.Models.Materials;

namespace XREngine.Rendering.Shaders.Generator
{
    /// <summary>
    /// Generates a typical vertex shader for use with most models.
    /// </summary>
    public class DefaultVertexShaderGenerator(XRMesh mesh) : ShaderGeneratorBase(mesh)
    {
        //Buffers leaving the vertex shader for each vertex
        public const string FragPosLocalName = "FragPosLocal";
        public const string FragPosName = "FragPos";
        public const string FragNormName = "FragNorm";
        public const string FragTanName = "FragTan";
        public const string FragBinormName = "FragBinorm"; //Binormal is created in vertex shader if tangents exist
        public const string FragColorName = "FragColor{0}";
        public const string FragUVName = "FragUV{0}";

        private void WriteAdjointMethod()
        {
            Line("mat3 adjoint(mat4 m)");
            using (OpenBracketState())
            {
                Line("return mat3(");
                Line("  cross(m[1].xyz, m[2].xyz),");
                Line("  cross(m[2].xyz, m[0].xyz),");
                Line("  cross(m[0].xyz, m[1].xyz));");
            }
        }

        /// <summary>
        /// Creates the vertex shader to render a typical model.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="allowMeshMorphing"></param>
        /// <param name="useMorphMultiRig"></param>
        /// <param name="allowColorMorphing"></param>
        /// <returns></returns>
        public override string Generate()
        {
            WriteVersion();
            Line();
            WriteInputs();
            WriteAdjointMethod();
            using (StartMain())
            {
                //Create MVP matrix right away
                Line($"mat4 ViewMatrix = inverse({EEngineUniform.InverseViewMatrix});");
                Line($"mat4 mvMatrix = ViewMatrix * ModelMatrix;");
                Line($"mat4 mvpMatrix = {EEngineUniform.ProjMatrix} * mvMatrix;");
                Line($"mat4 vpMatrix = {EEngineUniform.ProjMatrix} * ViewMatrix;");
                if (Mesh.NormalsBuffer is not null)
                    Line("mat3 normalMatrix = adjoint(ModelMatrix);");
                Line();

                //Transform position, normals and tangents
                if (Mesh.HasSkinning)
                    WriteSkinnedMeshInputs();
                else
                    WriteStaticMeshInputs();

                WriteColorOutputs();
                WriteTexCoordOutputs();
            }
            return End();
        }

        private void WriteInputs()
        {
            //Write header in fields (from buffers)
            WriteBuffers();
            Line();

            //Write header uniforms
            WriteBufferBlocks();
            Line();

            //Write single uniforms
            WriteUniforms();
            Line();

            //Write header out fields (to fragment shader)
            WriteOutData();
            Line();

            //For some reason, this is necessary
            WritePipelineData();
        }

        private void WriteTexCoordOutputs()
        {
            if (Mesh.TexCoordBuffers is null)
                return;

            for (int i = 0; i < Mesh.TexCoordBuffers.Length; ++i)
                Line($"{string.Format(FragUVName, i)} = {ECommonBufferType.TexCoord}{i};");
        }

        private void WriteColorOutputs()
        {
            if (Mesh.ColorBuffers is null)
                return;

            for (int i = 0; i < Mesh.ColorBuffers.Length; ++i)
                Line($"{string.Format(FragColorName, i)} = {ECommonBufferType.Color}{i};");
        }

        private void WritePipelineData()
        {
            if (!Engine.Rendering.Settings.AllowShaderPipelines)
                return;

            using (StartOutStructState("gl_PerVertex"))
            {
                Var("vec4", "gl_Position");
                Var("float", "gl_PointSize");
                Var("float", "gl_ClipDistance[]");
            }
            Line();
        }

        private void WriteBuffers()
        {
            //uint blendshapeCount = Mesh.BlendshapeCount;
            uint location = 0u;

            WriteInVar(location++, EShaderVarType._vec3, ECommonBufferType.Position.ToString());

            if (Mesh.NormalsBuffer is not null)
                WriteInVar(location++, EShaderVarType._vec3, ECommonBufferType.Normal.ToString());

            if (Mesh.TangentsBuffer is not null)
                WriteInVar(location++, EShaderVarType._vec3, ECommonBufferType.Tangent.ToString());

            if (Mesh.TexCoordBuffers is not null)
                for (uint i = 0; i < Mesh.TexCoordBuffers.Length; ++i)
                    WriteInVar(location++, EShaderVarType._vec2, $"{ECommonBufferType.TexCoord}{i}");

            if (Mesh.ColorBuffers is not null)
                for (uint i = 0; i < Mesh.ColorBuffers.Length; ++i)
                    WriteInVar(location++, EShaderVarType._vec4, $"{ECommonBufferType.Color}{i}");

            if (Mesh.HasSkinning)
            {
                bool optimizeTo4Weights = Engine.Rendering.Settings.OptimizeSkinningTo4Weights || (Engine.Rendering.Settings.OptimizeSkinningWeightsIfPossible && Mesh.MaxWeightCount <= 4);
                if (optimizeTo4Weights)
                {
                    EShaderVarType intVecVarType = Engine.Rendering.Settings.UseIntegerUniformsInShaders
                        ? EShaderVarType._ivec4
                        : EShaderVarType._vec4;

                    WriteInVar(location++, intVecVarType, ECommonBufferType.BoneMatrixOffset.ToString());
                    WriteInVar(location++, EShaderVarType._vec4, ECommonBufferType.BoneMatrixCount.ToString());
                }
                else
                {
                    EShaderVarType intVarType = Engine.Rendering.Settings.UseIntegerUniformsInShaders
                        ? EShaderVarType._int
                        : EShaderVarType._float;

                    WriteInVar(location++, intVarType, ECommonBufferType.BoneMatrixOffset.ToString());
                    WriteInVar(location++, intVarType, ECommonBufferType.BoneMatrixCount.ToString());
                }
            }

            if (Mesh.BlendshapeCount > 0)
            {
                EShaderVarType intVarType = Engine.Rendering.Settings.UseIntegerUniformsInShaders
                    ? EShaderVarType._int
                    : EShaderVarType._float;

                WriteInVar(location++, intVarType, ECommonBufferType.BlendshapeOffset.ToString());
                WriteInVar(location++, intVarType, ECommonBufferType.BlendshapeCount.ToString());
            }
        }

        private void WriteUniforms()
        {
            WriteUniform(EShaderVarType._mat4, EEngineUniform.ModelMatrix.ToString());

            //TODO: stereo support
            WriteUniform(EShaderVarType._mat4, EEngineUniform.InverseViewMatrix.ToString());
            WriteUniform(EShaderVarType._mat4, EEngineUniform.ProjMatrix.ToString());
            //WriteUniform(EShaderVarType._mat4, EEngineUniform.LeftEyeViewMatrix.ToString());
            //WriteUniform(EShaderVarType._mat4, EEngineUniform.LeftEyeProjMatrix.ToString());
            //WriteUniform(EShaderVarType._mat4, EEngineUniform.RightEyeViewMatrix.ToString());
            //WriteUniform(EShaderVarType._mat4, EEngineUniform.RightEyeProjMatrix.ToString());

            if (Mesh.HasSkinning)
                WriteUniform(EShaderVarType._mat4, EEngineUniform.RootInvModelMatrix.ToString());
        }

        /// <summary>
        /// Shader buffer objects
        /// </summary>
        private void WriteBufferBlocks()
        {
            if (Mesh.HasSkinning)
            {
                using (StartShaderStorageBufferBlock($"{ECommonBufferType.BoneMatrices}Buffer", 0))
                    WriteUniform(EShaderVarType._mat4, ECommonBufferType.BoneMatrices.ToString(), true);

                using (StartShaderStorageBufferBlock($"{ECommonBufferType.BoneInvBindMatrices}Buffer", 1))
                    WriteUniform(EShaderVarType._mat4, ECommonBufferType.BoneInvBindMatrices.ToString(), true);

                bool optimizeTo4Weights = Engine.Rendering.Settings.OptimizeSkinningTo4Weights || (Engine.Rendering.Settings.OptimizeSkinningWeightsIfPossible && Mesh.MaxWeightCount <= 4);
                if (!optimizeTo4Weights)
                {
                    using (StartShaderStorageBufferBlock($"{ECommonBufferType.BoneMatrixIndices}Buffer", 2))
                        WriteUniform(EShaderVarType._int, ECommonBufferType.BoneMatrixIndices.ToString(), true);

                    using (StartShaderStorageBufferBlock($"{ECommonBufferType.BoneMatrixWeights}Buffer", 3))
                        WriteUniform(EShaderVarType._float, ECommonBufferType.BoneMatrixWeights.ToString(), true);
                }
            }

            if (Mesh.BlendshapeCount > 0)
            {
                using (StartShaderStorageBufferBlock($"{ECommonBufferType.BlendshapeIndices}Buffer", 4))
                    WriteUniform(EShaderVarType._ivec4, ECommonBufferType.BlendshapeIndices.ToString(), true);

                using (StartShaderStorageBufferBlock($"{ECommonBufferType.BlendshapeDeltas}Buffer", 5))
                    WriteUniform(EShaderVarType._vec4, ECommonBufferType.BlendshapeDeltas.ToString(), true);

                using (StartShaderStorageBufferBlock($"{ECommonBufferType.BlendshapeWeights}Buffer", 6))
                    WriteUniform(EShaderVarType._float, ECommonBufferType.BlendshapeWeights.ToString(), true);
            }
        }

        /// <summary>
        /// This information is sent to the fragment shader.
        /// </summary>
        private void WriteOutData()
        {
            WriteOutVar(0, EShaderVarType._vec3, FragPosName);

            if (Mesh.NormalsBuffer is not null)
                WriteOutVar(1, EShaderVarType._vec3, FragNormName);

            if (Mesh.TangentsBuffer is not null)
            {
                WriteOutVar(2, EShaderVarType._vec3, FragTanName);
                WriteOutVar(3, EShaderVarType._vec3, FragBinormName);
            }

            if (Mesh.TexCoordBuffers is not null)
                for (int i = 0; i < Mesh.TexCoordBuffers.Length.ClampMax(8); ++i)
                    WriteOutVar(4 + i, EShaderVarType._vec2, string.Format(FragUVName, i));

            if (Mesh.ColorBuffers is not null)
                for (int i = 0; i < Mesh.ColorBuffers.Length.ClampMax(8); ++i)
                    WriteOutVar(12 + i, EShaderVarType._vec4, string.Format(FragColorName, i));

            WriteOutVar(20, EShaderVarType._vec3, FragPosLocalName);
        }

        /// <summary>
        /// Calculates positions, and optionally normals, tangents, and binormals for a rigged mesh.
        /// </summary>
        private void WriteSkinnedMeshInputs()
        {
            bool hasNormals = Mesh.NormalsBuffer is not null;
            bool hasTangents = Mesh.TangentsBuffer is not null;

            Line("vec4 finalPosition = vec4(0.0f);");
            Line($"vec4 basePosition = vec4({ECommonBufferType.Position}, 1.0f);");

            if (hasNormals)
            {
                Line("vec3 finalNormal = vec3(0.0f);");
                Line($"vec3 baseNormal = {ECommonBufferType.Normal};");
            }
            if (hasTangents)
            {
                Line("vec3 finalTangent = vec3(0.0f);");
                Line($"vec3 baseTangent = {ECommonBufferType.Tangent};");
            }

            Line();

            if (!WriteBlendshapeCalc() && !WriteSkinningCalc())
            {
                Line("finalPosition = basePosition;");
                if (hasNormals)
                    Line("finalNormal = baseNormal;");
                if (hasTangents)
                    Line("finalTangent = baseTangent;");
            }

            Line();
            if (hasNormals)
            {
                Line($"{FragNormName} = normalize(normalMatrix * finalNormal);");
                if (hasTangents)
                {
                    Line($"{FragTanName} = normalize(normalMatrix * finalTangent);");
                    Line("vec3 finalBinormal = cross(finalNormal, finalTangent);");
                    Line($"{FragBinormName} = normalize(normalMatrix * finalBinormal);");
                }
            }

            ResolvePosition("finalPosition");
        }

        private bool NeedsSkinningCalc()
            => Mesh.HasSkinning && !Engine.Rendering.Settings.CalculateSkinningInComputeShader;

        private bool NeedsBlendshapeCalc()
            => Mesh.BlendshapeCount > 0 && !Engine.Rendering.Settings.CalculateBlendshapesInComputeShader;

        private bool WriteSkinningCalc()
        {
            if (Engine.Rendering.Settings.CalculateSkinningInComputeShader)
                return false;
            
            bool optimizeTo4Weights = Engine.Rendering.Settings.OptimizeSkinningTo4Weights || (Engine.Rendering.Settings.OptimizeSkinningWeightsIfPossible && Mesh.MaxWeightCount <= 4);
            if (optimizeTo4Weights)
            {
                Line($"for (int i = 0; i < 4; i++)");
                using (OpenBracketState())
                {
                    Line($"int boneIndex = {ECommonBufferType.BoneMatrixOffset}[i];");
                    Line($"float weight = {ECommonBufferType.BoneMatrixCount}[i];");
                    Line($"mat4 boneMatrix = {ECommonBufferType.BoneInvBindMatrices}[boneIndex] * {ECommonBufferType.BoneMatrices}[boneIndex] * {EEngineUniform.RootInvModelMatrix};");
                    Line("finalPosition += (boneMatrix * basePosition) * weight;");
                    Line("mat3 boneMatrix3 = adjoint(boneMatrix);");
                    Line("finalNormal += (boneMatrix3 * baseNormal) * weight;");
                    Line("finalTangent += (boneMatrix3 * baseTangent) * weight;");
                }
            }
            else
            {
                Line($"for (int i = 0; i < {ECommonBufferType.BoneMatrixCount}; i++)");
                using (OpenBracketState())
                {
                    Line($"int index = {ECommonBufferType.BoneMatrixOffset} + i;");
                    Line($"int boneIndex = {ECommonBufferType.BoneMatrixIndices}[index];");
                    Line($"float weight = {ECommonBufferType.BoneMatrixWeights}[index];");
                    Line($"mat4 boneMatrix = {ECommonBufferType.BoneInvBindMatrices}[boneIndex] * {ECommonBufferType.BoneMatrices}[boneIndex] * {EEngineUniform.RootInvModelMatrix};");
                    Line("finalPosition += (boneMatrix * basePosition) * weight;");
                    Line("mat3 boneMatrix3 = adjoint(boneMatrix);");
                    Line("finalNormal += (boneMatrix3 * baseNormal) * weight;");
                    Line("finalTangent += (boneMatrix3 * baseTangent) * weight;");
                }
            }

            return true;
        }
        
        private bool WriteBlendshapeCalc()
        {
            if (Engine.Rendering.Settings.CalculateBlendshapesInComputeShader || Mesh.BlendshapeCount == 0)
                return false;
            
            const string minWeight = "0.01f";
            if (Mesh.MaxBlendshapeAccumulation)
            {
                // MAX blendshape accumulation
                Line("vec3 maxPositionDelta = vec3(0.0f);");
                Line("vec3 maxNormalDelta = vec3(0.0f);");
                Line("vec3 maxTangentDelta = vec3(0.0f);");

                Line($"for (int i = 0; i < {ECommonBufferType.BlendshapeCount}; i++)");
                using (OpenBracketState())
                {
                    Line($"int index = {ECommonBufferType.BlendshapeOffset} + i;");

                    Line($"ivec4 blendshapeIndices = {ECommonBufferType.BlendshapeIndices}[index];");
                    Line($"int blendshapeIndex = blendshapeIndices.x;");
                    Line($"float weight = {ECommonBufferType.BlendshapeWeights}[blendshapeIndex];");

                    Line($"if (weight > {minWeight})");
                    using (OpenBracketState())
                    {
                        Line($"int blendshapeDeltaPosIndex = blendshapeIndices.y;");
                        Line($"int blendshapeDeltaNrmIndex = blendshapeIndices.z;");
                        Line($"int blendshapeDeltaTanIndex = blendshapeIndices.w;");
                        Line($"maxPositionDelta = max(maxPositionDelta, {ECommonBufferType.BlendshapeDeltas}[blendshapeDeltaPosIndex].xyz * weight);");
                        Line($"maxNormalDelta = max(maxNormalDelta, {ECommonBufferType.BlendshapeDeltas}[blendshapeDeltaNrmIndex].xyz * weight);");
                        Line($"maxTangentDelta = max(maxTangentDelta, {ECommonBufferType.BlendshapeDeltas}[blendshapeDeltaTanIndex].xyz * weight);");
                    }
                }

                Line("finalPosition += vec4(maxPositionDelta, 0.0f);");
                Line("finalNormal += maxNormalDelta;");
                Line("finalTangent += maxTangentDelta;");
            }
            else
            {
                // Use a single loop to accumulate blendshape deltas
                Line("vec3 positionDeltaSum = vec3(0.0f);");
                Line("vec3 normalDeltaSum = vec3(0.0f);");
                Line("vec3 tangentDeltaSum = vec3(0.0f);");

                Line($"for (int i = 0; i < {ECommonBufferType.BlendshapeCount}; i++)");
                using (OpenBracketState())
                {
                    Line($"int index = {ECommonBufferType.BlendshapeOffset} + i;");

                    Line($"ivec4 blendshapeIndices = {ECommonBufferType.BlendshapeIndices}[index];");
                    Line($"int blendshapeIndex = blendshapeIndices.x;");
                    Line($"float weight = {ECommonBufferType.BlendshapeWeights}[blendshapeIndex];");

                    Line($"if (weight > {minWeight})");
                    using (OpenBracketState())
                    {
                        Line($"int blendshapeDeltaPosIndex = blendshapeIndices.y;");
                        Line($"int blendshapeDeltaNrmIndex = blendshapeIndices.z;");
                        Line($"int blendshapeDeltaTanIndex = blendshapeIndices.w;");
                        Line($"positionDeltaSum += {ECommonBufferType.BlendshapeDeltas}[blendshapeDeltaPosIndex].xyz * weight;");
                        Line($"normalDeltaSum += {ECommonBufferType.BlendshapeDeltas}[blendshapeDeltaNrmIndex].xyz * weight;");
                        Line($"tangentDeltaSum += {ECommonBufferType.BlendshapeDeltas}[blendshapeDeltaTanIndex].xyz * weight;");
                    }
                }

                Line("finalPosition += vec4(positionDeltaSum, 0.0f);");
                Line("finalNormal += normalDeltaSum;");
                Line("finalTangent += tangentDeltaSum;");
            }

            return true;
        }

        /// <summary>
        /// Calculates positions, and optionally normals, tangents, and binormals for a static mesh.
        /// </summary>
        private void WriteStaticMeshInputs()
        {
            Line($"vec4 position = vec4({ECommonBufferType.Position}, 1.0f);");
            if (Mesh.NormalsBuffer is not null)
                Line($"vec3 normal = {ECommonBufferType.Normal};");
            if (Mesh.TangentsBuffer is not null)
                Line($"vec3 tangent = {ECommonBufferType.Tangent};");
            Line();

            if (!Engine.Rendering.Settings.CalculateBlendshapesInComputeShader && Mesh.BlendshapeCount > 0)
            {
                
            }

            ResolvePosition("position");

            if (Mesh.NormalsBuffer is not null)
            {
                Line($"{FragNormName} = normalize(normalMatrix * normal);");
                if (Mesh.TangentsBuffer is not null)
                {
                    Line($"{FragTanName} = normalize(normalMatrix * tangent);");
                    Line("vec3 binormal = cross(normal, tangent);");
                    Line($"{FragBinormName} = normalize(normalMatrix * binormal);");
                }
            }
        }
        private void ResolvePosition(string posName)
        {
            //Line("mat4 ViewMatrix = WorldToCameraSpaceMatrix;");
            //if (mesh.BillboardingFlags == ECameraTransformFlags.None)
            //{
            //    Line($"{posName} = ModelMatrix * Vector4({posName}.xyz, 1.0f);");
            //    Line($"{FragPosName} = {posName}.xyz;");
            //    Line($"gl_Position = ProjMatrix * ViewMatrix * {posName};");
            //    return;
            //}
            //Line("mat4 BillboardMatrix = CameraToWorldSpaceMatrix;");
            //if (mesh.BillboardingFlags.HasFlag(ECameraTransformFlags.RotateX))
            //{
            //    //Do not align X column to be stationary from camera's viewpoint
            //    Line("ViewMatrix[0][0] = 1.0f;");
            //    Line("ViewMatrix[0][1] = 0.0f;");
            //    Line("ViewMatrix[0][2] = 0.0f;");

            //    //Do not fix Y column to rotate with camera
            //    Line("BillboardMatrix[1][0] = 0.0f;");
            //    Line("BillboardMatrix[1][1] = 1.0f;");
            //    Line("BillboardMatrix[1][2] = 0.0f;");

            //    //Do not fix Z column to rotate with camera
            //    Line("BillboardMatrix[2][0] = 0.0f;");
            //    Line("BillboardMatrix[2][1] = 0.0f;");
            //    Line("BillboardMatrix[2][2] = 1.0f;");
            //}
            //if (mesh.BillboardingFlags.HasFlag(ECameraTransformFlags.RotateY))
            //{
            //    //Do not fix X column to rotate with camera
            //    Line("BillboardMatrix[0][0] = 1.0f;");
            //    Line("BillboardMatrix[0][1] = 0.0f;");
            //    Line("BillboardMatrix[0][2] = 0.0f;");

            //    //Do not align Y column to be stationary from camera's viewpoint
            //    Line("ViewMatrix[1][0] = 0.0f;");
            //    Line("ViewMatrix[1][1] = 1.0f;");
            //    Line("ViewMatrix[1][2] = 0.0f;");

            //    //Do not fix Z column to rotate with camera
            //    Line("BillboardMatrix[2][0] = 0.0f;");
            //    Line("BillboardMatrix[2][1] = 0.0f;");
            //    Line("BillboardMatrix[2][2] = 1.0f;");
            //}
            //if (mesh.BillboardingFlags.HasFlag(ECameraTransformFlags.RotateZ))
            //{
            //    //Do not fix X column to rotate with camera
            //    Line("BillboardMatrix[0][0] = 1.0f;");
            //    Line("BillboardMatrix[0][1] = 0.0f;");
            //    Line("BillboardMatrix[0][2] = 0.0f;");

            //    //Do not fix Y column to rotate with camera
            //    Line("BillboardMatrix[1][0] = 0.0f;");
            //    Line("BillboardMatrix[1][1] = 1.0f;");
            //    Line("BillboardMatrix[1][2] = 0.0f;");

            //    //Do not align Z column to be stationary from camera's viewpoint
            //    Line("ViewMatrix[2][0] = 0.0f;");
            //    Line("ViewMatrix[2][1] = 0.0f;");
            //    Line("ViewMatrix[2][2] = 1.0f;");
            //}
            //if (mesh.BillboardingFlags.HasFlag(ECameraTransformFlags.ConstrainTranslationX))
            //{
            //    //Clear X translation
            //    Line("ViewMatrix[3][0] = 0.0f;");
            //    Line("BillboardMatrix[3][0] = 0.0f;");
            //}
            //if (mesh.BillboardingFlags.HasFlag(ECameraTransformFlags.ConstrainTranslationY))
            //{
            //    //Clear Y translation
            //    Line("ViewMatrix[3][1] = 0.0f;");
            //    Line("BillboardMatrix[3][1] = 0.0f;");
            //}
            //if (mesh.BillboardingFlags.HasFlag(ECameraTransformFlags.ConstrainTranslationZ))
            //{
            //    //Clear Z translation
            //    Line("ViewMatrix[3][2] = 0.0f;");
            //    Line("BillboardMatrix[3][2] = 0.0f;");
            //}
            Line($"{FragPosLocalName} = {posName}.xyz;");
            Line($"vec4 glPos = mvpMatrix * {posName};");
            Line($"{FragPosName} = glPos.xyz / glPos.w;");
            Line($"gl_Position = glPos;");
        }
    }
}
