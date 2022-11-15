using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GodRay
{
    public class MenuItems
    {
        [MenuItem("GameObject/3D Object/Cone", false, priority = 7)]
        public static void CreateCone()
        {
            SpawnConeInHierarchy();
        }

        [MenuItem("GameObject/Tool/SaveMesh", false, priority = 7)]
        public static void SaveMesh()
        {
            Transform[] selections = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
            if (selections.Length <= 0)
            {
                // 没有选中目标，不保存
                return;
            }
            else
            {
                Transform aimTransform = selections[0];
                MeshFilter mf = aimTransform.GetComponent<MeshFilter>();
                if (!mf) return;
                using (StreamWriter streamWriter = new StreamWriter(string.Format("{0}{1}.obj", "./Assets/", aimTransform.name)))
                {
                    streamWriter.Write(MeshToString(mf, new Vector3(-1f, 1f, 1f)));
                    streamWriter.Close();
                }

                AssetDatabase.Refresh();
                // create prefab
                // Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(string.Format("{0}{1}.obj", "", aimTransform.name));
                // mf.mesh = mesh;
                //
                // PrefabUtility.CreatePrefab(string.Format("{0}{1}.prefab", "", aimTransform.name),
                //     aimTransform.gameObject);
                // AssetDatabase.Refresh();

                // MeshToString(mf,new Vector3(-1f, 1f, 1f));
            }
            // foreach (Transform selection in selections)
            // {
            //     GameObject cone = new GameObject("Cone");
            //     cone.transform.SetParent(selections[0]);
            //     cone.transform.localPosition = Vector3.zero;
            //     cone.transform.localRotation = Quaternion.identity;
            //     cone.transform.localScale = Vector3.one;
            //     Undo.RegisterCreatedObjectUndo(cone, "Undo Create Cone");
            //     SetMesh2(cone);
            // }
        }

        public const int GeomSidesDefault = 24;

        public const int GeomSegmentsDefault = 5;
        public const bool GeomCap = false;

        public static void SetMesh2(GameObject go)
        {
            int geomCustomSides = GeomSidesDefault;
            int geomCustomSegments = GeomSegmentsDefault;
            bool geomCap = GeomCap;
            // var radiusEnd = lengthZ * Mathf.Tan(coneAngle * Mathf.Deg2Rad * 0.5f);
            // VolumetricLightBeam
            Mesh myMesh = GenerateConeZ_Radius(1f, 1f, 1f, geomCustomSides, geomCustomSegments, true, true);
            // myMesh.RecalculateBounds();
            // myMesh.RecalculateNormals();
            // myMesh.RecalculateTangents();
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = myMesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            // TODO:附加godRay材质
            Material myMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mr.sharedMaterial = myMat;
        }

        public static void SpawnConeInHierarchy()
        {
            Transform[] selections = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
            if (selections.Length <= 0)
            {
                GameObject cone = new GameObject("Cone");
                cone.transform.position = Vector3.zero;
                cone.transform.rotation = Quaternion.identity;
                cone.transform.localScale = Vector3.one;
                Undo.RegisterCreatedObjectUndo(cone, "Undo Create Cone");
                SetMesh2(cone);
                return;
            }

            foreach (Transform selection in selections)
            {
                GameObject cone = new GameObject("Cone");
                cone.transform.SetParent(selection);
                cone.transform.localPosition = Vector3.zero;
                cone.transform.localRotation = Quaternion.identity;
                cone.transform.localScale = Vector3.one;
                Undo.RegisterCreatedObjectUndo(cone, "Undo Create Cone");
                SetMesh2(cone);
            }
        }

        const float kMinTruncatedRadius = 0.001f;


        /// <summary>
        /// 创建mesh
        /// </summary>
        /// <param name="lengthZ"></param>
        /// <param name="radiusStart"></param>
        /// <param name="radiusEnd"></param>
        /// <param name="numSides"></param>
        /// <param name="numSegments"></param>
        /// <param name="cap"></param>
        /// <param name="doubleSided"></param>
        /// <returns></returns>
        public static Mesh GenerateConeZ_Radius(float lengthZ, float radiusStart, float radiusEnd, int numSides,
            int numSegments, bool cap, bool doubleSided)
        {
            Mesh mesh = new Mesh();
            bool genCap = true;
            // We use the XY position of the vertices to compute the cone normal in the shader.
            // With a perfectly sharp cone, we couldn't compute accurate normals at its top.
            radiusStart = Mathf.Max(radiusStart, kMinTruncatedRadius);

            int vertCountSides = numSides * (numSegments + 2);
            int vertCountTotal = vertCountSides;

            if (genCap)
                vertCountTotal += numSides + 1;

            // VERTICES
            {
                var vertices = new Vector3[vertCountTotal];

                for (int i = 0; i < numSides; i++)
                {
                    float angle = 2 * Mathf.PI * i / numSides;
                    float angleCos = Mathf.Cos(angle);
                    float angleSin = Mathf.Sin(angle);

                    for (int seg = 0; seg < numSegments + 2; seg++)
                    {
                        float tseg = (float) seg / (numSegments + 1);
                        Debug.Assert(tseg >= 0f && tseg <= 1f);
                        float radius = Mathf.Lerp(radiusStart, radiusEnd, tseg);
                        vertices[i + seg * numSides] =
                            new Vector3(radius * angleCos, radius * angleSin, tseg * lengthZ);
                    }
                }

                if (genCap)
                {
                    int ind = vertCountSides;

                    vertices[ind] = Vector3.zero;
                    ind++;

                    for (int i = 0; i < numSides; i++)
                    {
                        float angle = 2 * Mathf.PI * i / numSides;
                        float angleCos = Mathf.Cos(angle);
                        float angleSin = Mathf.Sin(angle);
                        vertices[ind] = new Vector3(radiusStart * angleCos, radiusStart * angleSin, 0f);
                        ind++;
                    }

                    Debug.Assert(ind == vertices.Length);
                }

                if (!doubleSided)
                {
                    mesh.vertices = vertices;
                }
                else
                {
                    var vertices2 = new Vector3[vertices.Length * 2];
                    vertices.CopyTo(vertices2, 0);
                    vertices.CopyTo(vertices2, vertices.Length);
                    mesh.vertices = vertices2;
                }
            }

            // UV (used to flags vertices as sides or cap)
            // X: 0 = sides ; 1 = cap
            // Y: 0 = front face ; 1 = back face (doubleSided only)
            {
                var uv = new Vector2[vertCountTotal];
                int ind = 0;
                for (int i = 0; i < vertCountSides; i++)
                    uv[ind++] = Vector2.zero;

                if (genCap)
                {
                    for (int i = 0; i < numSides + 1; i++)
                        uv[ind++] = new Vector2(1, 0);
                }

                Debug.Assert(ind == uv.Length);


                if (!doubleSided)
                {
                    mesh.uv = uv;
                }
                else
                {
                    var uv2 = new Vector2[uv.Length * 2];
                    uv.CopyTo(uv2, 0);
                    uv.CopyTo(uv2, uv.Length);

                    for (int i = 0; i < uv.Length; i++)
                    {
                        var value = uv2[i + uv.Length];
                        uv2[i + uv.Length] = new Vector2(value.x, 1);
                    }

                    mesh.uv = uv2;
                }
            }

            // INDICES
            {
                int triCountSides = numSides * 2 * Mathf.Max(numSegments + 1, 1);
                int indCountSides = triCountSides * 3;
                int indCountTotal = indCountSides;

                if (genCap)
                    indCountTotal += numSides * 3;

                var indices = new int[indCountTotal];
                int ind = 0;

                for (int i = 0; i < numSides; i++)
                {
                    int ip1 = i + 1;
                    if (ip1 == numSides)
                        ip1 = 0;

                    for (int k = 0; k < numSegments + 1; ++k)
                    {
                        var offset = k * numSides;

                        indices[ind++] = offset + i;
                        indices[ind++] = offset + ip1;
                        indices[ind++] = offset + i + numSides;

                        indices[ind++] = offset + ip1 + numSides;
                        indices[ind++] = offset + i + numSides;
                        indices[ind++] = offset + ip1;
                    }
                }

                if (genCap)
                {
                    for (int i = 0; i < numSides - 1; i++)
                    {
                        indices[ind++] = vertCountSides;
                        indices[ind++] = vertCountSides + i + 2;
                        indices[ind++] = vertCountSides + i + 1;
                    }

                    indices[ind++] = vertCountSides;
                    indices[ind++] = vertCountSides + 1;
                    indices[ind++] = vertCountSides + numSides;
                }

                Debug.Assert(ind == indices.Length);

                if (!doubleSided)
                {
                    mesh.triangles = indices;
                }
                else
                {
                    var indices2 = new int[indices.Length * 2];
                    indices.CopyTo(indices2, 0);

                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        indices2[indices.Length + i + 0] = indices[i + 0] + vertCountTotal;
                        indices2[indices.Length + i + 1] = indices[i + 2] + vertCountTotal;
                        indices2[indices.Length + i + 2] = indices[i + 1] + vertCountTotal;
                    }

                    mesh.triangles = indices2;
                }
            }

            var bounds = new Bounds(
                new Vector3(0, 0, lengthZ * 0.5f),
                new Vector3(Mathf.Max(radiusStart, radiusEnd) * 2, Mathf.Max(radiusStart, radiusEnd) * 2, lengthZ)
            );
            mesh.bounds = bounds;

            Debug.Assert(mesh.vertexCount == GetVertexCount(numSides, numSegments, genCap, doubleSided));
            Debug.Assert(mesh.triangles.Length == GetIndicesCount(numSides, numSegments, genCap, doubleSided));

            return mesh;
        }

        public static int GetVertexCount(int numSides, int numSegments, bool geomCap, bool doubleSided)
        {
            Debug.Assert(numSides >= 2);
            Debug.Assert(numSegments >= 0);

            int count = numSides * (numSegments + 2);
            if (geomCap) count += numSides + 1;
            if (doubleSided) count *= 2;
            return count;
        }

        public static int GetIndicesCount(int numSides, int numSegments, bool geomCap, bool doubleSided)
        {
            Debug.Assert(numSides >= 2);
            Debug.Assert(numSegments >= 0);

            int count = numSides * (numSegments + 1) * 2 * 3;
            if (geomCap) count += numSides * 3;
            if (doubleSided) count *= 2;
            return count;
        }

        /// <summary>
        /// 将mesh转换为固定格式的字符串流，之后写入文件即可保存为obj文件
        /// </summary>
        /// <param name="mf"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private static string MeshToString(MeshFilter mf, Vector3 scale)
        {
            Mesh mesh = mf.mesh;
            Material[] sharedMaterials = mf.GetComponent<Renderer>().sharedMaterials;
            Vector2 textureOffset = mf.GetComponent<Renderer>().material.GetTextureOffset("_MainTex");
            Vector2 textureScale = mf.GetComponent<Renderer>().material.GetTextureScale("_MainTex");

            StringBuilder stringBuilder = new StringBuilder().Append("mtllib design.mtl")
                .Append("\n")
                .Append("g ")
                .Append(mf.name)
                .Append("\n");

            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vector = vertices[i];
                stringBuilder.Append(string.Format("v {0} {1} {2}\n", vector.x * scale.x, vector.y * scale.y,
                    vector.z * scale.z));
            }

            stringBuilder.Append("\n");

            Dictionary<int, int> dictionary = new Dictionary<int, int>();

            if (mesh.subMeshCount > 1)
            {
                int[] triangles = mesh.GetTriangles(1);

                for (int j = 0; j < triangles.Length; j += 3)
                {
                    if (!dictionary.ContainsKey(triangles[j]))
                    {
                        dictionary.Add(triangles[j], 1);
                    }

                    if (!dictionary.ContainsKey(triangles[j + 1]))
                    {
                        dictionary.Add(triangles[j + 1], 1);
                    }

                    if (!dictionary.ContainsKey(triangles[j + 2]))
                    {
                        dictionary.Add(triangles[j + 2], 1);
                    }
                }
            }

            for (int num = 0; num != mesh.uv.Length; num++)
            {
                Vector2 vector2 = Vector2.Scale(mesh.uv[num], textureScale) + textureOffset;

                if (dictionary.ContainsKey(num))
                {
                    stringBuilder.Append(string.Format("vt {0} {1}\n", mesh.uv[num].x, mesh.uv[num].y));
                }
                else
                {
                    stringBuilder.Append(string.Format("vt {0} {1}\n", vector2.x, vector2.y));
                }
            }

            for (int k = 0; k < mesh.subMeshCount; k++)
            {
                stringBuilder.Append("\n");

                if (k == 0)
                {
                    stringBuilder.Append("usemtl ").Append("Material_design").Append("\n");
                }

                if (k == 1)
                {
                    stringBuilder.Append("usemtl ").Append("Material_logo").Append("\n");
                }

                int[] triangles2 = mesh.GetTriangles(k);

                for (int l = 0; l < triangles2.Length; l += 3)
                {
                    stringBuilder.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", triangles2[l] + 1,
                        triangles2[l + 2] + 1, triangles2[l + 1] + 1));
                }
            }

            return stringBuilder.ToString();
        }
    }
}