using System;
using System.Collections.Generic;
using System.Linq;
using Sceelix.Core.Annotations;
using Sceelix.Core.Attributes;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Extensions;
using Sceelix.Mathematics.Data;
using Sceelix.Mathematics.Parameters;
using Sceelix.Meshes.Data;
using Sceelix.Paths.Data;


namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// Randomly distort the mesh's vertices
    /// </summary>
    [Procedure("8034DF9C-9104-453D-B249-B06D9073AC33", Label = "Mesh Distort")]
    public class MeshDistort : SystemProcedure
    {
        /// <summary>
        /// Input and Output
        /// </summary>
        private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// The direction (and amount) of the distortion
        /// </summary>
        private readonly Vector3DParameter _parameterAmount = new Vector3DParameter("Direction");

        /// <summary>
        /// The random seed
        /// </summary>
        private readonly IntParameter _parameterSeed = new IntParameter("Seed", 1000);

        /// <summary>
        /// What the direction is referring to. 
        /// </summary>
        private readonly ChoiceParameter _parameterRelativeTo = new ChoiceParameter("Relative To", "Normals", new string[]
        {
            "Normals",
            "World",
            "Scope"
        });

        private class VertexEx
        {
            public Vertex Vertex = null;
            public List<Face> adjacentFace = new List<Face>();

            public Vector3D GetVertexNormal()
            {
                Vector3D v = new Vector3D();
                foreach(Face face in adjacentFace)
                    v += face.Normal;

                return v / adjacentFace.Count;
            }
        }

        List<VertexEx> vertices = new List<VertexEx>();

        private void CollateVertices(Face face, Vertex vertex)
        {
            var q = from foundvertex in vertices
                    where foundvertex.Vertex == vertex
                    select foundvertex;

            VertexEx found = null;

            if(q.Count() == 0)
            {
                found = new VertexEx() { Vertex = vertex };
                vertices.Add(found);
            }
            else
                found = q.First();

            found.adjacentFace.Add(face);
        }

        protected override void Run()
        {
            MeshEntity meshEntity = _input.Read();

            Vector3D actualTranslation;
            
            actualTranslation = _parameterRelativeTo.Value == "World" 
                ? _parameterAmount.Value 
                : meshEntity.BoxScope.ToWorldDirection(_parameterAmount.Value);

            foreach(Face face0 in meshEntity.Faces)
                foreach(Vertex vertex0 in face0.Vertices)
                    CollateVertices(face0, vertex0);

            Random random = new Random(_parameterSeed.Value);

            foreach(Face face1 in meshEntity.Faces)
                foreach(Vertex vertex1 in face1.Vertices)
                {
                    if(_parameterRelativeTo.Value == "Normals")
                    {
                        var q = from foundvertex in vertices
                                where foundvertex.Vertex == vertex1
                                select foundvertex;
                        actualTranslation = q.First().GetVertexNormal();
                        actualTranslation *= _parameterAmount.Value.Length;
                    }

                    float r = random.Float(-1, 1);
                    vertex1.Position = vertex1.Position + actualTranslation * r;
                }


            // Data was modified in-place, no need to re-construct.
            MeshEntity output = meshEntity;
            //meshEntity.Attributes.SetAttributesTo(output.Attributes);

            //finally, return the newly create meshEntity
            _output.Write(output);
        }
    }
}
