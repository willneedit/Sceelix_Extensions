using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;
using Sceelix.Meshes.Procedures;
using Sceelix.Paths.Data;


namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// Performs Constructive Solid Geometry (boolean operations on solid objects)
    /// </summary>
    [Procedure("8034DF9C-9104-453D-B249-B06D9073AC31", Label = "Mesh Boolean")]
    public class MeshBoolean : SystemProcedure
    {
        /// <summary>
        /// Inputs and Output
        /// </summary>
        private readonly SingleInput<MeshEntity> _inputA = new SingleInput<MeshEntity>("Input A");
        private readonly SingleInput<MeshEntity> _inputB = new SingleInput<MeshEntity>("Input B");
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// The desired operation
        /// </summary>
        private readonly BoolParameter _flipA = new BoolParameter("Flip Normals A", false);
        private readonly BoolParameter _flipB = new BoolParameter("Flip Normals B", false);
        private readonly BoolParameter _flipOut = new BoolParameter("Flip Normals Output", false);
        private readonly ChoiceParameter _parameterOperation = new ChoiceParameter("Operation", "Intersection", new string[]
        {
            "Intersection",
            "Union",
            "Difference"
        });

        private int FindOrAddVert(Vertex vert, List<Net3dBool.Vector3d> vertList, List<int> vertIndices)
        {
            Net3dBool.Vector3d v = new Net3dBool.Vector3d(vert.Position.X, vert.Position.Y, vert.Position.Z);

            int found = -1;
            for(int i = 0, c = vertList.Count; i < c; ++i)
                if(vertList[i] == v)
                {
                    found = i;
                    break;
                }

            if(found < 0)
            {
                found = vertList.Count();
                vertList.Add(v);
            }

            vertIndices.Add(found);
            return found;
        }

        private bool ConvertN3dSolid(MeshEntity mesh, bool flip, string desc, out Net3dBool.Solid solid)
        {
            solid = null;

            // Triangulate (simple): Convert nGons into triangle fans
            List<Face> tris = new List<Face>();
            List<Net3dBool.Vector3d> vertList = new List<Net3dBool.Vector3d>();
            List<int> vertIndices = new List<int>();

            foreach(Face face in mesh.Faces)
                Tessellation.TriangleFan(face, tris);

            foreach(Face face in tris)
            {
                Vertex[] verts = face.Vertices.ToArray();

                if(flip)
                {
                    FindOrAddVert(verts[0], vertList, vertIndices);
                    FindOrAddVert(verts[1], vertList, vertIndices);
                    FindOrAddVert(verts[2], vertList, vertIndices);
                }
                else
                {
                    FindOrAddVert(verts[2], vertList, vertIndices);
                    FindOrAddVert(verts[1], vertList, vertIndices);
                    FindOrAddVert(verts[0], vertList, vertIndices);
                }
            }

            solid = new Net3dBool.Solid(vertList.ToArray(), vertIndices.ToArray());
            return true;
        }

        private Vector3D ToVector3D(Net3dBool.Vector3d v3)
        {
            return new Vector3D((float) v3.X, (float) v3.Y, (float) v3.Z);
        }

        private bool AssembleMeshEntity(Net3dBool.Solid Solid, out MeshEntity meshEntity)
        {
            Net3dBool.Vector3d[] verts = Solid.GetVertices();
            int[] indices = Solid.GetIndices();

            List<Face> faces = new List<Face>();


            int maxIndex = indices.Max() + 1;

            Vertex[] vertices = new Vertex[maxIndex];
            for(int i1 = 0; i1 < maxIndex; ++i1)
                vertices[i1] = new Vertex(ToVector3D(verts[i1]));

            for(int i2 = 0, c = indices.Length; i2 < c; i2 += 3)
            {
                Vertex[] facepos = null;
                if(_flipOut.Value)
                {
                    facepos = new Vertex[]
                    {
                        vertices[indices[i2]],
                        vertices[indices[i2 + 1]],
                        vertices[indices[i2 + 2]],
                    };
                }
                else 
                {
                    facepos = new Vertex[]
                    {
                        vertices[indices[i2 + 2]],
                        vertices[indices[i2 + 1]],
                        vertices[indices[i2]],
                    };
                }

                faces.Add(new Face(facepos));                  
            }
            meshEntity = new MeshEntity(faces.ToArray());
            return true;
        }

        protected override void Run()
        {
            MeshEntity meshEntityA = _inputA.Read();
            MeshEntity meshEntityB = _inputB.Read();


            if(!ConvertN3dSolid(meshEntityA, _flipA.Value, "Input A", out Net3dBool.Solid SolidA))
                return;

            if(!ConvertN3dSolid(meshEntityB, _flipB.Value, "Input B", out Net3dBool.Solid SolidB))
                return;

            Net3dBool.BooleanModeller modeller = new Net3dBool.BooleanModeller(SolidA, SolidB);

            Net3dBool.Solid result = null;

            if(_parameterOperation.Value == "Intersection")
                result = modeller.GetIntersection();

            if(_parameterOperation.Value == "Union")
                result = modeller.GetUnion();

            if(_parameterOperation.Value == "Difference")
                result = modeller.GetDifference();

            AssembleMeshEntity(result, out MeshEntity output);

            //finally, return the newly create meshEntity

            meshEntityA.Attributes.SetAttributesTo(output.Attributes);
            meshEntityB.Attributes.SetAttributesTo(output.Attributes);

            // And, weld the vertices of adjacent faces.
            MeshUnifyProcedure.UnifyVerticesParameter.Unify(output);

            _output.Write(output);
        }
    }
}
