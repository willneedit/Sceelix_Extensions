using System.Collections.Generic;
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
    /// Remaps the UV coordinate space of the individual faces to a region on a target texture
    /// Example: Texture is a charset in a grid pattern, to have the mesh display an individual letter.
    /// </summary>
    [Procedure("8034DF9C-9104-453D-B249-B06D9073AC30", Label = "Mesh UV Atlas")]
    public class MeshUVAtlas : SystemProcedure
    {
        private readonly SingleInput<MeshEntity> _input = new SingleInput<MeshEntity>("Input");
        private readonly Output<MeshEntity> _output = new Output<MeshEntity>("Output");

        /// <summary>
        /// Bounding box inside the target texture
        /// </summary>
        private readonly DoubleParameter _UMin = new DoubleParameter("U Minimum", 0) { MinValue = 0, MaxValue = 1 };
        private readonly DoubleParameter _VMin = new DoubleParameter("V Minimum", 0) { MinValue = 0, MaxValue = 1 };
        private readonly DoubleParameter _UMax = new DoubleParameter("U Maximum", 1) { MinValue = 0, MaxValue = 1 };
        private readonly DoubleParameter _VMax = new DoubleParameter("V Maximum", 1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// Stretch the source UV parameters to (0,0)..(1,1) before remapping
        /// </summary>
        private readonly BoolParameter _stretch = new BoolParameter("Stretch Source", true);


        protected override void Run()
        {
            MeshEntity meshEntity = _input.Read();

            double srcUMin = 9999, srcVMin = 9999;
            double srcUMax = -9999, srcVMax = -9999;

            // Determine the boundaries or set them to (0,0)..(1,1) and treat the UV coordinates as a subset within.
            if(_stretch.Value)
            {
                foreach (Face face in meshEntity)
                {
                    foreach (HalfVertex hv in face.HalfVertices)
                    {
                        if (hv.UV0.X < srcUMin) srcUMin = hv.UV0.X;
                        if (hv.UV0.X > srcUMax) srcUMax = hv.UV0.X;
                        if (hv.UV0.Y < srcVMin) srcVMin = hv.UV0.Y;
                        if (hv.UV0.Y > srcVMax) srcVMax = hv.UV0.Y;
                    }
                }
            }
            else
            {
                srcUMin = 0;
                srcVMin = 0;
                srcUMax = 1;
                srcVMax = 1;
            }

            // Calculate the extent within the target texture
            if(srcUMax == srcUMin || srcVMax == srcVMin)
            {
                Logger.Log("Source UV coordinates do not expand themselves to a face, cannot proceed", Logging.LogType.Error);
                return;
            }

            double dstUStretch = (_UMax.Value - _UMin.Value) / (srcUMax - srcUMin);
            double dstVStretch = (_VMax.Value - _VMin.Value) / (srcVMax - srcVMin);

            // And remap all the UV coordinates.
            foreach (Face face in meshEntity)
            {
                foreach(HalfVertex hv in face.HalfVertices)
                {
                    hv.UV0 = new Vector2D(
                        (float)((hv.UV0.X - srcUMin) * dstUStretch + _UMin.Value),
                        (float)((hv.UV0.Y - srcVMin) * dstVStretch + _VMin.Value)
                    );

                }
            }

            //finally, return the newly create pathentity
            _output.Write(meshEntity);
        }
    }
}
