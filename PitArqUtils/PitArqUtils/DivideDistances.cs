using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PitArqUtils
{
    public class DivideDistances : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DivideDistances()
          : base("Divide Distances", "DivideDistances",
              "Divide a curve with a preset of distances between points.",
              "PitArq", "Division")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve to divide", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distances", "Ds", "Distances between points", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Ps", "Division points", GH_ParamAccess.list);
            pManager.AddVectorParameter("Tangents", "Ts", "Tangent vectors at division points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Parameters", "Params", "Parameters values at division points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declaring variables
            Curve curve = null;
            List<double> distances = new List<double>();

            // Data extraction check
            if (!DA.GetData(0, ref curve)) return;
            if (!DA.GetDataList(1, distances)) return;

            // Data validation
            if (curve == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The curve is null");
                return; 
            }
            if (distances.Count < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The distances array is empty");
                return;
            }

            // Declaring output values
            List<Point3d> points = new List<Point3d>();
            List<Vector3d> tangents = new List<Vector3d>();
            List<double> parameters = new List<double>();


            // Coponent functionality

            var t0 = curve.Domain.Min;
            points.Add(curve.PointAt(t0));

            var sphere_center = curve.PointAt(t0);
            var t = t0;
            var rest_of_curve = curve;

            for (int i = 0; i < distances.Count; i++)
            {
                if (distances[i] <= 0) continue;
                var sphere = new Sphere(sphere_center, distances[i]);
                Curve[] overlap_curves;
                Point3d[] intersect_points;
                var b = Rhino.Geometry.Intersect.Intersection.CurveBrep(rest_of_curve, sphere.ToBrep(), 0.0,
                  out overlap_curves, out intersect_points);
                if (!b || (overlap_curves.Length == 0 && intersect_points.Length == 0))
                    break;
                double intersect_param;
                Point3d intersect_point;
                NextintersectParamAndPoint(overlap_curves, intersect_points, rest_of_curve,
                  out intersect_param, out intersect_point);

                // Assign output values
                points.Add(intersect_point);
                tangents.Add(curve.TangentAt(intersect_param));
                parameters.Add(intersect_param);

                // Prepare values for next loop
                t = intersect_param;
                sphere_center = intersect_point;
                rest_of_curve = curve.Split(t)[1];
            }

            DA.SetDataList(0, points);
            DA.SetDataList(1, tangents);
            DA.SetDataList(2, parameters);
        }

        private static void NextintersectParamAndPoint(Curve[] overlapCurves, Point3d[] intersectPoints,
    Curve curve, out double intersectParam, out Point3d intersectPoint)
        {
            var intersect_params_and_points = new Dictionary<double, Point3d>();
            foreach (var point in intersectPoints)
            {
                double curve_param;
                curve.ClosestPoint(point, out curve_param);
                intersect_params_and_points[curve_param] = point;
            }
            foreach (var overlap_curve in overlapCurves)
            {
                intersect_params_and_points[overlap_curve.Domain.Min] = overlap_curve.PointAt(overlap_curve.Domain.Min);
                intersect_params_and_points[overlap_curve.Domain.Max] = overlap_curve.PointAt(overlap_curve.Domain.Max);
            }
            var min_t = intersect_params_and_points.Keys.Min();
            intersectParam = min_t;
            intersectPoint = intersect_params_and_points[intersectParam];
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
                return Properties.Resources.DivideDistances;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4f99aa42-5fe2-4a2c-a923-a099b1d76aa2"); }
        }
    }
}