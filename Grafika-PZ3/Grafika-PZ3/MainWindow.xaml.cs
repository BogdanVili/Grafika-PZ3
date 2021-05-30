using Grafika_PZ3.Models;
using System;
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
                ToLatLon(s.X, s.Y, 34, out newX, out newY);
                s.X = newX;
                s.Y = newY;
            }

            foreach (NodeEntity n in nodeEntities)
            {
                ToLatLon(n.X, n.Y, 34, out newX, out newY);
                n.X = newX;
                n.Y = newY;
            }

            foreach (SwitchEntity s in switchEntities)
            {
                ToLatLon(s.X, s.Y, 34, out newX, out newY);
                s.X = newX;
                s.Y = newY;
            }

            foreach (LineEntity l in lineEntities)
            {
                foreach (Models.Point p in l.Vertices)
                {
                    ToLatLon(p.X, p.Y, 34, out newX, out newY);
                    p.X = newX;
                    p.Y = newY;
                }
            }

            //Remove objects outside map
            for(int i=0; i < substationEntities.Count; i++)
            {
                if(substationEntities[i].X > 45.2325 && substationEntities[i].Y > 19.793909 &&  substationEntities[i].X < 45.277031 && substationEntities[i].Y < 19.894459)
                {
                    continue;
                }
                else
                {
                    substationEntities.Remove(substationEntities[i]);
                }
            }

            for (int i = 0; i < nodeEntities.Count; i++)
            {
                if (nodeEntities[i].X > 45.2325 && nodeEntities[i].Y > 19.793909 && nodeEntities[i].X < 45.277031 && nodeEntities[i].Y < 19.894459)
                {
                    continue;
                }
                else
                {
                    nodeEntities.Remove(nodeEntities[i]);
                }
            }

            for (int i = 0; i < switchEntities.Count; i++)
            {
                if (switchEntities[i].X > 45.2325 && switchEntities[i].Y > 19.793909 && switchEntities[i].X < 45.277031 && switchEntities[i].Y < 19.894459)
                {
                    continue;
                }
                else
                {
                    switchEntities.Remove(switchEntities[i]);
                }
            }

            for (int i = 0; i < lineEntities.Count; i++)
            {
                for(int j=0; j<lineEntities[i].Vertices.Count; j++)
                {
                    if (lineEntities[i].Vertices[j].X > 45.2325 && lineEntities[i].Vertices[j].Y > 19.793909 && lineEntities[i].Vertices[j].X < 45.277031 && lineEntities[i].Vertices[j].Y < 19.894459)
                    {
                        continue;
                    }
                    else
                    {
                        lineEntities[i].Vertices.Remove(lineEntities[i].Vertices[j]);
                    }
                }

                if(lineEntities[i].Vertices.Count == 0 || lineEntities[i].Vertices.Count == 1)
                {
                    lineEntities.Remove(lineEntities[i]);
                }
            }

            //Scaling
            double smallestX = 45.2325;
            double smallestY = 19.793909;

            for (int i = 0; i < substationEntities.Count; i++)
            {
                substationEntities[i].X -= smallestX;
                substationEntities[i].Y -= smallestY;
            }

            for (int i = 0; i < nodeEntities.Count; i++)
            {
                nodeEntities[i].X -= smallestX;
                nodeEntities[i].Y -= smallestY;
            }

            for (int i = 0; i < switchEntities.Count; i++)
            {
                switchEntities[i].X -= smallestX;
                switchEntities[i].Y -= smallestY;
            }

            for (int i = 0; i < lineEntities.Count; i++)
            {
                for (int j = 0; j < lineEntities[i].Vertices.Count; j++)
                {
                    lineEntities[i].Vertices[j].X -= smallestX;
                    lineEntities[i].Vertices[j].Y -= smallestY;
                }
            }
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
                mainViewport.CaptureMouse();
                mouseCaptureCase = "translation";
                beforeMousePosition = e.GetPosition(mainViewport);
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


    }
}
