using System;
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

namespace Sceelix.Extensions
{
    internal static class Tessellation
    {
        public static void TriangleFan(Face face, List<Face> tris)
        {
            Vertex[] verts = face.Vertices.ToArray();

            for(int i0 = 2; i0 < face.Vertices.Count(); ++i0)
            {
                Face newFace = new Face(new Vertex[3]
                    {
                            verts[0],
                            verts[i0 - 1],
                            verts[i0]
                    });

                newFace.Attributes.SetAttributesTo(face.Attributes);

                tris.Add(newFace);
            }
        }

        public static Vertex MeanVertex(Vertex v1, Vertex v2)
        {
            Vector3D v = new Vector3D(
                (v1.Position.X + v2.Position.X) / 2,
                (v1.Position.Y + v2.Position.Y) / 2,
                (v1.Position.Z + v2.Position.Z) / 2
            );


            Vertex vertex = new Vertex(v);
            v1.Attributes.SetAttributesTo(vertex.Attributes);

            return vertex;
        }

        public static void Tessellate(Face face, List<Face> tris)
        {
            Vertex[] verts = face.Vertices.ToArray();

            if(verts.Length != 3)
                throw new ArgumentException("Mesh must be triangulated");

            Vertex[] newVerts = new Vertex[6];

            // the original triangle
            newVerts[0] = verts[0];
            newVerts[1] = verts[1];
            newVerts[2] = verts[2];

            // The dividing triangle
            newVerts[3] = MeanVertex(verts[0], verts[1]);
            newVerts[4] = MeanVertex(verts[1], verts[2]);
            newVerts[5] = MeanVertex(verts[2], verts[0]);

            tris.Add(new Face(new Vertex[3]
            {
                newVerts[0],
                newVerts[3],
                newVerts[5]
            }));

            tris.Add(new Face(new Vertex[3]
            {
                newVerts[3],
                newVerts[1],
                newVerts[4]
            }));

            tris.Add(new Face(new Vertex[3]
            {
                newVerts[5],
                newVerts[4],
                newVerts[2]
            }));

            tris.Add(new Face(new Vertex[3]
            {
                newVerts[3],
                newVerts[4],
                newVerts[5]
            }));

            foreach(Face newFace in tris)
                face.Attributes.SetAttributesTo(newFace.Attributes);
        }

    }
}
