using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;

namespace Set
{
    /// <summary>
    /// Interaction logic for Card.xaml
    /// </summary>
    public partial class Card : UserControl
    {
        int m_iCardIndex;
        int m_iNumber;
        int m_iColor;
        int m_iShape;
        int m_iFilling;
        const int iStrokeThickness = 6;
        Viewbox[] m_arrViewBox;
        public int Index
        {
            get { return m_iCardIndex; }
        }

        public Card(int iCardIndex)
        {
            InitializeComponent();

            m_iCardIndex = iCardIndex;
            m_iNumber = iCardIndex % 10;
            m_iColor = iCardIndex / 10 % 10;
            m_iShape = iCardIndex / 100 % 10;
            m_iFilling = iCardIndex / 1000 % 10;

            if (m_iNumber == 2)
            {
                gridCard.ColumnDefinitions.RemoveAt(0);
            }
            m_arrViewBox = new Viewbox[m_iNumber];
        }

        private LinearGradientBrush GetStripesBrush(Color color)
        {
            GradientStopCollection gradientStopCollection = new GradientStopCollection();
            gradientStopCollection.Add(new GradientStop(color, 0));
            gradientStopCollection.Add(new GradientStop(color, 0.4));
            gradientStopCollection.Add(new GradientStop(Colors.White, 0.4));
            gradientStopCollection.Add(new GradientStop(Colors.White, 1));
            System.Windows.Media.LinearGradientBrush returnValue = 
                new System.Windows.Media.LinearGradientBrush(gradientStopCollection, new Point(0, 0), new Point(0, 1));
            returnValue.SpreadMethod = GradientSpreadMethod.Repeat;
            returnValue.RelativeTransform = new ScaleTransform(0.09, 0.09);
            return returnValue;
        }

        static Point bez3pts1(Point pointStart, Point pointInt, Point pointEnd)
        {
            Point pointReturn = new Point();
            double c1 = Math.Sqrt((pointInt.X - pointStart.X) * (pointInt.X - pointStart.X) + (pointInt.Y - pointStart.Y) * (pointInt.Y - pointStart.Y));
            double c2 = Math.Sqrt((pointInt.X - pointEnd.X) * (pointInt.X - pointEnd.X) + (pointInt.Y - pointEnd.Y) * (pointInt.Y - pointEnd.Y));
            double t = c1 / (c1 + c2);
            pointReturn.X = (pointInt.X - (1 - t) * (1 - t) * pointStart.X - t * t * pointEnd.X) / (2 * t * (1 - t));
            pointReturn.Y = (pointInt.Y - (1 - t) * (1 - t) * pointStart.Y - t * t * pointEnd.Y) / (2 * t * (1 - t));
            return pointReturn;
        }

        private static Shape Diamod(double dSize)
        {
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(dSize / 4, 0));
            myPointCollection.Add(new Point(0, dSize / 2));
            myPointCollection.Add(new Point(dSize / 4, dSize));
            myPointCollection.Add(new Point(dSize / 2, dSize / 2));

            Polygon polygonDiamond = new Polygon();
            polygonDiamond.Points = myPointCollection;
            polygonDiamond.StrokeThickness = iStrokeThickness;
            return polygonDiamond;
        }
        private static Shape Worm(double dSize)
        {
            Path path = new Path();
            path.StrokeThickness = iStrokeThickness;
            path.StrokeLineJoin = PenLineJoin.Round;
            
            /*
            string[] arrStrPoints = new string[10];
            double dBringToStartX = -(dSize / 8);
            double dBringToStartY = -(dSize / 5);
            arrStrPoints[0] = (dBringToStartX + 2 * (dSize / 5)).ToString() + "," + (dBringToStartY + dSize / 4).ToString() + " ";
            arrStrPoints[1] = (dBringToStartX + 3 * (dSize / 5)).ToString() + "," + (dBringToStartY + (dSize / 4) - (dSize / 10)).ToString() + " ";
            arrStrPoints[2] = (dBringToStartX + dSize / 5).ToString() + "," + (dBringToStartY + dSize / 4).ToString() + " ";
            arrStrPoints[3] = dBringToStartX.ToString() + "," + (dBringToStartY + dSize / 4).ToString() + " ";
            arrStrPoints[4] = (dBringToStartX + 2 * (dSize / 5)).ToString() + "," + (dBringToStartY + dSize).ToString() + " ";
            arrStrPoints[5] = (dBringToStartX + dSize / 5).ToString() + "," + (dBringToStartY + dSize).ToString() + " ";
            arrStrPoints[6] = (dBringToStartX + dSize / 5).ToString() + "," + (dBringToStartY + dSize / 4).ToString() + " ";
            arrStrPoints[7] = (dBringToStartX + 3 * (dSize / 5)).ToString() + "," + (dBringToStartY + dSize).ToString() + " ";
            arrStrPoints[8] = (dBringToStartX + 2 * (dSize / 5)).ToString() + "," + (dBringToStartY + dSize).ToString() + " ";
            arrStrPoints[9] = dBringToStartX.ToString() + "," + (dBringToStartY + dSize + (dSize / 10)).ToString() + " ";
            
            string sGeometry = "M " + arrStrPoints[0]; 
            sGeometry += "C " + arrStrPoints[0] + arrStrPoints[1] + arrStrPoints[2];
            sGeometry += "C " + arrStrPoints[3] + arrStrPoints[4] + arrStrPoints[5];
            sGeometry += "M " + arrStrPoints[0]; 
            sGeometry += "C " + arrStrPoints[6] + arrStrPoints[7] + arrStrPoints[8];
            sGeometry += "C " + arrStrPoints[8] + arrStrPoints[9] + arrStrPoints[5];
            path.Data = Geometry.Parse(sGeometry);
            */

            QuadraticBezierSegment pBezierSegment = new QuadraticBezierSegment();
            PointCollection pointsCollection = new PointCollection();
            pointsCollection.Add(new Point(0, 20));
            pointsCollection.Add(new Point(-10, 10));
            pointsCollection.Add(new Point(0, 0));
            pointsCollection.Add(new Point(10, 0));
            pointsCollection.Add(new Point(20, 40));
            pointsCollection.Add(new Point(30, 50));
            pointsCollection.Add(new Point(20, 60));
            pointsCollection.Add(new Point(10, 60));

            PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();
            pathSegmentCollection.Add(new QuadraticBezierSegment(bez3pts1(pointsCollection[0], pointsCollection[1], pointsCollection[2]), pointsCollection[2], true));
            pathSegmentCollection.Add(new QuadraticBezierSegment(bez3pts1(pointsCollection[2], pointsCollection[3], pointsCollection[4]), pointsCollection[4], true));
            pathSegmentCollection.Add(new QuadraticBezierSegment(bez3pts1(pointsCollection[4], pointsCollection[5], pointsCollection[6]), pointsCollection[6], true));
            pathSegmentCollection.Add(new QuadraticBezierSegment(bez3pts1(pointsCollection[6], pointsCollection[7], pointsCollection[0]), pointsCollection[0], true));

            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = pointsCollection[0];
            pathFigure.IsClosed = true;
            pathFigure.Segments = pathSegmentCollection;

            PathFigureCollection pathFigureCollection = new PathFigureCollection();
            pathFigureCollection.Add(pathFigure);

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures = pathFigureCollection;

            path.Data = pathGeometry;


            /*
            GraphicsPath p = new GraphicsPath();
            System.Drawing.Point[] wormArray = new System.Drawing.Point[6];
            wormArray[0] = new System.Drawing.Point(0, 0);
            wormArray[1] = new System.Drawing.Point(iSize / 10, 2 * (iSize / 5));
            wormArray[2] = new System.Drawing.Point(iSize / 10, 9 * (iSize / 10));
            wormArray[3] = new System.Drawing.Point(3 * (iSize / 5), iSize);
            wormArray[4] = new System.Drawing.Point((iSize / 2), 3 * (iSize / 5));
            wormArray[5] = new System.Drawing.Point((iSize / 2), (iSize / 10));

            for (int i = 0; i < 6; i++)
            {
                wormArray[i].X += x;
                wormArray[i].Y += y;
            }
            p.AddClosedCurve(wormArray);
            */
            return path;
        }
        private static Shape RoundedRectangle(double dSize)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Height = dSize;
            rectangle.Width = dSize / 2;
            rectangle.RadiusX = dSize / 5;
            rectangle.RadiusY = dSize / 5;
            rectangle.StrokeThickness = iStrokeThickness;
            return rectangle;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Shape[] shapes = new Shape[m_iNumber];
            SetShape(shapes);
            DrawShapes(shapes, GetOutLineBrush());
        }
        private void borderCard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            borderCard.CornerRadius = new CornerRadius(Math.Min(gridCard.RowDefinitions[0].ActualHeight / 5, gridCard.ColumnDefinitions[0].ActualWidth * 2 / 5));
            gridCard.Margin = new Thickness(borderCard.ActualWidth / 20, borderCard.ActualHeight / 20, borderCard.ActualWidth / 20, borderCard.ActualHeight / 20);
            for (int iIndex = 0; iIndex < m_iNumber; iIndex++)
            {
                if (m_arrViewBox[iIndex] != null)
                {
                    double dMarginX = gridCard.ColumnDefinitions[iIndex].ActualWidth / 10;
                    double dMarginY = gridCard.RowDefinitions[0].ActualHeight / 10;
                    m_arrViewBox[iIndex].Margin = new Thickness(dMarginX, dMarginY, dMarginX, dMarginY);
                }
            }

        }
        private void SetShape(Shape[] shapes)
        {
            //double dSize = Math.Min((grid1.RowDefinitions[0].ActualHeight) * 0.9, grid1.ColumnDefinitions[0].ActualWidth * 2 * 0.9);
            double dSize = 100; // arbitrary
            for (int iIndex = 0; iIndex < m_iNumber; iIndex++)
            {
                switch (m_iShape)
                {
                    case 1:
                        shapes[iIndex] = RoundedRectangle(dSize);
                        break;
                    case 2:
                        shapes[iIndex] = Diamod(dSize);
                        break;
                    case 3:
                        shapes[iIndex] = Worm(dSize);
                        break;

                }
            }
        }
        private void DrawShapes(Shape[] shapes, SolidColorBrush brushOutLine)
        {
            for (int iIndex = 0; iIndex < m_iNumber; iIndex++)
            {
                shapes[iIndex].Stroke = brushOutLine;
                shapes[iIndex].Fill = GetFillBrush(brushOutLine, brushOutLine.Color);
                DropShadowEffect dropShadowEffect = new DropShadowEffect();
                dropShadowEffect.BlurRadius = 10;
                shapes[iIndex].Effect = dropShadowEffect;
                double dMarginX = gridCard.ColumnDefinitions[iIndex].ActualWidth / 10;
                double dMarginY = gridCard.RowDefinitions[0].ActualHeight / 10;
                m_arrViewBox[iIndex] = new Viewbox();
                m_arrViewBox[iIndex].Child = shapes[iIndex];
                m_arrViewBox[iIndex].Margin = new Thickness(dMarginX, dMarginY, dMarginX, dMarginY);
                m_arrViewBox[iIndex].Stretch = Stretch.Uniform;
                if (m_iNumber == 1)
                {
                    Grid.SetColumn(m_arrViewBox[iIndex], 1);
                }
                else
                {
                    Grid.SetColumn(m_arrViewBox[iIndex], iIndex);
                }
                gridCard.Children.Add(m_arrViewBox[iIndex]);
            }
        }
        private Brush GetFillBrush(Brush brushOuLine, Color colorShape)
        {
            Brush brushFill = Brushes.White;
            switch (m_iFilling)
            {
                case 1:
                    brushFill = Brushes.White;
                    break;
                case 2:
                    brushFill = GetStripesBrush(colorShape);
                    break;
                case 3:
                    brushFill = brushOuLine;
                    break;
            }
            return brushFill;
        }
        private SolidColorBrush GetOutLineBrush()
        {
            SolidColorBrush brushOutLine = Brushes.White;
            switch (m_iColor)
            {
                case 1:
                    brushOutLine = Brushes.Green;
                    break;
                case 2:
                    brushOutLine = Brushes.Red;
                    break;
                case 3:
                    brushOutLine = Brushes.Purple;
                    break;
            }
            return brushOutLine;
        }
        public static int ConvertNumberToCardIndex(int iNum)
        {
            int iRetVal;
            iRetVal = iNum % 3 + 1;
            iRetVal += ((iNum / 3) % 3 + 1) * 10;
            iRetVal += ((iNum / 9) % 3 + 1) * 100;
            iRetVal += ((iNum / 27) % 3 + 1) * 1000;
            return iRetVal;
        }
        public static int ConvertCardIndexToNumber(int iCardIndex)
        {
            int iRetVal;
            iRetVal = iCardIndex % 10 - 1;
            iRetVal += (((iCardIndex / 10) -1) % 10) * 3;
            iRetVal += (((iCardIndex / 100) - 1) % 10) * 9;
            iRetVal += (((iCardIndex / 1000) - 1) % 10) * 27;
            return iRetVal;
        }
        public static bool CheckThreeCards(int iIndex1, int iIndex2, int iIndex3)
        {
            bool bNumber = AllTheSameOrNoneTheSame(iIndex1 % 10, iIndex2 % 10, iIndex3 % 10);
            bool bColor = AllTheSameOrNoneTheSame(iIndex1 / 10 % 10, iIndex2 / 10 % 10, iIndex3 / 10 % 10);
            bool bShape = AllTheSameOrNoneTheSame(iIndex1 / 100 % 10, iIndex2 / 100 % 10, iIndex3 / 100 % 10);
            bool bFilling = AllTheSameOrNoneTheSame(iIndex1 / 1000 % 10, iIndex2 / 1000 % 10, iIndex3 / 1000 % 10);
            return (bNumber && bColor && bShape && bFilling);
        }
        private static bool AllTheSameOrNoneTheSame(int i, int j, int k)
        {
            if (i == j && j == k)
            {
                return true;
            }
            if (i != j && i != k && j != k)
            {
                return true;
            }
            return false;
        }
        
    }
    
}
