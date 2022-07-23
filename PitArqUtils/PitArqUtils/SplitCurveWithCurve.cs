using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PitArqUtils
{
    public class SplitCurveWithCurve : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public SplitCurveWithCurve()
          : base("Split Curve With Curve", "SplitCurveCurve",
              "Split a curve or multiple curves with another closed curve as an outline.",
              "PitArq", "Intersect")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "Base plane for curve projection", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddCurveParameter("Outline", "O", "Closed curve used to split", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curve", "C", "Curve to split", GH_ParamAccess.item);

            // pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Inner curves", "InnC", "Splitted inner curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("Outer curves", "Out", "Splitted outer curves", GH_ParamAccess.list);
            pManager.AddNumberParameter("Intersection parameters", "Ps", "Intersection parameters", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declaring variables
            Plane plane = Plane.WorldXY;
            Curve outline = null;
            Curve crv = null;

            // Data extraction check
            if (!DA.GetData(0, ref plane)) return;
            if (!DA.GetData(1, ref outline)) return;
            if (!DA.GetData(2, ref crv)) return;

            // Data validation
            if(!outline.IsClosed)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The outline curve must be closed");
                return;
            }

            // Declaring output values
            List<Curve> innCrvs = null;
            List<Curve> outCrvs = null;
            innCrvs = new List<Curve>();
            outCrvs = new List<Curve>();


            // Component functionality
            outline.Transform(Transform.PlanarProjection(plane));
            crv.Transform(Transform.PlanarProjection(plane));
            // SplitWithClosedCurve(plane, outline, crv, innCrvs, outCrvs);


            var intersections = Rhino.Geometry.Intersect.Intersection.CurveCurve(outline, crv, 0.001, 0.0);

            List<Curve> resultCrv;
            List<double> parameters = new List<double>();

            if (intersections.Count > 0)
            {
                for (int i = 0; i < intersections.Count; i++)
                {
                    parameters.Add(intersections[i].ParameterB);
                }

                var splitCrvs = crv.Split(parameters).ToList();
                resultCrv = splitCrvs;
            }
            else
            {
                resultCrv = new List<Curve>();
                resultCrv.Add(crv);
            }

            foreach (Curve c in resultCrv)
            {
                var pt = c.PointAtNormalizedLength(0.5);

                if (outline.Contains(pt, plane, 0.001) == PointContainment.Inside)
                {
                    innCrvs.Add(c);
                }
                else
                {
                    outCrvs.Add(c);
                }
            }

            // Assigning the output values
            DA.SetDataList(0, innCrvs);
            DA.SetDataList(1, outCrvs);
            DA.SetDataList(2, parameters);
        }

        private void SplitWithClosedCurve(Plane plane, Curve outline, Curve crv, List<Curve> innC, List<Curve> outC)
        {
            var intersections = Rhino.Geometry.Intersect.Intersection.CurveCurve(outline, crv, 0.001, 0.0);

            List<double> parameters = new List<double>();
            for (int i = 0; i < intersections.Count; i++)
            {
                parameters.Add(intersections[i].ParameterB);
            }

            var splitCrvs = crv.Split(parameters).ToList();

            foreach (Curve c in splitCrvs)
            {
                var pt = c.PointAtNormalizedLength(0.5);

                if (outline.Contains(pt, plane, 0.001) == PointContainment.Inside)
                {
                    innC.Add(c);
                }
                else
                {
                    outC.Add(c);
                }
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4d4422db-025a-45cf-b2f4-842c90c7811e"); }
        }
    }
}