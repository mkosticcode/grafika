using projekat.Model;
using Projekat2.ModelView;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Point = System.Windows.Point;

namespace Projekat2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Staticki podaci
        private Point start = new Point();
        private Point diffOffset = new Point();
        private int zoomMax = 7;
        private int zoomCurent = 1;
        double squareSize = 0.02;
        private double lineSize = 0.005;
        double noviX, noviY;
        double minX = 45.2325, maxX = 45.277031, minY = 19.793909, maxY = 19.894459;
        Int32Collection IndiciesObjects = new Int32Collection() { 2, 3, 1, 2, 1, 0, 7, 1, 3, 7, 5, 1, 6, 5, 7, 6, 4, 5, 6, 2, 4, 2, 0, 4, 2, 7, 3, 2, 6, 7, 0, 1, 5, 0, 5, 4 };
        #endregion Staticki podaci
        #region Strukture podataka
        Dictionary<long, SubstationEntity> Substations = new Dictionary<long, SubstationEntity>();
        Dictionary<long, LineEntity> Lines = new Dictionary<long, LineEntity>();
        Dictionary<long, NodeEntity> Nodes = new Dictionary<long, NodeEntity>();
        Dictionary<long, SwitchEntity> Switches = new Dictionary<long, SwitchEntity>();
        List<GeometryModel3D> graphicLines = new List<GeometryModel3D>();
        Dictionary<GeometryModel3D, LineEntity> graphicLines2 = new Dictionary<GeometryModel3D, LineEntity>();
        Dictionary<GeometryModel3D, LineEntity> graphicLines3 = new Dictionary<GeometryModel3D, LineEntity>();
        Dictionary<GeometryModel3D, LineEntity> graphicLines4 = new Dictionary<GeometryModel3D, LineEntity>();
        Dictionary<GeometryModel3D, long> AllEntity = new Dictionary<GeometryModel3D, long>();
        public static Dictionary<PowerEntity, GeometryModel3D> entities = new Dictionary<PowerEntity, GeometryModel3D>();
        public static Dictionary<LineEntity, GeometryModel3D> entities2 = new Dictionary<LineEntity, GeometryModel3D>();
        private Point startRotation = new Point();
        long lineStartNodeID = -1;
        long lineEndNodeID = -1;
        int lineStartNodeType = -1;
        int lineEndNodeType = -1;
        GeometryModel3D targetedEntity = null;
        private bool hiddenEntitiesAndLines = false;
        bool colorSwitch = false;
        bool resistance = false;
        bool resistance2 = false;
        bool resistance3 = false;
        bool resistance4 = false;
        #endregion Strukture podataka


        public MainWindow()
        {
            InitializeComponent();
            LoadDataFromXML();
            CreateNodes();
            CreateLines();

        }
        private void CreateLines()
        {
            double X, Y;
            double X1, Y1;

            foreach (LineEntity line in Lines.Values)
            {
                GeometryModel3D vod2 = new GeometryModel3D();
                for (int i = 0; i < line.Vertices.Count - 1; i++)
                {

                    Point3DCollection pozicije = new Point3DCollection();
                    GeometryModel3D vod = new GeometryModel3D();
                    Model3DGroup currentLine = new Model3DGroup();
                    if (line.ConductorMaterial == "Steel")
                    {
                        vod.Material = new DiffuseMaterial(Brushes.Gray);
                    }
                    else if (line.ConductorMaterial == "Copper")
                    {
                        vod.Material = new DiffuseMaterial(Brushes.Purple);
                    }
                    else if (line.ConductorMaterial == "Acsr")
                    {
                        vod.Material = new DiffuseMaterial(Brushes.Chocolate);
                    }
                    else
                    {
                        vod.Material = new DiffuseMaterial(Brushes.Black);
                    }


                    X = Library.ConvertXCordinates(line.Vertices[i].X, maxX, minX);
                    Y = Library.ConvertYCordinates(line.Vertices[i].Y, maxY, minY);

                    X1 = Library.ConvertXCordinates(line.Vertices[i + 1].X, maxX, minX);
                    Y1 = Library.ConvertYCordinates(line.Vertices[i + 1].Y, maxY, minY);

                    pozicije.Add(new Point3D(Y, X, 0));
                    pozicije.Add(new Point3D(Y + lineSize, X, 0));
                    pozicije.Add(new Point3D(Y, X + lineSize, 0));
                    pozicije.Add(new Point3D(Y + lineSize, X + lineSize, 0));
                    pozicije.Add(new Point3D(Y1, X1, lineSize));
                    pozicije.Add(new Point3D(Y1 + lineSize, X1, lineSize));
                    pozicije.Add(new Point3D(Y1, X1 + lineSize, lineSize));
                    pozicije.Add(new Point3D(Y1 + lineSize, X1 + lineSize, lineSize));

                    vod.Geometry = new MeshGeometry3D()
                    {
                        Positions = pozicije,
                        TriangleIndices = IndiciesObjects
                    };
                    MapaPodloga.Children.Add(vod);
                    graphicLines.Add(vod);
                    graphicLines2.Add(vod, line);
                    graphicLines3.Add(vod, line);
                    graphicLines4.Add(vod, line);
                    AllEntity.Add(vod, line.Id);
                }
            }
        }
        public void CreateNodes()
        {
            double X, Y;
            int noOfLevels = 0;

            foreach (SubstationEntity s in Substations.Values)
            {

                X = Library.ConvertXCordinates(s.X, maxX, minX);
                Y = Library.ConvertYCordinates(s.Y, maxY, minY);

                GeometryModel3D substation = CreateModel(X, Y, 0);

                while (CheckCoordinatesMatch((substation.Geometry as MeshGeometry3D).Positions))
                {
                    noOfLevels++;
                    substation = CreateModel(X, Y, 0, noOfLevels * squareSize);
                }
                entities.Add(s, substation);
                MapaPodloga.Children.Add(substation);
                AllEntity.Add(substation, s.Id);

                noOfLevels = 0;
            }


            foreach (NodeEntity n in Nodes.Values)
            {
                X = Library.ConvertXCordinates(n.X, maxX, minX);
                Y = Library.ConvertYCordinates(n.Y, maxY, minY);

                GeometryModel3D node = CreateModel(X, Y, 1);

                while (CheckCoordinatesMatch((node.Geometry as MeshGeometry3D).Positions))
                {
                    noOfLevels++;
                    node = CreateModel(X, Y, 1, noOfLevels * squareSize);
                }

                entities.Add(n, node);
                MapaPodloga.Children.Add(node);
                AllEntity.Add(node, n.Id);

                noOfLevels = 0; 
            }

            foreach (SwitchEntity s in Switches.Values)
            {
                X = Library.ConvertXCordinates(s.X, maxX, minX);
                Y = Library.ConvertYCordinates(s.Y, maxY, minY);

                GeometryModel3D sw = CreateModel(X, Y, 2);

                while (CheckCoordinatesMatch((sw.Geometry as MeshGeometry3D).Positions))
                {
                    noOfLevels++;
                    sw = CreateModel(X, Y, 2, noOfLevels * squareSize);
                }

                entities.Add(s, sw);
                MapaPodloga.Children.Add(sw);
                AllEntity.Add(sw, s.Id);

                noOfLevels = 0; 

            }
        }
        private bool CheckCoordinatesMatch(Point3DCollection el)
        {
            foreach (GeometryModel3D item in AllEntity.Keys)
            {
                Point3DCollection koordinate = (item.Geometry as MeshGeometry3D).Positions;

                if (koordinate[0].X == el[0].X && koordinate[0].Y == el[0].Y && koordinate[0].Z == el[0].Z)
                {
                    return true;
                }

               
                if (koordinate[0].Z == el[0].Z)
                {
                    Rect r1 = new Rect(new Point(el[0].X, el[0].Y), new Point(el[3].X, el[3].Y));
                    Rect r2 = new Rect(new Point(koordinate[0].X, koordinate[0].Y), new Point(koordinate[3].X, koordinate[3].Y));
                    Rect r3 = Rect.Intersect(r1, r2); 

                    if (r3 != Rect.Empty)
                        return true;
                }
            }
            return false;
        }
        public void LoadDataFromXML()
        {           

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("Geographic.xml");

            XmlNodeList xmlNodeList;

            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                SubstationEntity s = new SubstationEntity();
                s.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                s.Name = xmlNode.SelectSingleNode("Name").InnerText;
                s.X = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                s.Y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);

                Library.ToLatLon(s.X, s.Y, 34, out noviX, out noviY);
                s.X = noviX;
                s.Y = noviY;
                if (s.X < minX || s.X > maxX || s.Y < minY || s.Y > maxY)
                    continue;

                Substations.Add(s.Id, s);
            }

            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                NodeEntity n = new NodeEntity();
                n.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                n.Name = xmlNode.SelectSingleNode("Name").InnerText;
                n.X = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                n.Y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);

                Library.ToLatLon(n.X, n.Y, 34, out noviX, out noviY);
                n.X = noviX;
                n.Y = noviY;
                if (n.X < minX || n.X > maxX || n.Y < minY || n.Y > maxY)
                    continue;

                Nodes.Add(n.Id, n);
            }

            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                SwitchEntity s = new SwitchEntity();
                s.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                s.Name = xmlNode.SelectSingleNode("Name").InnerText;
                s.X = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                s.Y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);
                s.Status = xmlNode.SelectSingleNode("Status").InnerText;

                Library.ToLatLon(s.X, s.Y, 34, out noviX, out noviY);
                s.X = noviX;
                s.Y = noviY;
                if (s.X < minX || s.X > maxX || s.Y < minY || s.Y > maxY)
                    continue;

                Switches.Add(s.Id, s);
            }

            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                LineEntity l = new LineEntity();
                l.Id = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                l.Name = xmlNode.SelectSingleNode("Name").InnerText;
                l.ConductorMaterial = xmlNode.SelectSingleNode("ConductorMaterial").InnerText;
                l.FirstEnd = long.Parse(xmlNode.SelectSingleNode("FirstEnd").InnerText);
                l.SecondEnd = long.Parse(xmlNode.SelectSingleNode("SecondEnd").InnerText);
                l.R = double.Parse(xmlNode.SelectSingleNode("R").InnerText);

                
                if (!ShouldLineEntityBeOnMap(l))
                    continue;
                    
                l.Vertices = new List<PointEntity>();
                int brojacVertices = 0;
                foreach (XmlNode item in xmlNode.ChildNodes[9].ChildNodes)
                {
                    brojacVertices++;

                    PointEntity p = new PointEntity();
                    p.X = double.Parse(item.SelectSingleNode("X").InnerText);
                    p.Y = double.Parse(item.SelectSingleNode("Y").InnerText);

                    Library.ToLatLon(p.X, p.Y, 34, out noviX, out noviY);
                    p.X = noviX;
                    p.Y = noviY;

                    if (p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY)
                        continue;

                    l.Vertices.Add(p);
                }

                if (l.Vertices.Count == brojacVertices)
                {
                    double cvorX, cvorY;
                    OnMap(l.FirstEnd, out cvorX, out cvorY);
                    PointEntity start = new PointEntity() { X = cvorX, Y = cvorY };
                    OnMap(l.SecondEnd, out cvorX, out cvorY);
                    PointEntity end = new PointEntity() { X = cvorX, Y = cvorY };

                    l.Vertices.Insert(0, start);
                    l.Vertices.Add(end);
                    Lines.Add(l.Id, l);
                }


            }

        }
        private void OnMap(long idCvora, out double x, out double y)
        {
            if (Substations.ContainsKey(idCvora))
            {
                x = Substations[idCvora].X;
                y = Substations[idCvora].Y;
            }
            else if (Switches.ContainsKey(idCvora))
            {
                x = Switches[idCvora].X;
                y = Switches[idCvora].Y;
            }
            else
            {
                x = Nodes[idCvora].X;
                y = Nodes[idCvora].Y;
            }
        }
        private GeometryModel3D CreateModel(double X, double Y, int tip, double Z = 0)
        {           
            Point3DCollection pozicije = new Point3DCollection();
            double squareSize = 0.02;
            pozicije.Add(new Point3D(Y, X, Z));
            pozicije.Add(new Point3D(Y + squareSize, X, Z));
            pozicije.Add(new Point3D(Y, X + squareSize, Z));
            pozicije.Add(new Point3D(Y + squareSize, X + squareSize, Z));
            pozicije.Add(new Point3D(Y, X, Z + squareSize));
            pozicije.Add(new Point3D(Y + squareSize, X, Z + squareSize));
            pozicije.Add(new Point3D(Y, X + squareSize, Z + squareSize));
            pozicije.Add(new Point3D(Y + squareSize, X + squareSize, Z + squareSize));

            GeometryModel3D node = new GeometryModel3D();

            if (tip == 0)
                node.Material = new DiffuseMaterial(Brushes.Black);
            else if (tip == 1)
                node.Material = new DiffuseMaterial(Brushes.Blue);
            else if (tip == 2)
                node.Material = new DiffuseMaterial(Brushes.Crimson);

            node.Geometry = new MeshGeometry3D()
            {
                Positions = pozicije,
                TriangleIndices = IndiciesObjects
            };

            return node;
        }
        private bool ShouldLineEntityBeOnMap(LineEntity l)
        {
            if (Substations.ContainsKey(l.FirstEnd) || Nodes.ContainsKey(l.FirstEnd) || Switches.ContainsKey(l.FirstEnd))
            {
                if (Substations.ContainsKey(l.SecondEnd) || Nodes.ContainsKey(l.SecondEnd) || Switches.ContainsKey(l.SecondEnd))
                    return true;
            }
            return false;
        }
        #region hit
        private void viewport1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mouseCoordinates = e.GetPosition(this);
            PointHitTestParameters pointparams = new PointHitTestParameters(mouseCoordinates);

            targetedEntity = null;
            VisualTreeHelper.HitTest(this, null, HTResult, pointparams);
        }
        private HitTestResultBehavior HTResult(HitTestResult rawresult)
        {
            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {
                rtvrnColors();

                lineStartNodeID = -1;
                lineEndNodeID = -1;

                if (AllEntity.ContainsKey(rayResult.ModelHit as GeometryModel3D))
                {
                    targetedEntity = (GeometryModel3D)rayResult.ModelHit;
                }
                if (targetedEntity == null) return HitTestResultBehavior.Stop;
                long Id = AllEntity[targetedEntity];
                ToolTip tt = new ToolTip();
                tt.StaysOpen = false;


                if (Substations.ContainsKey(Id))
                {
                    tt.Content = "Substation\n" + "ID: " + Id + "\nName: " + Substations[Id].Name;
                    tt.IsOpen = true;
                }

                if (Nodes.ContainsKey(Id))
                {
                    tt.Content = "Node\n" + "ID: " + Id + "\nName: " + Nodes[Id].Name;
                    tt.IsOpen = true;
                }

                if (Switches.ContainsKey(Id))
                {
                    tt.Content = "Switch\n" + "ID: " + Id + "\nName: " + Switches[Id].Name;
                    tt.IsOpen = true;
                }

                if (Lines.ContainsKey(Id))
                {
                    LineEntity line = Lines[Id];
                    tt.Content = "Line\n" + "ID: " + Id + "\nName: " + line.Name + "\nStartNode: " + line.FirstEnd + "\nEndNode: " + line.SecondEnd;
                    tt.IsOpen = true;

                   
                    AllEntity.FirstOrDefault(x => x.Value == line.FirstEnd).Key.Material = new DiffuseMaterial(Brushes.Yellow);
                    AllEntity.FirstOrDefault(x => x.Value == line.SecondEnd).Key.Material = new DiffuseMaterial(Brushes.Yellow);

                    lineStartNodeType = GetTypeForEntity(line.FirstEnd);
                    lineEndNodeType = GetTypeForEntity(line.SecondEnd);

                    lineStartNodeID = line.FirstEnd;
                    lineEndNodeID = line.SecondEnd;
                }

            }

            return HitTestResultBehavior.Stop;
        }
        public void rtvrnColors()
        {
           

            if (lineStartNodeID != -1 && lineEndNodeID != -1)
            {
                if (lineStartNodeType == 0) AllEntity.FirstOrDefault(x => x.Value == lineStartNodeID).Key.Material = new DiffuseMaterial(Brushes.Black); 
                else if (lineStartNodeType == 1) AllEntity.FirstOrDefault(x => x.Value == lineStartNodeID).Key.Material = new DiffuseMaterial(Brushes.Blue); 
                else if (lineStartNodeType == 2) AllEntity.FirstOrDefault(x => x.Value == lineStartNodeID).Key.Material = new DiffuseMaterial(Brushes.Crimson); 

                if (lineEndNodeType == 0) AllEntity.FirstOrDefault(x => x.Value == lineEndNodeID).Key.Material = new DiffuseMaterial(Brushes.Black);
                else if (lineEndNodeType == 1) AllEntity.FirstOrDefault(x => x.Value == lineEndNodeID).Key.Material = new DiffuseMaterial(Brushes.Blue); 
                else if (lineEndNodeType == 2) AllEntity.FirstOrDefault(x => x.Value == lineEndNodeID).Key.Material = new DiffuseMaterial(Brushes.Crimson); 
            }

        }
        private int GetTypeForEntity(long id)
        {           
            if (Substations.ContainsKey(id))
                return 0;
            else if (Nodes.ContainsKey(id))
                return 1;
            else if (Switches.ContainsKey(id))
                return 2;
            else
                return -1;
        }
        #endregion hit
        #region Map rotate
        private void viewport1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewport1.CaptureMouse();
            start = e.GetPosition(this);
            diffOffset.X = translacija.OffsetX;
            diffOffset.Y = translacija.OffsetY;
        }

        private void viewport1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            viewport1.ReleaseMouseCapture();
        }

        private void viewport1_MouseMove(object sender, MouseEventArgs e)
        {
            if (viewport1.IsMouseCaptured)
            {
                Point end = e.GetPosition(this);
                double offsetX = end.X - start.X;
                double offsetY = end.Y - start.Y;
                double w = this.Width;
                double h = this.Height;
                double translateX = (offsetX * 100) / w;
                double translateY = -(offsetY * 100) / h;
                translacija.OffsetX = diffOffset.X + (translateX / (100 * skaliranje.ScaleX));
                translacija.OffsetY = diffOffset.Y + (translateY / (100 * skaliranje.ScaleX));
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Point end = e.GetPosition(this);
                double offsetX = end.X - startRotation.X;
                double offsetY = end.Y - startRotation.Y;

                rotiranje.CenterX = 1;
                rotiranje.CenterY = 1;
                rotiranje.CenterZ = 0;

                offsetX = offsetX > 0 ? 1 : -1;
                offsetY = offsetY > 0 ? 1 : -1;

                if ((Xosa.Angle + (0.3) * offsetY < 87 && Xosa.Angle + (0.3) * offsetY > -71))
                    Xosa.Angle += (0.3) * offsetY;

                if ((Yosa.Angle + (0.3) * offsetX < 100 && Yosa.Angle + (0.3) * offsetX > -71))
                    Yosa.Angle += (0.3) * offsetX;

                startRotation = end;

            }
            else if (e.MiddleButton == MouseButtonState.Released)
            {
               
                startRotation = new Point();
            }
        }

        private void viewport1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(this);
            double scaleX = 1;
            double scaleY = 1;
            if (e.Delta > 0 && zoomCurent < zoomMax)
            {
                scaleX = skaliranje.ScaleX + 0.1;
                scaleY = skaliranje.ScaleY + 0.1;
                zoomCurent++;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
            }
            else if (e.Delta <= 0 && zoomCurent > -zoomMax)
            {
                scaleX = skaliranje.ScaleX - 0.1;
                scaleY = skaliranje.ScaleY - 0.1;
                zoomCurent--;
                skaliranje.ScaleX = scaleX;
                skaliranje.ScaleY = scaleY;
            }
        }
        #endregion Map rotate
        #region ekstra
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (!hiddenEntitiesAndLines)
            {

                foreach (var line in graphicLines4.Keys)
                {

                    foreach (var obj in entities.Keys)
                    {

                        if (obj.GetType() == typeof(SwitchEntity))
                        {
                            var obj2 = (SwitchEntity)obj;
                            if (obj2.Id == graphicLines4[line].FirstEnd && obj2.Status == "Open")
                            {
                                for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                                {
                                    if (MapaPodloga.Children[i] == line)
                                    {
                                        ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Transparent);
                                    }
                                }
                                break;
                            }
                        }
                    }
                    foreach (var obj3 in entities.Keys)
                    {
                        if (obj3.GetType() == typeof(SwitchEntity))
                        {
                            var obj2 = (SwitchEntity)obj3;
                            if (obj2.Id == graphicLines4[line].SecondEnd)
                            {
                                for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                                {
                                    if (MapaPodloga.Children[i] == entities[obj3])
                                    {
                                        ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Transparent);
                                    }
                                }
                                break;
                            }
                        }
                        else if (obj3.GetType() == typeof(NodeEntity))
                        {
                            var obj2 = (NodeEntity)obj3;
                            if (obj2.Id == graphicLines4[line].SecondEnd)
                            {
                                for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                                {
                                    if (MapaPodloga.Children[i] == entities[obj3])
                                    {
                                        ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Transparent);
                                    }
                                }
                                break;
                            }
                        }
                        else
                        {
                            var obj2 = (SubstationEntity)obj3;
                            if (obj2.Id == graphicLines4[line].SecondEnd)
                            {
                                for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                                {
                                    if (MapaPodloga.Children[i] == entities[obj3])
                                    {
                                        ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Transparent);
                                    }
                                }
                                break;
                            }
                        }

                    }

                }

                hiddenEntitiesAndLines = true;
            }
            else
            {
                foreach (var line in graphicLines2.Keys)
                {

                    foreach (var obj in entities.Keys)
                    {

                        if (obj.GetType() == typeof(SwitchEntity))
                        {
                            var obj2 = (SwitchEntity)obj;
                            if (obj2.Id == graphicLines2[line].FirstEnd && obj2.Status == "Open")
                            {
                                for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                                {
                                    if (MapaPodloga.Children[i] == line)
                                    {
                                        if (graphicLines2[line].ConductorMaterial == "Steel")
                                            ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Gray);
                                        if (graphicLines2[line].ConductorMaterial == "Copper")
                                            ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Purple);
                                        if (graphicLines2[line].ConductorMaterial == "Acsr")
                                            ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Chocolate);
                                    }
                                }
                                break;
                            }
                        }
                    }
                    foreach (var obj3 in entities.Keys)
                    {
                        if (obj3.GetType() == typeof(SwitchEntity))
                        {
                            var obj2 = (SwitchEntity)obj3;
                            if (obj2.Id == graphicLines2[line].SecondEnd)
                            {
                                for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                                {
                                    if (MapaPodloga.Children[i] == entities[obj3])
                                    {
                                        ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Crimson);
                                    }
                                }
                                break;
                            }
                        }
                        else if (obj3.GetType() == typeof(NodeEntity))
                        {
                            var obj2 = (NodeEntity)obj3;
                            if (obj2.Id == graphicLines2[line].SecondEnd)
                            {
                                for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                                {
                                    if (MapaPodloga.Children[i] == entities[obj3])
                                    {
                                        ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Blue);
                                    }
                                }
                                break;
                            }
                        }
                        else
                        {
                            var obj2 = (SubstationEntity)obj3;
                            if (obj2.Id == graphicLines2[line].SecondEnd)
                            {
                                for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                                {
                                    if (MapaPodloga.Children[i] == entities[obj3])
                                    {
                                        ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Black);
                                    }
                                }
                                break;
                            }
                        }

                    }

                }

                hiddenEntitiesAndLines = false;
            }
        }
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (!colorSwitch)
            {
                foreach (var obj in entities.Keys)
                {

                    if (obj.GetType() == typeof(SwitchEntity))
                    {
                        var obj2 = (SwitchEntity)obj;
                        if (obj2.Status == "Open")
                        {

                            for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                            {
                                if (MapaPodloga.Children[i] == entities[obj2])
                                {
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Green);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                            {
                                if (MapaPodloga.Children[i] == entities[obj2])
                                {
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Red);
                                }
                            }
                        }
                    }
                }


                colorSwitch = true;
            }
            else
            {
                foreach (var obj in entities.Keys)
                {

                    if (obj.GetType() == typeof(SwitchEntity))
                    {
                        var obj2 = (SwitchEntity)obj;

                        for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                        {
                            if (MapaPodloga.Children[i] == entities[obj2])
                            {
                                ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Crimson);
                            }
                        }

                    }
                }
                colorSwitch = false;
            }
        }
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (!resistance)
            {
                foreach (var obj in graphicLines3.Keys)
                {

                    for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                    {
                        if (MapaPodloga.Children[i] == obj)
                        {
                            if (graphicLines2[obj].R < 1)
                                ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Red);
                            if (graphicLines2[obj].R >= 1 && graphicLines2[obj].R < 2)
                                ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Orange);
                            if (graphicLines2[obj].R >= 2)
                                ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Yellow);
                        }
                    }
                }
                resistance = true;
            }
            else
            {
                foreach (var obj in graphicLines2.Keys)
                {

                    for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                    {
                        if (MapaPodloga.Children[i] == obj)
                        {
                            if (graphicLines2[obj].ConductorMaterial == "Steel")
                                ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Gray);
                            if (graphicLines2[obj].ConductorMaterial == "Copper")
                                ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Purple);
                            if (graphicLines2[obj].ConductorMaterial == "Acsr")
                                ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Chocolate);
                        }
                    }
                }
                resistance = false;
            }
        }
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            if (!resistance2)
            {
                foreach (var obj in graphicLines3.Keys)
                {

                    for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                    {
                        if (MapaPodloga.Children[i] == obj)
                        {
                            if (graphicLines2[obj].R < 1)
                                ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Transparent);                            
                        }
                    }
                }
                resistance2 = true;
            }
            else
            {
                foreach (var obj in graphicLines2.Keys)
                {

                    for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                    {
                        if (MapaPodloga.Children[i] == obj)
                        {
                            if (graphicLines2[obj].R < 1)
                            {
                                if (graphicLines2[obj].ConductorMaterial == "Steel")
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Gray);
                                if (graphicLines2[obj].ConductorMaterial == "Copper")
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Purple);
                                if (graphicLines2[obj].ConductorMaterial == "Acsr")
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Chocolate);
                            }
                        }
                    }
                }
                resistance2 = false;
            }
        }
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            if (!resistance3)
            {
                foreach (var obj in graphicLines3.Keys)
                {

                    for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                    {
                        if (MapaPodloga.Children[i] == obj)
                        {
                            if (graphicLines2[obj].R < 2 && graphicLines2[obj].R>=1)
                                ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Transparent);
                        }
                    }
                }
                resistance3 = true;
            }
            else
            {
                foreach (var obj in graphicLines2.Keys)
                {

                    for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                    {
                        if (MapaPodloga.Children[i] == obj)
                        {
                            if (graphicLines2[obj].R < 2 && graphicLines2[obj].R >= 1)
                            {
                                if (graphicLines2[obj].ConductorMaterial == "Steel")
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Gray);
                                if (graphicLines2[obj].ConductorMaterial == "Copper")
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Purple);
                                if (graphicLines2[obj].ConductorMaterial == "Acsr")
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Chocolate);
                            }
                        }
                    }
                }
                resistance3 = false;
            }
        }
        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            if (!resistance4)
            {
                foreach (var obj in graphicLines3.Keys)
                {

                    for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                    {
                        if (MapaPodloga.Children[i] == obj)
                        {
                            if (graphicLines2[obj].R >= 2)
                                ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Transparent);
                        }
                    }
                }
                resistance4 = true;
            }
            else
            {
                foreach (var obj in graphicLines2.Keys)
                {

                    for (int i = 0; i < MapaPodloga.Children.Count(); i++)
                    {
                        if (MapaPodloga.Children[i] == obj)
                        {
                            if (graphicLines2[obj].R >= 2)
                            {
                                if (graphicLines2[obj].ConductorMaterial == "Steel")
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Gray);
                                if (graphicLines2[obj].ConductorMaterial == "Copper")
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Purple);
                                if (graphicLines2[obj].ConductorMaterial == "Acsr")
                                    ((GeometryModel3D)MapaPodloga.Children[i]).Material = new DiffuseMaterial(System.Windows.Media.Brushes.Chocolate);
                            }
                        }
                    }
                }
                resistance4 = false;
            }
        }
        #endregion ekstra
    }
}
