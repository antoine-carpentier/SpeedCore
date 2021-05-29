using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Markup;
using _3DTools;

namespace SpeedCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window

    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //Initialize variables
            double stud_spacing = 1.0;
            double stud_length = .5;
            double wall_thickness = 1.5;
            int studindex = Stud.SelectedIndex;

            if (IsDouble(Spacing.Text))
            {
                stud_spacing = Convert.ToDouble(Spacing.Text)/12.0;
            }

            if (IsDouble(Length.Text))
            {
                stud_length = Convert.ToDouble(Length.Text)/12.0;
            }

            if (IsDouble(WallThickness.Text))
            {
                wall_thickness = Convert.ToDouble(WallThickness.Text)/12.0;
            }

            //Count how many rows/columns of studs are on a 60"x60" plate
            int stud_count = (int)(5 / stud_spacing) + 1;

            //Create for 3D model for the isometric view
            GeometryModel3D SpeedCore1 = new GeometryModel3D();
            MeshGeometry3D scmesh = Create3DScene(stud_spacing,wall_thickness, stud_length, studindex,stud_count);
            SpeedCore1.Geometry = scmesh;
            SpeedCore1.Material = new DiffuseMaterial(new SolidColorBrush(Colors.LightSlateGray));

            //Set up the various views
            IsoView(SpeedCore1);
            SecView(stud_spacing, wall_thickness, stud_length, studindex, stud_count);
            ElView(stud_spacing, wall_thickness, stud_length, studindex, stud_count);

        }

        //Set up Iso view with the 3d model created in Create3DScene
        private void IsoView(GeometryModel3D geometryModel3D)
        {
            DirectionalLight DirLight1 = new DirectionalLight();
            DirLight1.Color = Colors.White;
            DirLight1.Direction = new Vector3D(-1, -1, -1);

            PerspectiveCamera Camera1 = new PerspectiveCamera();
            Camera1.FarPlaneDistance = 20;
            Camera1.NearPlaneDistance = 1;
            Camera1.FieldOfView = 60;
            Camera1.Position = new Point3D(5, 5, 5);
            Camera1.LookDirection = new Vector3D(-1, -1, -1);
            Camera1.UpDirection = new Vector3D(0, 1, 0);

            Model3DGroup modelGroup = new Model3DGroup();
            modelGroup.Children.Add(geometryModel3D);
            modelGroup.Children.Add(DirLight1);
            ModelVisual3D modelsVisual = new ModelVisual3D();
            modelsVisual.Content = modelGroup;

            //Clear the canvas of any content to allow updating of the model
            if (this.CanvasIso.Children.Count > 0)
            {
                this.CanvasIso.Children.Clear();
            }

            Viewport3D myViewport = new Viewport3D();
            myViewport.Camera = Camera1;
            myViewport.Children.Add(modelsVisual);
            this.CanvasIso.Children.Add(myViewport);
            myViewport.Height = 360;
            myViewport.Width = 360;

            Canvas.SetTop(myViewport, 0);
            Canvas.SetLeft(myViewport, 0);

            //Set up the mouse trackball to rotate 3d model
            Trackball trackball = new Trackball();
            trackball.EventSource = BorderIso;
            myViewport.Camera.Transform = trackball.Transform;
        }

        //Set up section view with the variable specified by user
        private void SecView(double spacing, double thickness, double length, int studindex, int stud_count)
        {
            double head_diameter = 0.0;
            double head_length = 0.0;
            double stud_diameter = 0.0;
            double scale_factor = 360.0 / 7.0;

            switch (studindex)
            {
                case 0:
                    head_diameter = .5 / 12.0;
                    head_length = .187 / 12.0;
                    stud_diameter = .25 / 12.0;
                    Console.WriteLine("Stud 1/4");
                    break;

                case 1:
                    head_diameter = .75 / 12.0;
                    head_length = .281 / 12.0;
                    stud_diameter = .375 / 12.0;
                    Console.WriteLine("Stud 3/8");
                    break;

                case 2:
                    head_diameter = 1 / 12.0;
                    head_length = .281 / 12.0;
                    stud_diameter = .5 / 12.0;
                    Console.WriteLine("Stud 1/2");
                    break;

                case 3:
                    head_diameter = 1.25 / 12.0;
                    head_length = .312 / 12.0;
                    stud_diameter = .625 / 12.0;
                    Console.WriteLine("Stud 5/8");
                    break;

                default:
                    Console.WriteLine("Stud not selected");
                    break;
            }

            //Create the stud shape 
            PointCollection stud = new PointCollection();
            stud.Add(new Point(0,0));
            stud.Add(new Point(scale_factor*head_length, 0));
            stud.Add(new Point(scale_factor*head_length, (scale_factor*head_diameter - scale_factor * stud_diameter)/2.0));
            stud.Add(new Point(scale_factor * length, (scale_factor * head_diameter - scale_factor * stud_diameter) / 2.0));
            stud.Add(new Point(scale_factor * length, (scale_factor * head_diameter + scale_factor * stud_diameter) / 2.0));
            stud.Add(new Point(scale_factor * head_length, (scale_factor * head_diameter + scale_factor * stud_diameter) / 2.0));
            stud.Add(new Point(scale_factor * head_length, scale_factor * head_diameter));
            stud.Add(new Point(0, scale_factor * head_diameter));

            //create the plate shape (a basic rectangle)
            PointCollection plate = new PointCollection();
            plate.Add(new Point(0, 0));
            plate.Add(new Point(scale_factor*0.083, 0));
            plate.Add(new Point(scale_factor * 0.083, scale_factor * 5.2));
            plate.Add(new Point(0, scale_factor * 5.2));

            //create the plate polygons using the point from the point collection above
            Polygon myplateleft = new Polygon();
            myplateleft.Points = plate;
            myplateleft.Fill = Brushes.LightSlateGray;
            myplateleft.Stretch = Stretch.Fill;
            myplateleft.Stroke = Brushes.Black;
            myplateleft.StrokeThickness = 1;

            //polygons cannot be reused by canvas, so need to create a duplicate polygon for the second plate
            Polygon myplateright = new Polygon();
            myplateright.Points = plate;
            myplateright.Fill = Brushes.LightSlateGray;
            myplateright.Stretch = Stretch.Fill;
            myplateright.Stroke = Brushes.Black;
            myplateright.StrokeThickness = 1;

            //create the right stud polygon using the point from the point collection above
            Polygon mystudright = new Polygon();
            mystudright.Points = stud;
            mystudright.Fill = Brushes.LightSlateGray;
            mystudright.Stretch = Stretch.Fill;
            mystudright.Stroke = Brushes.Black;
            mystudright.StrokeThickness = 1;

            //create the right stud polygon using the point from the point collection above, then flip it vertically
            Polygon mystudleft = new Polygon();
            mystudleft.Points = stud;
            mystudleft.Fill = Brushes.LightSlateGray;
            mystudleft.Stretch = Stretch.Fill;
            mystudleft.Stroke = Brushes.Black;
            mystudleft.StrokeThickness = 1;

            ScaleTransform flipstud = new ScaleTransform(-1.0, 1.0);
            mystudleft.RenderTransform = flipstud;

            //Clear the canvas of any content to allow updating of the model
            if (this.CanvasSec.Children.Count > 0)
            {
                this.CanvasSec.Children.Clear();
            }
 
            //add one left and one right stud to the canvas. They will be used to create the other studs
            this.CanvasSec.Children.Add(mystudleft);
            Canvas.SetLeft(mystudleft, 180 - scale_factor * (thickness / 2.0 - length - 1.3 * 0.083));
            Canvas.SetTop(mystudleft, 180 - scale_factor * ((0 - (stud_count - 1) / 2.0) * spacing + head_diameter / 2.0));

            this.CanvasSec.Children.Add(mystudright);
            Canvas.SetLeft(mystudright, 180 + scale_factor * (thickness / 2.0 - length - 0.1 * 0.083));
            Canvas.SetTop(mystudright, 180 - scale_factor * ((0 - (stud_count - 1) / 2.0) * spacing + head_diameter / 2.0));

            //create copies of the two placed studs as new UIElements and add them to the canvas until we reach the expected stud row count
            for (int i = 1; i <= stud_count - 1; i++)
            {
                Polygon mystudleft2 = XamlReader.Parse(XamlWriter.Save(mystudleft)) as Polygon;
                this.CanvasSec.Children.Add(mystudleft2);
                Canvas.SetLeft(mystudleft2, 180 - scale_factor * (thickness / 2.0 - length - 1.3*0.083));
                Canvas.SetTop(mystudleft2, 180 - scale_factor * ((i - (stud_count - 1) / 2.0) * spacing + head_diameter / 2.0));

                Polygon mystudright2 = XamlReader.Parse(XamlWriter.Save(mystudright)) as Polygon;
                this.CanvasSec.Children.Add(mystudright2);
                Canvas.SetLeft(mystudright2, 180 + scale_factor * (thickness / 2.0 - length - 0.1 * 0.083));
                Canvas.SetTop(mystudright2, 180 - scale_factor * ((i - (stud_count - 1) / 2.0) * spacing + head_diameter / 2.0));
            }

            //add the left and right plates to the canvas
            this.CanvasSec.Children.Add(myplateleft);
            Canvas.SetLeft(myplateleft, 180-scale_factor*thickness/2.0);
            Canvas.SetTop(myplateleft, 180-scale_factor*2.6);

            this.CanvasSec.Children.Add(myplateright);
            Canvas.SetLeft(myplateright, 180 + scale_factor * thickness / 2.0);
            Canvas.SetTop(myplateright, 180 - scale_factor * 2.6);

        }

        //Set up the elevation view
        private void ElView(double spacing, double thickness, double length, int studindex, int stud_count)
        {
            double head_diameter = 0.0;
            double scale_factor = 360.0 / 7.0;

            switch (studindex)
            {
                case 0:
                    head_diameter = .5 / 12.0;
                    Console.WriteLine("Stud 1/4");
                    break;

                case 1:
                    head_diameter = .75 / 12.0;
                    Console.WriteLine("Stud 3/8");
                    break;

                case 2:
                    head_diameter = 1 / 12.0;
                    Console.WriteLine("Stud 1/2");
                    break;

                case 3:
                    head_diameter = 1.25 / 12.0;
                    Console.WriteLine("Stud 5/8");
                    break;

                default:
                    Console.WriteLine("Stud not selected");
                    break;
            }

            //Create a point collection for the plate. A simple 5.2'x5.2' plate 
            PointCollection plate = new PointCollection();
            plate.Add(new Point(0, 0));
            plate.Add(new Point(scale_factor * 5.2, 0));
            plate.Add(new Point(scale_factor * 5.2, scale_factor * 5.2));
            plate.Add(new Point(0, scale_factor * 5.2));

            Polygon myplate = new Polygon();
            myplate.Points = plate;
            myplate.Fill = Brushes.LightSlateGray;
            myplate.Stretch = Stretch.Fill;
            myplate.Stroke = Brushes.Black;
            myplate.StrokeThickness = 2;

            //The stud will be basic circles
            Ellipse stud = new Ellipse();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Colors.SlateGray;
            stud.Fill = mySolidColorBrush;
            stud.StrokeThickness = 1;
            stud.Stroke = Brushes.Black;

            // Set the diameter of the studs. They are scale 1.5x as it would otherwise be very small for some
            stud.Width = 1.5*scale_factor*head_diameter;
            stud.Height = 1.5*scale_factor * head_diameter;


            if (this.CanvasEl.Children.Count > 0)
            {
                this.CanvasEl.Children.Clear();
            }

            //add the plate to the canvas and center it
            this.CanvasEl.Children.Add(myplate);
            Canvas.SetLeft(myplate, 180 - scale_factor * 2.6);
            Canvas.SetTop(myplate, 180 - scale_factor * 2.6);

            //Add the circles (studs) to the canvas
            for (int i = 0; i <= stud_count - 1; i++)
            {
                Ellipse stud2 = XamlReader.Parse(XamlWriter.Save(stud)) as Ellipse;
                this.CanvasEl.Children.Add(stud2);

                Canvas.SetLeft(stud2, 180 - scale_factor * ((i - (stud_count - 1) / 2.0) * spacing + head_diameter / 2.0));
                Canvas.SetTop(stud2, 180 - scale_factor * ((0 - (stud_count - 1) / 2.0) * spacing + head_diameter / 2.0));

                for (int j = 0; j <= stud_count - 1; j++)
                {
                    Ellipse stud3 = XamlReader.Parse(XamlWriter.Save(stud)) as Ellipse;
                    this.CanvasEl.Children.Add(stud3);

                    Canvas.SetLeft(stud3, 180 - scale_factor * ((i - (stud_count - 1) / 2.0) * spacing + head_diameter / 2.0));
                    Canvas.SetTop(stud3, 180 - scale_factor * ((j - (stud_count - 1) / 2.0) * spacing + head_diameter / 2.0));
                }
            }
        }

        //Set up the 3d model for use by the isometric view
        MeshGeometry3D Create3DScene(double spacing, double thickness, double length, int studindex, int stud_count)
        {
            MeshGeometry3D cylinder = new MeshGeometry3D();

            double head_diameter = 0.0;
            double head_length = 0.0;
            double stud_diameter = 0.0;

            Point3D stud_pointtrue = new Point3D(0, 0, 0);
            Point3D stud_pointfalse = new Point3D(0, 0, 0);

            switch (studindex)
            {
                case 0:
                    head_diameter = .5 / 12.0;
                    head_length = .187 / 12.0;
                    stud_diameter = .25 / 12.0;
                    Console.WriteLine("Stud 1/4");
                    break;

                case 1:
                    head_diameter = .75 / 12.0;
                    head_length = .281 / 12.0;
                    stud_diameter = .375 / 12.0;
                    Console.WriteLine("Stud 3/8");
                    break;

                case 2:
                    head_diameter = 1 / 12.0;
                    head_length = .281 / 12.0;
                    stud_diameter = .5 / 12.0;
                    Console.WriteLine("Stud 1/2");
                    break;

                case 3:
                    head_diameter = 1.25 / 12.0;
                    head_length = .312 / 12.0;
                    stud_diameter = .625 / 12.0;
                    Console.WriteLine("Stud 5/8");
                    break;

                default:
                    Console.WriteLine("Stud not selected");
                    break;
            }

            //create the 2 parallelograms that represent the steel plates
            AddPlates(cylinder, 0.083, thickness);

            //add studs to the 3d model
            //2 cases to consider: an odd amount of stud rows => there will be studs at the very center of the plates
            // an even amount of stud rows => the center of the plates will be at a spacing/2 distance of the closest studs
            if (stud_count % 2 == 0)
            {
                Console.WriteLine(stud_count);

                for (int i = 0; i <= stud_count - 1; i++)
                {
                    stud_pointtrue = new Point3D(-thickness / 2.0 + 0.0415, 0 + (i - (stud_count - 1) / 2.0) * spacing, 0 - ((stud_count - 1) / 2.0) * spacing);
                    AddStud(cylinder, stud_pointtrue, length, head_length, head_diameter, stud_diameter, true);
                    stud_pointfalse = new Point3D(thickness / 2.0 - 0.0415, 0 - (i - (stud_count - 1) / 2.0) * spacing, 0 - ((stud_count - 1) / 2.0) * spacing);
                    AddStud(cylinder, stud_pointfalse, length, head_length, head_diameter, stud_diameter, false);


                    for (int j = 0; j <= stud_count - 2; j++)
                    {
                        stud_pointtrue = new Point3D(stud_pointtrue.X, stud_pointtrue.Y, stud_pointtrue.Z + spacing);
                        AddStud(cylinder, stud_pointtrue, length, head_length, head_diameter, stud_diameter, true);
                        stud_pointfalse = new Point3D(stud_pointfalse.X, stud_pointfalse.Y, stud_pointfalse.Z + spacing);
                        AddStud(cylinder, stud_pointfalse, length, head_length, head_diameter, stud_diameter, false);
                    }
                }
            }
            else
            {

                Console.WriteLine(stud_count);

                for (int i = 0; i <= stud_count - 1; i++)
                {
                    stud_pointtrue = new Point3D(-thickness / 2.0 + 0.0415, 0 + (i - stud_count / 2) * spacing, 0 - (stud_count / 2) * spacing);
                    AddStud(cylinder, stud_pointtrue, length, head_length, head_diameter, stud_diameter, true);
                    stud_pointfalse = new Point3D(thickness / 2.0 - 0.0415, 0 - (i - stud_count / 2) * spacing, 0 - (stud_count / 2) * spacing);
                    AddStud(cylinder, stud_pointfalse, length, head_length, head_diameter, stud_diameter, false);


                    for (int j = 0; j <= stud_count - 2; j++)
                    {
                        stud_pointtrue = new Point3D(stud_pointtrue.X, stud_pointtrue.Y, stud_pointtrue.Z + spacing);
                        AddStud(cylinder, stud_pointtrue, length, head_length, head_diameter, stud_diameter, true);
                        stud_pointfalse = new Point3D(stud_pointfalse.X, stud_pointfalse.Y, stud_pointfalse.Z + spacing);
                        AddStud(cylinder, stud_pointfalse, length, head_length, head_diameter, stud_diameter, false);
                    }
                }
            }

            return cylinder;
        }

        // Add a 5x5xthickness plate.
        private void AddPlates(MeshGeometry3D mesh, double thickness, double spacing)
        {
            //Since there may be already existing models in the mesh, we need to know how many points are already defined 
            Int32 offset = mesh.Positions.Count;
            Point3DCollection corners = new Point3DCollection
            {
                new Point3D(-spacing / 2 + thickness / 2, 2.6, 2.6),
                new Point3D(-spacing / 2 - thickness / 2, 2.6, 2.6),
                new Point3D(-spacing / 2 - thickness / 2, -2.6, 2.6),
                new Point3D(-spacing / 2 + thickness / 2, -2.6, 2.6),
                new Point3D(-spacing / 2 + thickness / 2, 2.6, -2.6),
                new Point3D(-spacing / 2 - thickness / 2, 2.6, -2.6),
                new Point3D(-spacing / 2 - thickness / 2, -2.6, -2.6),
                new Point3D(-spacing / 2 + thickness / 2, -2.6, -2.6)
            };

            foreach (Point3D point in corners)
            {
                mesh.Positions.Add(point);
            }

            //this will be used to create the triangles (that's why we need to know the offset, so that the triangles are using the correct points)
            Int32[] indices ={
            //front
              offset+0,offset+1,offset+2,
              offset+0,offset+2,offset+3,
            //back
              offset+4,offset+7,offset+6,
              offset+4,offset+6,offset+5,
            //Right
              offset+4,offset+0,offset+3,
              offset+4,offset+3,offset+7,
           //Left
              offset+1,offset+5,offset+6,
              offset+1,offset+6,offset+2,
           //Top
              offset+1,offset+0,offset+4,
              offset+1,offset+4,offset+5,
           //Bottom
              offset+2,offset+6,offset+7,
              offset+2,offset+7,offset+3
            };

            foreach (Int32 index in indices)
            {

                mesh.TriangleIndices.Add(index);
            }

            //same thing, with the second plate this time
            offset = mesh.Positions.Count;
            corners = new Point3DCollection
            {
                new Point3D(spacing / 2 + thickness / 2, 2.6, 2.6),
                new Point3D(spacing / 2 - thickness / 2, 2.6, 2.6),
                new Point3D(spacing / 2 - thickness / 2, -2.6, 2.6),
                new Point3D(spacing / 2 + thickness / 2, -2.6, 2.6),
                new Point3D(spacing / 2 + thickness / 2, 2.6, -2.6),
                new Point3D(spacing / 2 - thickness / 2, 2.6, -2.6),
                new Point3D(spacing / 2 - thickness / 2, -2.6, -2.6),
                new Point3D(spacing / 2 + thickness / 2, -2.6, -2.6)
            };

            foreach (Point3D point in corners)
            {
                mesh.Positions.Add(point);
            }

            Int32[] indices2 = {
            //front
              offset+0,offset+1,offset+2,
              offset+0,offset+2,offset+3,
            //back
              offset+4,offset+7,offset+6,
              offset+4,offset+6,offset+5,
            //Right
              offset+4,offset+0,offset+3,
              offset+4,offset+3,offset+7,
           //Left
              offset+1,offset+5,offset+6,
              offset+1,offset+6,offset+2,
           //Top
              offset+1,offset+0,offset+4,
              offset+1,offset+4,offset+5,
           //Bottom
              offset+2,offset+6,offset+7,
              offset+2,offset+7,offset+3
            };

            foreach (Int32 index in indices2)
            {

                mesh.TriangleIndices.Add(index);
            }
        }

        //Create 3d model of studs
        private void AddStud(MeshGeometry3D mesh, Point3D base_point, double length, double head_length, double head_diameter, double stud_diameter, bool direction)
        {
            //true and false are used to know if the studs are facing left or facing right
            if (direction == true)
            {
                Point3D junction_point = new Point3D(base_point.X + length - head_length, base_point.Y, base_point.Z);
                Vector3D axis1 = new Vector3D(length - head_length, 0, 0);
                Vector3D axis2 = new Vector3D(head_length, 0, 0);

                //a stud is made of 2 cylinder of different size touching at the junction point
                AddCylinder(mesh, base_point, axis1, stud_diameter, 30);
                AddCylinder(mesh, junction_point, axis2, head_diameter, 30);
            }
            else
            {
                Point3D junction_point = new Point3D(base_point.X - length + head_length, base_point.Y, base_point.Z);
                Vector3D axis1 = new Vector3D(-length + head_length, 0, 0);
                Vector3D axis2 = new Vector3D(-head_length, 0, 0);

                AddCylinder(mesh, base_point, axis1, stud_diameter, 30);
                AddCylinder(mesh, junction_point, axis2, head_diameter, 30);
            }
        }

        //Create a 3d cylinder model (used for studs)
        private void AddCylinder(MeshGeometry3D mesh, Point3D end_point, Vector3D axis, double radius, int num_sides)
        {
            // Get two vectors perpendicular to the axis.
            Vector3D v1 = new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y);
            Vector3D v2 = Vector3D.CrossProduct(v1, axis);

            // Make the vectors have length radius.
            v1 *= (radius / v1.Length);
            v2 *= (radius / v2.Length);

            // Make the top end cap.
            double theta = 0;
            double dtheta = 2 * Math.PI / num_sides;

            //create triangle. The cylinder is in fact a multi-faceted extruded polygon
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                theta += dtheta;
                Point3D p2 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                AddTriangle(mesh, end_point, p2, p1);
                AddTriangle(mesh, p1, p2, end_point);
            }

            // Make the bottom end cap.
            Point3D end_point2 = end_point + axis;
            theta = 0;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point2 +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                theta += dtheta;
                Point3D p2 = end_point2 +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                AddTriangle(mesh, p1, p2, end_point2);
                AddTriangle(mesh, end_point2, p2, p1);
            }

            // Make the sides.
            theta = 0;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                theta += dtheta;
                Point3D p2 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;

                Point3D p3 = p1 + axis;
                Point3D p4 = p2 + axis;

                AddTriangle(mesh, p1, p3, p2);
                AddTriangle(mesh, p2, p3, p4);
            }
        }
        
        //Create the triangles that are used by 3d models
        private void AddTriangle(MeshGeometry3D mesh, Point3D p1, Point3D p2, Point3D p3)
        {
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);

            mesh.TriangleIndices.Add(mesh.Positions.Count - 3);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 2);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 1);

        }

        //Check whether the user input is a double or not
        public static bool IsDouble(string ValueToTest)
        {
            double Test;
            bool OutPut;
            OutPut = double.TryParse(ValueToTest, out Test);
            return OutPut;
        }
    }

}
