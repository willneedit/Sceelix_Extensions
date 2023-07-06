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
    /// Subdivides the mesh's faces (preferably triangles) into smaller faces,
    /// like Mesh Split, but independent of the faces' orientations.
    /// </summary>
    [Procedure("8034DF9C-9104-453D-B249-B06D9073AC32", Label = "Mesh Tessellate")]
    public class MeshTessellate : SystemProcedure
    {
        /// <summary>
        /// Inputs and Output
        /// </summary>
        private readonly SingleInput<MeshEntity> _inputA = new SingleInput<MeshEntity>("Input");
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// The desired operation
        /// </summary>
        private readonly ChoiceParameter _parameterOperation = new ChoiceParameter("Operation", "Triangulate", new string[]
        {
            "Triangulate",
            "Tessellate"
        });

        protected override void Run()
        {
            MeshEntity meshEntityA = _inputA.Read();

            List<Face> tris = new List<Face>();

            if(_parameterOperation.Value == "Triangulate")
                foreach(Face face0 in meshEntityA.Faces)
                    Tessellation.TriangleFan(face0, tris);

            if(_parameterOperation.Value == "Tessellate")
                foreach(Face face1 in meshEntityA.Faces)
                {
                    List<Face> intermediate = new List<Face>();
                    Tessellation.TriangleFan(face1, intermediate);
                    foreach(Face intFace in intermediate)
                        Tessellation.Tessellate(intFace, tris);
                }

            MeshEntity output = new MeshEntity(tris.ToArray());

            // And, weld the vertices of adjacent faces.
            MeshUnifyProcedure.UnifyVerticesParameter.Unify(output);

            meshEntityA.Attributes.SetAttributesTo(output.Attributes);
            //finally, return the newly create meshEntity
            _output.Write(output);
        }
    }
}
