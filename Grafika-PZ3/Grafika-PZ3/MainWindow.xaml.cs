using Grafika_PZ3.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Grafika_PZ3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<SubstationEntity> substationEntities = new List<SubstationEntity>();
        List<NodeEntity> nodeEntities = new List<NodeEntity>();
        List<SwitchEntity> switchEntities = new List<SwitchEntity>();
        List<LineEntity> lineEntities = new List<LineEntity>();

        string mouseCaptureCase = "";
        System.Windows.Point beforeMousePosition;

        private GeometryModel3D hitgeo;

        private ToolTip toolTip = new ToolTip();

        private ArrayList models = new ArrayList();

        GeometryModel3D changeBackFirst;
        GeometryModel3D changeBackSecond;
        DiffuseMaterial changeBackFirstTexture;
        DiffuseMaterial changeBackSecondTexture;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var filename = "Geographic.xml";
            var currentDirectory = Directory.GetCurrentDirectory();
            var purchaseOrderFilepath = System.IO.Path.Combine(currentDirectory, filename);

            //Load xml
            XDocument xdoc = XDocument.Load(filename);

            substationEntities = xdoc.Descendants("SubstationEntity")
                     .Select(sub => new SubstationEntity
                     {
                         Id = (long)sub.Element("Id"),
                         Name = (string)sub.Element("Name"),
                         X = (double)sub.Element("X"),
                         Y = (double)sub.Element("Y"),
                     }).ToList();


            nodeEntities = xdoc.Descendants("NodeEntity")
                     .Select(node => new NodeEntity
                     {
                         Id = (long)node.Element("Id"),
                         Name = (string)node.Element("Name"),
                         X = (double)node.Element("X"),
                         Y = (double)node.Element("Y"),
                     }).ToList();


            switchEntities = xdoc.Descendants("SwitchEntity")
                     .Select(sw => new SwitchEntity
                     {
                         Id = (long)sw.Element("Id"),
                         Name = (string)sw.Element("Name"),
                         Status = (string)sw.Element("Status"),
                         X = (double)sw.Element("X"),
                         Y = (double)sw.Element("Y"),
                     }).ToList();


            lineEntities = xdoc.Descendants("LineEntity")
                     .Select(line => new LineEntity
                     {
                         Id = (long)line.Element("Id"),
                         Name = (string)line.Element("Name"),
                         ConductorMaterial = (string)line.Element("ConductorMaterial"),
                         IsUnderground = (bool)line.Element("IsUnderground"),
                         R = (float)line.Element("R"),
                         FirstEnd = (long)line.Element("FirstEnd"),
                         SecondEnd = (long)line.Element("SecondEnd"),
                         LineType = (string)line.Element("LineType"),
                         ThermalConstantHeat = (long)line.Element("ThermalConstantHeat"),
                         Vertices = line.Element("Vertices").Descendants("Point").Select(p => new Models.Point
                         {
                             X = (double)p.Element("X"),
                             Y = (double)p.Element("Y"),
                         }).ToList()
                     }).ToList();


            double newX;
            double newY;

            foreach (SubstationEntity s in substationEntities)
            {
                ToLatLon(s.X, s.Y, 34, out newY, out newX);
                s.X = newX;
                s.Y = newY;
            }

            foreach (NodeEntity n in nodeEntities)
            {
                ToLatLon(n.X, n.Y, 34, out newY, out newX);
                n.X = newX;
                n.Y = newY;
            }

            foreach (SwitchEntity s in switchEntities)
            {
                ToLatLon(s.X, s.Y, 34, out newY, out newX);
                s.X = newX;
                s.Y = newY;
            }

            foreach (LineEntity l in lineEntities)
            {
                foreach (Models.Point p in l.Vertices)
                {
                    ToLatLon(p.X, p.Y, 34, out newY, out newX);
                    p.X = newX;
                    p.Y = newY;
                }
            }

            //Remove objects outside map
            for (int i = 0; i < substationEntities.Count; i++)
            {
                if(substationEntities[i].X > 19.793909 && substationEntities[i].Y > 45.2325 && substationEntities[i].X < 19.894459 && substationEntities[i].Y < 45.277031)
                {
                    continue;
                }
                else
                {
                    substationEntities.Remove(substationEntities[i]);
                    i--;
                }
            }

            for (int i = 0; i < nodeEntities.Count; i++)
            {
                if (nodeEntities[i].X > 19.793909 && nodeEntities[i].Y > 45.2325 && nodeEntities[i].X < 19.894459 && nodeEntities[i].Y < 45.277031)
                {
                    continue;
                }
                else
                {
                    nodeEntities.Remove(nodeEntities[i]);
                    i--;
                }
            }

            for (int i = 0; i < switchEntities.Count; i++)
            {
                if (switchEntities[i].X > 19.793909 && switchEntities[i].Y > 45.2325 && switchEntities[i].X < 19.894459 && switchEntities[i].Y < 45.277031)
                {
                    continue;
                }
                else
                {
                    switchEntities.Remove(switchEntities[i]);
                    i--;
                }
            }

            for (int i = 0; i < lineEntities.Count; i++)
            {
                for(int j = 0; j < lineEntities[i].Vertices.Count; j++)
                {
                    if (lineEntities[i].Vertices[j].X > 19.793909 && lineEntities[i].Vertices[j].Y > 45.2325 && lineEntities[i].Vertices[j].X < 19.894459 && lineEntities[i].Vertices[j].Y < 45.277031)
                    {
                        continue;
                    }
                    else
                    {
                        lineEntities[i].Vertices.Remove(lineEntities[i].Vertices[j]);
                        j--;
                    }
                }

                if(lineEntities[i].Vertices.Count <= 1)
                {
                    lineEntities.Remove(lineEntities[i]);
                    i--;
                }
            }

            //Scaling
            double smallestX = 19.793909;
            double smallestY = 45.2325;

            double largestX = 19.894459;
            double largestY = 45.277031;

            double scaleX = (largestX - smallestX) / 10;
            double scaleY = (largestY - smallestY) / 10;

            for (int i = 0; i < substationEntities.Count; i++)
            {
                substationEntities[i].X -= smallestX;
                substationEntities[i].Y -= smallestY;

                substationEntities[i].X /= scaleX;
                substationEntities[i].Y /= scaleY;

                if (substationEntities[i].X <= 0 || substationEntities[i].X >= 10 || substationEntities[i].Y <= 0 || substationEntities[i].Y >= 10)
                {
                    substationEntities.Remove(substationEntities[i]);
                }
            }

            for (int i = 0; i < nodeEntities.Count; i++)
            {
                nodeEntities[i].X -= smallestX;
                nodeEntities[i].Y -= smallestY;

                nodeEntities[i].X /= scaleX;
                nodeEntities[i].Y /= scaleY;
            }

            for (int i = 0; i < switchEntities.Count; i++)
            {
                switchEntities[i].X -= smallestX;
                switchEntities[i].Y -= smallestY;

                switchEntities[i].X /= scaleX;
                switchEntities[i].Y /= scaleY;
            }

            for (int i = 0; i < lineEntities.Count; i++)
            {
                for (int j = 0; j < lineEntities[i].Vertices.Count; j++)
                {
                    lineEntities[i].Vertices[j].X -= smallestX;
                    lineEntities[i].Vertices[j].Y -= smallestY;

                    lineEntities[i].Vertices[j].X /= scaleX;
                    lineEntities[i].Vertices[j].Y /= scaleY;
                }

                if(lineEntities[i].Vertices.Count <= 1)
                {
                    lineEntities.Remove(lineEntities[i]);
                }
            }

            DrawCubes();
            DrawLines();
        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        private void mainViewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            /*
            scale.CenterX = e.GetPosition(mainViewport).X;
            scale.CenterY = e.GetPosition(mainViewport).Y;
            */

            scale.CenterX = 5;
            scale.CenterY = 5;

            if (e.Delta > 0)
            {
                scale.ScaleX += 0.2;
                scale.ScaleY += 0.2;
                scale.ScaleZ += 0.2;
            }
            else
            {
                scale.ScaleX -= 0.2;
                scale.ScaleY -= 0.2;
                scale.ScaleZ -= 0.2;
            }
        }

        private void mainViewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                if (changeBackFirst != null)
                {
                    changeBackFirst.Material = changeBackFirstTexture;
                }

                if (changeBackSecond != null)
                {
                    changeBackSecond.Material = changeBackSecondTexture;
                }

                toolTip.IsOpen = false;

                mainViewport.CaptureMouse();
                mouseCaptureCase = "translation";
                beforeMousePosition = e.GetPosition(mainViewport);

                System.Windows.Point mouseposition = e.GetPosition(mainViewport);
                Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
                Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);

                PointHitTestParameters pointparams =
                         new PointHitTestParameters(mouseposition);
                RayHitTestParameters rayparams =
                         new RayHitTestParameters(testpoint3D, testdirection);

                //test for a result in the Viewport3D     
                hitgeo = null;
                VisualTreeHelper.HitTest(mainViewport, null, HTResult, pointparams);
            }

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                mainViewport.CaptureMouse();
                mouseCaptureCase = "rotation";
                beforeMousePosition = e.GetPosition(mainViewport);
            }
        }

        private void mainViewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mainViewport.ReleaseMouseCapture();
        }

        private void mainViewport_MouseMove(object sender, MouseEventArgs e)
        {
            if(mainViewport.IsMouseCaptured)
            {
                if(mouseCaptureCase == "translation")
                {
                    translation.OffsetX += (e.GetPosition(mainViewport).X - beforeMousePosition.X) / 2000;
                    translation.OffsetY += (beforeMousePosition.Y - e.GetPosition(mainViewport).Y) / 2000;
                }

                if(mouseCaptureCase == "rotation")
                {
                    rotateX.Angle += (e.GetPosition(mainViewport).X - beforeMousePosition.X) / 250;
                    rotateY.Angle += (e.GetPosition(mainViewport).Y - beforeMousePosition.Y) / 250;
                }
            }
        }

        private void DrawCubes()
        {
            #region switches
            foreach (SubstationEntity s in substationEntities)
            {
                MeshGeometry3D mesh = new MeshGeometry3D();

                mesh.Positions.Add(new Point3D(s.X, s.Y, 0.05));
                mesh.Positions.Add(new Point3D(s.X + 0.05, s.Y, 0.05));
                mesh.Positions.Add(new Point3D(s.X, s.Y, 0.05 + 0.05));
                mesh.Positions.Add(new Point3D(s.X + 0.05, s.Y, 0.05 + 0.05));

                mesh.Positions.Add(new Point3D(s.X, s.Y + 0.05, 0.05));
                mesh.Positions.Add(new Point3D(s.X + 0.05, s.Y + 0.05, 0.05));
                mesh.Positions.Add(new Point3D(s.X, s.Y + 0.05, 0.05 + 0.05));
                mesh.Positions.Add(new Point3D(s.X + 0.05, s.Y + 0.05, 0.05 + 0.05));

                for(int i = 0; i < map3dGroup.Children.Count; i++)
                {
                    if(map3dGroup.Children[i].Bounds.IntersectsWith(mesh.Bounds))
                    {
                        for( int j = 0; j < mesh.Positions.Count; j++)
                        {
                            mesh.Positions[j] = new Point3D(mesh.Positions[j].X, mesh.Positions[j].Y, mesh.Positions[j].Z + 0.1);
                        }
                    }
                }
                //forward face
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(2);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(3);

                //down face
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(1);

                //up face
                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(6);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(7);

                //right face
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(7);

                //left face
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(6);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(2);

                //back face
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(6);

                DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));

                GeometryModel3D cube = new GeometryModel3D(mesh, material);

                cube.SetValue(FrameworkElement.TagProperty, s);

                map3dGroup.Children.Add(cube);

                models.Add(cube);
            }
            #endregion

            #region nodes
            foreach (NodeEntity n in nodeEntities)
            {
                MeshGeometry3D mesh = new MeshGeometry3D();

                mesh.Positions.Add(new Point3D(n.X, n.Y, 0.05));
                mesh.Positions.Add(new Point3D(n.X + 0.05, n.Y, 0.05));
                mesh.Positions.Add(new Point3D(n.X, n.Y, 0.05 + 0.05));
                mesh.Positions.Add(new Point3D(n.X + 0.05, n.Y, 0.05 + 0.05));

                mesh.Positions.Add(new Point3D(n.X, n.Y + 0.05, 0.05));
                mesh.Positions.Add(new Point3D(n.X + 0.05, n.Y + 0.05, 0.05));
                mesh.Positions.Add(new Point3D(n.X, n.Y + 0.05, 0.05 + 0.05));
                mesh.Positions.Add(new Point3D(n.X + 0.05, n.Y + 0.05, 0.05 + 0.05));

                for (int i = 0; i < map3dGroup.Children.Count; i++)
                {
                    if (map3dGroup.Children[i].Bounds.IntersectsWith(mesh.Bounds))
                    {
                        for (int j = 0; j < mesh.Positions.Count; j++)
                        {
                            mesh.Positions[j] = new Point3D(mesh.Positions[j].X, mesh.Positions[j].Y, mesh.Positions[j].Z + 0.1);
                        }
                    }
                }
                //forward face
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(2);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(3);

                //down face
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(1);

                //up face
                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(6);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(7);

                //right face
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(7);

                //left face
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(6);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(2);

                //back face
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(6);

                DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));

                GeometryModel3D cube = new GeometryModel3D(mesh, material);

                cube.SetValue(FrameworkElement.TagProperty, n);

                map3dGroup.Children.Add(cube);

                models.Add(cube);
            }
            #endregion

            #region switches
            foreach (SwitchEntity s in switchEntities)
            {
                MeshGeometry3D mesh = new MeshGeometry3D();

                mesh.Positions.Add(new Point3D(s.X, s.Y, 0.05));
                mesh.Positions.Add(new Point3D(s.X + 0.05, s.Y, 0.05));
                mesh.Positions.Add(new Point3D(s.X, s.Y, 0.05 + 0.05));
                mesh.Positions.Add(new Point3D(s.X + 0.05, s.Y, 0.05 + 0.05));

                mesh.Positions.Add(new Point3D(s.X, s.Y + 0.05, 0.05));
                mesh.Positions.Add(new Point3D(s.X + 0.05, s.Y + 0.05, 0.05));
                mesh.Positions.Add(new Point3D(s.X, s.Y + 0.05, 0.05 + 0.05));
                mesh.Positions.Add(new Point3D(s.X + 0.05, s.Y + 0.05, 0.05 + 0.05));

                for (int i = 0; i < map3dGroup.Children.Count; i++)
                {
                    if (map3dGroup.Children[i].Bounds.IntersectsWith(mesh.Bounds))
                    {
                        for (int j = 0; j < mesh.Positions.Count; j++)
                        {
                            mesh.Positions[j] = new Point3D(mesh.Positions[j].X, mesh.Positions[j].Y, mesh.Positions[j].Z + 0.1);
                        }
                    }
                }
                //forward face
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(2);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(3);

                //down face
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(1);

                //up face
                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(6);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(7);

                //right face
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(7);

                //left face
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(6);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(2);

                //back face
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(6);

                DiffuseMaterial material;

                if(s.Status == "Open")
                {
                    material = new DiffuseMaterial(new SolidColorBrush(Colors.Green));
                }
                else
                {
                    material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                }

                GeometryModel3D cube = new GeometryModel3D(mesh, material);

                cube.SetValue(FrameworkElement.TagProperty, s);

                map3dGroup.Children.Add(cube);

                models.Add(cube);
            }
            #endregion
        }

        private void DrawLines()
        {
            foreach(LineEntity l in lineEntities)
            {
                for(int i = 0; i < l.Vertices.Count-1; i++)
                {
                    MeshGeometry3D mesh = new MeshGeometry3D();

                    mesh.Positions.Add(new Point3D(l.Vertices[i].X, l.Vertices[i].Y, 0.025)); ;
                    mesh.Positions.Add(new Point3D(l.Vertices[i].X + 0.025, l.Vertices[i].Y, 0.025));
                    mesh.Positions.Add(new Point3D(l.Vertices[i].X, l.Vertices[i].Y, 0.025 + 0.025));
                    mesh.Positions.Add(new Point3D(l.Vertices[i].X + 0.025, l.Vertices[i].Y, 0.025 + 0.025));

                    mesh.Positions.Add(new Point3D(l.Vertices[i + 1].X, l.Vertices[i + 1].Y + 0.025, 0.025));
                    mesh.Positions.Add(new Point3D(l.Vertices[i + 1].X + 0.025, l.Vertices[i + 1].Y + 0.025, 0.025));
                    mesh.Positions.Add(new Point3D(l.Vertices[i + 1].X, l.Vertices[i + 1].Y + 0.025, 0.025 + 0.025));
                    mesh.Positions.Add(new Point3D(l.Vertices[i + 1].X + 0.025, l.Vertices[i + 1].Y + 0.025, 0.025 + 0.025));

                    //forward face
                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(1);
                    mesh.TriangleIndices.Add(2);

                    mesh.TriangleIndices.Add(2);
                    mesh.TriangleIndices.Add(1);
                    mesh.TriangleIndices.Add(3);

                    //down face
                    mesh.TriangleIndices.Add(4);
                    mesh.TriangleIndices.Add(5);
                    mesh.TriangleIndices.Add(0);

                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(5);
                    mesh.TriangleIndices.Add(1);

                    //up face
                    mesh.TriangleIndices.Add(2);
                    mesh.TriangleIndices.Add(3);
                    mesh.TriangleIndices.Add(6);

                    mesh.TriangleIndices.Add(6);
                    mesh.TriangleIndices.Add(3);
                    mesh.TriangleIndices.Add(7);

                    //right face
                    mesh.TriangleIndices.Add(1);
                    mesh.TriangleIndices.Add(5);
                    mesh.TriangleIndices.Add(3);

                    mesh.TriangleIndices.Add(3);
                    mesh.TriangleIndices.Add(5);
                    mesh.TriangleIndices.Add(7);

                    //left face
                    mesh.TriangleIndices.Add(4);
                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(6);

                    mesh.TriangleIndices.Add(6);
                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(2);

                    //back face
                    mesh.TriangleIndices.Add(5);
                    mesh.TriangleIndices.Add(4);
                    mesh.TriangleIndices.Add(7);

                    mesh.TriangleIndices.Add(7);
                    mesh.TriangleIndices.Add(4);
                    mesh.TriangleIndices.Add(6);

                    DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));

                    GeometryModel3D line = new GeometryModel3D(mesh, material);

                    line.SetValue(FrameworkElement.TagProperty, l);

                    map3dGroup.Children.Add(line);

                    models.Add(line);
                }
            }
        }

        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {

            RayHitTestResult rayResult = rawresult as RayHitTestResult;
            var modelResult = rayResult.ModelHit.GetValue(FrameworkElement.TagProperty);

            if (rayResult != null)
            {
                bool gasit = false;
                for (int i = 0; i < models.Count; i++)
                {
                    if ((GeometryModel3D)models[i] == rayResult.ModelHit)
                    {
                        hitgeo = (GeometryModel3D)rayResult.ModelHit;
                        gasit = true;
                        
                        if(modelResult is SubstationEntity)
                        {
                            toolTip.Content = "ID: " + ((SubstationEntity)modelResult).Id + "\nName: " + ((SubstationEntity)modelResult).Name;
                            toolTip.IsOpen = true;
                        }
                        else if(modelResult is NodeEntity)
                        {
                            toolTip.Content = "ID: " + ((NodeEntity)modelResult).Id + "\nName: " + ((NodeEntity)modelResult).Name;
                            toolTip.IsOpen = true;
                        }
                        else if(modelResult is SwitchEntity)
                        {
                            toolTip.Content = "ID: " + ((SwitchEntity)modelResult).Id + "\nName: " + ((SwitchEntity)modelResult).Name + "\nStatus: " + ((SwitchEntity)modelResult).Status;
                            toolTip.IsOpen = true;
                        }
                        else if(modelResult is LineEntity)
                        {
                            LineEntity l = ((LineEntity)modelResult);

                            GeometryModel3D first = (GeometryModel3D)map3dGroup.Children.FirstOrDefault(node => (node.GetValue(FrameworkElement.TagProperty) as PowerEntity)?.Id == l.FirstEnd);
                            GeometryModel3D second = (GeometryModel3D)map3dGroup.Children.FirstOrDefault(node => (node.GetValue(FrameworkElement.TagProperty) as PowerEntity)?.Id == l.SecondEnd);

                            if (first != null && second != null)
                            {
                                changeBackFirst = first;
                                changeBackSecond = second;
                                changeBackFirstTexture = (DiffuseMaterial)first.Material;
                                changeBackSecondTexture = (DiffuseMaterial)second.Material;

                                first.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black));
                                second.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black));
                            }
                        }
                    }
                }
                if (!gasit)
                {
                    hitgeo = null;
                }
            }

            return HitTestResultBehavior.Stop;
        }
    }
}
