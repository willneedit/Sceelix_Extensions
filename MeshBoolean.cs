using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Sceelix.Meshes.Data;
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

        private bool ConvertN3dSolid(MeshEntity mesh, bool flip, string desc, out Net3dBool.Solid solid)
        {
            solid = null;

            // Triangulate (simple): Convert nGons into triangle fans
            List<Face> tris = new List<Face>();
            foreach(Face face in mesh.Faces)
            {
                if(face.Vertices.Count() < 3)
                {
                    Logger.Log($"{desc} - degenerated faces, skipping.", Logging.LogType.Warning);
                    continue;
                }

                Vertex[] verts = face.Vertices.ToArray();

                for(int i0 = 2; i0 < face.Vertices.Count(); ++i0)
                {
                    tris.Add(new Face(new Vertex[3]
                    {
                        verts[0],
                        verts[i0 - 1],
                        verts[i0]
                    }));
                }
            }

            int[] vertIndices = new int[tris.Count() * 3];
            Net3dBool.Vector3d[] vertices = new Net3dBool.Vector3d[tris.Count() * 3];

            int i = 0;
            foreach(Face face in tris)
            {
                Vertex[] verts = face.Vertices.ToArray();

                if(!flip)
                {
                    vertIndices[i] = i + 2;
                    vertIndices[i + 2] = i;
                }
                else
                {
                    vertIndices[i] = i;
                    vertIndices[i + 2] = i + 2;
                }

                vertIndices[i+1] = i+1;

                vertices[i  ] = new Net3dBool.Vector3d(verts[0].Position.X, verts[0].Position.Y, verts[0].Position.Z);
                vertices[i+1] = new Net3dBool.Vector3d(verts[1].Position.X, verts[1].Position.Y, verts[1].Position.Z);
                vertices[i+2] = new Net3dBool.Vector3d(verts[2].Position.X, verts[2].Position.Y, verts[2].Position.Z);

                i += 3;
            }

            solid = new Net3dBool.Solid(vertices, vertIndices);
            return true;
        }

        private Vector3D ToVertex(Net3dBool.Vector3d v3)
        {
            return new Vector3D((float) v3.X, (float) v3.Y, (float) v3.Z);
        }

        private bool AssembleMeshEntity(Net3dBool.Solid Solid, out MeshEntity meshEntity)
        {
            Net3dBool.Vector3d[] verts = Solid.GetVertices();
            int[] indices = Solid.GetIndices();

            List<Face> faces = new List<Face>();

            for(int i = 0, c = indices.Length; i < c; i += 3)
            {
                Vector3D[] facepos = null;
                if(_flipOut.Value)
                {
                    facepos = new Vector3D[]
                    {
                        ToVertex(verts[indices[i]]),
                        ToVertex(verts[indices[i + 1]]),
                        ToVertex(verts[indices[i + 2]]),
                    };
                }
                else 
                {
                    facepos = new Vector3D[]
                    {
                        ToVertex(verts[indices[i + 2]]),
                        ToVertex(verts[indices[i + 1]]),
                        ToVertex(verts[indices[i]]),
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
            _output.Write(output);
        }
    }
}
