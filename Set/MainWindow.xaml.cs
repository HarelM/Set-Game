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
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Threading;

namespace Set
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Card[] m_Cards;
        bool[] m_arrUsedCards;
        eCatdState[] m_arrCardState;
        int m_iScore;
        DispatcherTimer timer;
        DateTime m_timeStart;
        DateTime m_timeLastSuccess;

        enum eCatdState { eCardDoesNotExists = -1, eCardNotSelected, eCardSelected };
        // constants
        const bool DEBUG = false;
        const int iCardsNumber = 81;
        const int iCardsOnScreen = 15;
        const int iCardsPlayNumber = 12;
        const int iNoSetsFoundScore = 200;
        const int iMaxScorePerSet = 120;
        const int iHelpPenetly = -100;
        const int iSecondHelpPenetly = -50;
        const int iHighPenelty = -50;
        const int iLowPenelty = -5;
        const double iAnimationPutCardTime = 1;
        const double iAnimationRemoveCardTime = 0.5;
        static Random randomNumber;
        static SolidColorBrush brushSelectedColor = new SolidColorBrush(Colors.Blue);
        static SolidColorBrush brushUnselectedColor = new SolidColorBrush(Colors.White);
        static DropShadowEffect effectSelected;

        public MainWindow()
        {
            InitializeComponent();
            randomNumber = new Random();

            m_arrUsedCards = new bool[iCardsNumber];
            m_arrCardState = new eCatdState[iCardsOnScreen];

            effectSelected = new DropShadowEffect();
            effectSelected.ShadowDepth = 0;
            effectSelected.BlurRadius = 20;
            effectSelected.Color = brushSelectedColor.Color;
            m_Cards = new Card[iCardsOnScreen];

            
        }

        #region GUI Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            InitGame(true);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            for (int i = 0; i < iCardsOnScreen; i++)
            {
                if (m_arrCardState[i] != eCatdState.eCardDoesNotExists)
                {
                    ResizeAndCenterCard(i);
                }
            }
        }
        private void gridMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is Canvas))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            Canvas canvasCurrent = dep as Canvas;
            if (canvasCurrent != null)
            {
                int iRow = (int)((Canvas)dep).GetValue(Grid.RowProperty);
                int iCol = (int)((Canvas)dep).GetValue(Grid.ColumnProperty);
                if (m_arrCardState[iRow * 3 + iCol] == eCatdState.eCardNotSelected)
                {
                    SelectCard(iRow * 3 + iCol);
                }
                else if (m_arrCardState[iRow * 3 + iCol] == eCatdState.eCardSelected)
                {
                    UnselectCard(iRow * 3 + iCol);
                    UpdateScore(iLowPenelty);
                }
                ThreeIndexes threeIndexesSelectedMax = GetSelectedCardsLocation();
                if (threeIndexesSelectedMax.GetValidIndexes() == 3)
                {
                    CheckAndReplaceCards(threeIndexesSelectedMax);
                }
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            string sText = "Running Time: ";
            TimeSpan diff = (DateTime.Now - m_timeStart);
            sText += diff.Hours.ToString("00") + ":";
            sText += diff.Minutes.ToString("00") + ":";
            sText += diff.Seconds.ToString("00");
            textBlockRunningTime.Text = sText;
            int iRemainigCards = GetUnusedCardsNumber();
            UpdateRemainingCards(iRemainigCards);
            if (iRemainigCards == 0)
            {
                int iTotalSetsLeft = 0;
                GetSetsParticipationForAllTheCards(out iTotalSetsLeft);
                if (iTotalSetsLeft == 0)
                {
                    EndGame();
                    timer.Stop();
                }
            }
        }
        private void buttonHelpMe_Click(object sender, RoutedEventArgs e)
        {
            int iTotalSets = 0;
            ThreeIndexes threeIndexesOfBestSet = GetSetsParticipationForAllTheCards(out iTotalSets);
            if (iTotalSets > 0)
            {
                int iBestCard = threeIndexesOfBestSet.LocationIndex[0];
                if (m_arrCardState[iBestCard] == eCatdState.eCardNotSelected && 
                    GetSelectedCardsLocation().GetValidIndexes() > 0)
                {
                    UnselectAllCards();
                }
                if (m_arrCardState[iBestCard] == eCatdState.eCardNotSelected && 
                    GetSelectedCardsLocation().GetValidIndexes() == 0)
                {
                    SelectCard(iBestCard);
                    UpdateScore(iHelpPenetly);
                }
                else if (m_arrCardState[iBestCard] == eCatdState.eCardSelected && GetSelectedCardsLocation().GetValidIndexes() == 1)
                {
                    SelectCard(threeIndexesOfBestSet.LocationIndex[1]);
                    UpdateScore(iSecondHelpPenetly);

                    if (DEBUG == true)
                    {
                        SelectCard(threeIndexesOfBestSet.LocationIndex[2]);
                        ThreeIndexes threeIndexesSelectedMax = GetSelectedCardsLocation();
                        CheckAndReplaceCards(threeIndexesSelectedMax);
                    }
                }
            }
            else
            {
                UpdateScore(iNoSetsFoundScore);
                AnimateReplaceCard(12, true);
                AnimateReplaceCard(13, true);
                AnimateReplaceCard(14, true);
            }
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                menuNewGame_Click((object)menuNewGame, new RoutedEventArgs());
            }
            else if (e.Key == Key.F1)
            {
                menuHowToPlay_Click((object)menuNewGame, new RoutedEventArgs());
            }
        }
        //------------- Menu Items -------------\\
        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult results = MessageBox.Show("Do you want to exit the game?", "Exit", MessageBoxButton.YesNo);
            if (results == MessageBoxResult.Yes)
            {
                Close();
            }
        }
        private void menuNewGame_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult results = MessageBox.Show("Do you want to start a new game?", "New Game", MessageBoxButton.YesNo);
            if (results == MessageBoxResult.Yes)
            {
                InitGame(false);
            }
        }
        private void menuShowRecords_Click(object sender, RoutedEventArgs e)
        {
            RecordsWindow recordWindow = new RecordsWindow("", -1);
            recordWindow.Show();
        }
        private void menuHowToPlay_Click(object sender, RoutedEventArgs e)
        {
            string sInstructions = "";
            sInstructions += "How To Play:\n";
            sInstructions += "------------------------------------\n";
            sInstructions += "Every card has four characteristics:\n";
            sInstructions += "  1. Color (Red, Green, Purple)\n";
            sInstructions += "  2. Number (1, 2, 3)\n";
            sInstructions += "  3. Shape (Oval, Diagonal, Snake)\n";
            sInstructions += "  4. Filling (Full, Stripes, Empty)\n";
            sInstructions += "Your goal is to find three cards (a Set) that has the four\n"; 
            sInstructions += " characteristics above either all the same or none the same.\n";
            sInstructions += "Try using the 'Help Me' button in case you feel you can not find\n";
            sInstructions += " any sets or to see how to create a set.\n";
            MessageBox.Show(sInstructions);

        }
        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            string sAbout = "";
            sAbout += "About the game and the author:\n";
            sAbout += "----------------------------------------\n";
            sAbout += "This game was created in order to test the ability of WPF.\n";
            sAbout += "This game should not be sold or copied.\n";
            sAbout += "This game is based on a game of cards called 'Set'\n";
            sAbout += " and the company of this game did not give me permission to \n";
            sAbout += " use their trade mark or any other rights.\n";
            sAbout += "\n";
            sAbout += "This game was created by Harel Mazur May 2011.\n";
            sAbout += "This game was tested by Reut Zimerman May 2011.\n";
            MessageBox.Show(sAbout);
        }
        //--------------------------------------\\
        private static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                (SendOrPostCallback)delegate(object arg)
                {
                    var f = arg as DispatcherFrame;
                    f.Continue = false;
                },
                frame);
            Dispatcher.PushFrame(frame);
        }

        #endregion

        private void InitGame(bool bFirstGame)
        {
            timer.Start();
            m_timeStart = DateTime.Now;
            m_timeLastSuccess = DateTime.Now;
            m_iScore = 0;
            UpdateScore(0);

            for (int i = 0; i < iCardsNumber; i++)
            {
                m_arrUsedCards[i] = false;
            }

            for (int i = 0; i < iCardsOnScreen; i++)
            {
                AnimateReplaceCard(i, false);
            }
            buttonHelpMe.IsEnabled = true;
        }
        private void EndGame()
        {
            buttonHelpMe.IsEnabled = false;

            AddNewRecord addNewRecord = new AddNewRecord(m_iScore);
            addNewRecord.ShowDialog();
            RecordsWindow recordWindow = new RecordsWindow(addNewRecord.NewName, m_iScore);
            
            if (addNewRecord.NewGame == true)
            {
                InitGame(false);
            }
            else
            {
                for (int i = 0; i < iCardsOnScreen; i++)
                {
                    RemoveCard(i);
                }
                recordWindow.Show();
            }
        }

        #region Getting Paramters
        private int GetNewCard()
        {
            int iCardNumber = randomNumber.Next(GetUnusedCardsNumber());
            int iRetVal = 0;
            for (iRetVal = 0; iRetVal < iCardsNumber; iRetVal++)
			{
                if (iCardNumber == 0 && m_arrUsedCards[iRetVal] == false)
                {
                    return iRetVal;
                }
			    if (m_arrUsedCards[iRetVal] == false)
                {
                    iCardNumber--;
                }
			}
            // no more cards...
            return -1;
        }
        private int GetUnusedCardsNumber()
        {
            int iRetVal = 0;
            for (int i = 0; i < iCardsNumber; i++)
			{
			    if (m_arrUsedCards[i] == false)
                {
                    iRetVal++;
                }
			}
            return iRetVal;
        }
        private int GetTotalCardsOnBoard()
        {
            int iTotalCardsOnBoard = 0;
            for (int i = 0; i < iCardsOnScreen; i++)
            {
                if (m_arrCardState[i] != eCatdState.eCardDoesNotExists)
                {
                    iTotalCardsOnBoard++;
                }
            }
            return iTotalCardsOnBoard;
        }
        private ThreeIndexes GetSelectedCardsLocation()
        {
            ThreeIndexes threeIndexesMax = new ThreeIndexes();
            int iSelected = 0;
            for (int i = 0; i < iCardsOnScreen; i++)
            {
                if (m_arrCardState[i] == eCatdState.eCardSelected && iSelected < 3)
                {
                    threeIndexesMax.LocationIndex[iSelected] = i;
                    iSelected++;
                }
            }
            return threeIndexesMax;
        }
        private Canvas GetCanvas(int iLocationIndex)
        {
            Canvas canvasReturn = null;
            foreach (UIElement uiElement in gridMain.Children)
            {
                if ((int)uiElement.GetValue(Grid.ColumnProperty) == (iLocationIndex % 3) &&
                    (int)uiElement.GetValue(Grid.RowProperty) == (iLocationIndex / 3) && uiElement is Canvas)
                {
                    canvasReturn = uiElement as Canvas;
                }
            }
            return canvasReturn;
        }
        #endregion

        #region Cards Hnadeling
        private void UpdateCardState(int iIndex, eCatdState eState)
        {
            m_arrCardState[iIndex] = eState;
        }
        private void SelectCard(int iIndex)
        {
            UpdateCardState(iIndex, eCatdState.eCardSelected);
            m_Cards[iIndex].Effect = effectSelected;
        }
        private void UnselectCard(int iIndex)
        {
            UpdateCardState(iIndex, eCatdState.eCardNotSelected);
            m_Cards[iIndex].Effect = null;
        }
        private void UnselectAllCards()
        {
            for (int i = 0; i < iCardsOnScreen; i++)
            {
                if (m_arrCardState[i] == eCatdState.eCardSelected)
                {
                    UnselectCard(i);
                }
            }
        }
        private void PutCard(int iLocationIndex, int iCardIndex)
        {
            if (iCardIndex == -1)
            {
                // new card
                iCardIndex = GetNewCard();
            }
            if (iCardIndex == -1)
            {
                // not sure how this should happen, but y not...
                return;
            }
            m_Cards[iLocationIndex] = new Card(Card.ConvertNumberToCardIndex(iCardIndex));
            m_arrUsedCards[iCardIndex] = true;

            ResizeAndCenterCard(iLocationIndex);
            Canvas canvasCurrent = GetCanvas(iLocationIndex);
            canvasCurrent.Opacity = 1;
            canvasCurrent.Visibility = Visibility.Visible;
            if (canvasCurrent != null)
            {
                canvasCurrent.Children.Add(m_Cards[iLocationIndex]);
                UpdateCardState(iLocationIndex, eCatdState.eCardNotSelected);
            }
        }
        private void RemoveCard(int iLocationIndex)
        {
            Canvas canvasCurrent = GetCanvas(iLocationIndex);
            if (canvasCurrent != null)
            {
                canvasCurrent.Children.Remove(m_Cards[iLocationIndex]);
                UpdateCardState(iLocationIndex, eCatdState.eCardDoesNotExists);
            }
        }
        private void MoveCard(int iIndexFrom, int iIndexTo)
        {
            int iCardIndex = m_Cards[iIndexFrom].Index;
            RemoveCard(iIndexFrom);
            PutCard(iIndexTo, Card.ConvertCardIndexToNumber(iCardIndex));
        }
        private void RearangeCards()
        {
            int iTotalCardsOnBoard = GetTotalCardsOnBoard();
            for (int iCardsToLookFor = 0; iCardsToLookFor < 3; iCardsToLookFor++)
            {
                int iIndexFrom = -1;
                int iIndexTo = -1;
                for (int iGoodLocation = 0; iGoodLocation < iTotalCardsOnBoard; iGoodLocation++)
                {
                    if (m_arrCardState[iGoodLocation] == eCatdState.eCardDoesNotExists)
                    {
                        iIndexTo = iGoodLocation;
                        break;
                    }
                }
                if (iIndexTo == -1)
                {
                    break;
                }
                for (int iBadLocation = iTotalCardsOnBoard; iBadLocation < iCardsOnScreen; iBadLocation++)
                {
                    if (m_arrCardState[iBadLocation] != eCatdState.eCardDoesNotExists)
                    {
                        iIndexFrom = iBadLocation;
                        break;
                    }
                }
                UpdateCardState(iIndexFrom, eCatdState.eCardDoesNotExists);
                UpdateCardState(iIndexTo, eCatdState.eCardNotSelected);
                AnimateMoveCard(iIndexFrom, iIndexTo); 
            }
        }
        private void ResizeAndCenterCard(int iIndex)
        {
            if (m_Cards[iIndex] != null)
            {
                m_Cards[iIndex].Width = gridMain.ColumnDefinitions[0].ActualWidth * 0.9;
                m_Cards[iIndex].Height = gridMain.RowDefinitions[0].ActualHeight * 0.9;
                Canvas.SetLeft(m_Cards[iIndex], gridMain.ColumnDefinitions[0].ActualWidth / 2 - m_Cards[iIndex].Width / 2);
                Canvas.SetTop(m_Cards[iIndex], gridMain.RowDefinitions[0].ActualHeight / 2 - m_Cards[iIndex].Height / 2);
            }
        }
        private void CheckAndReplaceCards(ThreeIndexes threeIndexes)
        {
            int iIndex1 = threeIndexes.LocationIndex[0];
            int iIndex2 = threeIndexes.LocationIndex[1];
            int iIndex3 = threeIndexes.LocationIndex[2];
            if (Card.CheckThreeCards(m_Cards[iIndex1].Index, m_Cards[iIndex2].Index, m_Cards[iIndex3].Index))
            {
                UpdateScore(CalculateAddedPoints());
                m_timeLastSuccess = DateTime.Now;
                int iTotalCardOnBorad = GetTotalCardsOnBoard();
                
                if (iTotalCardOnBorad == iCardsPlayNumber && GetUnusedCardsNumber() > 0)
                {
                    AnimateReplaceCard(iIndex1, false);
                    AnimateReplaceCard(iIndex2, false);
                    AnimateReplaceCard(iIndex3, false);
                }
                else if (GetUnusedCardsNumber() == 0 || iTotalCardOnBorad != iCardsPlayNumber)
                {
                    AnimateRemoveAndRearangeCards(threeIndexes);
                }
                
            }
            else
            {
                UpdateScore(iHighPenelty);
                MessageBox.Show("Try Again...");
            }
            UnselectAllCards();
        }
        
        private ThreeIndexes GetSetsParticipationForAllTheCards(out int iTotalSets)
        {
            int iMaxSetsPerCard = 0;
            iTotalSets = 0;
            ThreeIndexes threeIndexesForBestCard = new ThreeIndexes();
            for (int i = 0; i < iCardsOnScreen; i++)
            {
                ThreeIndexes threeIndexes;
                int iCurrSetsPerCard = GetSetsForCard(i, out threeIndexes);
                iTotalSets += iCurrSetsPerCard;
                if (iMaxSetsPerCard < iCurrSetsPerCard)
                {
                    iMaxSetsPerCard = iCurrSetsPerCard;
                    threeIndexesForBestCard.LocationIndex[0] = threeIndexes.LocationIndex[0];
                    threeIndexesForBestCard.LocationIndex[1] = threeIndexes.LocationIndex[1];
                    threeIndexesForBestCard.LocationIndex[2] = threeIndexes.LocationIndex[2];
                }
            }
            iTotalSets = iTotalSets / 3;
            return threeIndexesForBestCard;
        }
        private int GetSetsForCard(int iIndex, out ThreeIndexes threeIndexes)
        {
            int iTotalSetsForCard = 0;
            threeIndexes = new ThreeIndexes(iIndex, -1, -1);
            for (int j = 0; j < iCardsOnScreen; j++)
            {
                if (j == iIndex)
                {
                    continue;
                }
                for (int k = j + 1; k < iCardsOnScreen; k++)
                {
                    if (k == iIndex)
                    {
                        continue;
                    }
                    if (m_arrCardState[iIndex] != eCatdState.eCardDoesNotExists && 
                        m_arrCardState[j] != eCatdState.eCardDoesNotExists && 
                        m_arrCardState[k] != eCatdState.eCardDoesNotExists)
                    {
                        int iCardIndex1 = m_Cards[iIndex].Index;
                        int iCardIndex2 = m_Cards[j].Index;
                        int iCardIndex3 = m_Cards[k].Index;
                        if (Card.CheckThreeCards(iCardIndex1, iCardIndex2, iCardIndex3) == true)
                        {
                            threeIndexes.LocationIndex[1] = j;
                            threeIndexes.LocationIndex[2] = k;
                            iTotalSetsForCard++;
                        }
                    }
                }
            }
            return iTotalSetsForCard;
        }
        private int FindBestCard(int[] arrResults)
        {
            int iMaxLocation = 0;
            for (int i = 0; i < iCardsOnScreen; i++)
            {
                iMaxLocation = arrResults[i] > arrResults[iMaxLocation] ? i : iMaxLocation;
            }
            return iMaxLocation;
        }
        #endregion

        #region Score and Remaining cards text
        private int CalculateAddedPoints()
        {
            int iAddToScore = iMaxScorePerSet - (int)((DateTime.Now - m_timeLastSuccess).TotalSeconds);
            iAddToScore = Math.Max(1, iAddToScore);
            return iAddToScore;
        }
        private void UpdateScore(int iDiff)
        {
            m_iScore += iDiff;
            textBlockScore.Text = "Score: " + m_iScore.ToString();
        }
        private void UpdateRemainingCards(int iRemainingCards)
        {
            textBlockCardsLeft.Text = "Cards Left: " + iRemainingCards.ToString();
        }
        #endregion

        #region Animation
        private void AnimatePutCard(int iLocationIndex)
        {
            Canvas targetCanves = GetCanvas(iLocationIndex);
            Point pointCanvasCorner = targetCanves.PointToScreen(new Point(0, 0));
            Point pointWindowCorner = PointToScreen(new Point(0, 0));

            // Create the animation path.
            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure();

            PolyLineSegment pLineSegment = new PolyLineSegment();
            Point pointStart = new Point();
            pointStart.X = pointWindowCorner.X - pointCanvasCorner.X + this.ActualWidth / 2 - targetCanves.ActualHeight / 4;
            pointStart.Y = pointWindowCorner.Y - pointCanvasCorner.Y - targetCanves.ActualWidth;
            pFigure.StartPoint = pointStart;
            pLineSegment.Points.Add(pFigure.StartPoint);
            pLineSegment.Points.Add(new Point(0, 0));

            // Create a MatrixTransform. This transform will be used to move the button.
            MatrixTransform matrixTransform = new MatrixTransform();
            targetCanves.RenderTransform = matrixTransform;

            pFigure.Segments.Add(pLineSegment);
            animationPath.Figures.Add(pFigure);

            // Freeze the PathGeometry for performance benefits.
            animationPath.Freeze();

            // Create a MatrixAnimationUsingPath to move the
            // button along the path by animating its MatrixTransform.
            MatrixAnimationUsingPath matrixAnimation = new MatrixAnimationUsingPath();
            matrixAnimation.PathGeometry = animationPath;
            matrixAnimation.Duration = TimeSpan.FromSeconds(iAnimationPutCardTime);

            DoubleAnimation doubleAnimation = new DoubleAnimation(90, 360, TimeSpan.FromSeconds(iAnimationPutCardTime));
            RotateTransform renderTransform = new RotateTransform(0, targetCanves.ActualWidth / 2, targetCanves.ActualHeight / 2);//targetCanves.ActualHeight);
            targetCanves.Children[0].RenderTransform = renderTransform;
            renderTransform.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);

            //matrixAnimation.DoesRotateWithTangent = true;
            matrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);
        }
        private void AnimateMoveCard(int iLocationFrom, int iLocationTo)
        {
            Canvas canvesFrom = GetCanvas(iLocationFrom);
            Point pointCanvasCornerFrom = canvesFrom.PointToScreen(new Point(0, 0));
            Canvas canvesTo = GetCanvas(iLocationTo);
            Point pointCanvasCornerTo = canvesTo.PointToScreen(new Point(0, 0));

            // Create the animation path.
            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure();

            PolyLineSegment pLineSegment = new PolyLineSegment();
            pFigure.StartPoint = new Point(0, 0);
            pLineSegment.Points.Add(pFigure.StartPoint);
            pLineSegment.Points.Add(new Point(pointCanvasCornerTo.X - pointCanvasCornerFrom.X, pointCanvasCornerTo.Y - pointCanvasCornerFrom.Y));

            // Create a MatrixTransform. This transform will be used to move the button.
            MatrixTransform matrixTransform = new MatrixTransform();

            pFigure.Segments.Add(pLineSegment);
            animationPath.Figures.Add(pFigure);

            // Freeze the PathGeometry for performance benefits.
            animationPath.Freeze();

            // Create a MatrixAnimationUsingPath to move the
            // button along the path by animating its MatrixTransform.
            MatrixAnimationUsingPath matrixAnimation = new MatrixAnimationUsingPath();
            matrixAnimation.PathGeometry = animationPath;
            matrixAnimation.Duration = TimeSpan.FromSeconds(iAnimationRemoveCardTime);

            canvesFrom.Children[0].RenderTransform = matrixTransform;
            matrixAnimation.Completed += delegate
            {
                MoveCard(iLocationFrom, iLocationTo);
            };

            matrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);
        }
        private void AnimateReplaceCard(int iLocationIndex, bool bFullScreen)
        {
            Storyboard storyBoard = BuildRemoveStoryBoard(iLocationIndex);
            storyBoard.Completed += delegate
            {
                GetCanvas(iLocationIndex).Visibility = Visibility.Collapsed;
                RemoveCard(iLocationIndex);
                if (bFullScreen == true || iLocationIndex < iCardsPlayNumber)
                {
                    PutCard(iLocationIndex, -1);
                    AnimatePutCard(iLocationIndex);
                }
            };
            storyBoard.Begin();
        }
        private void AnimateRemoveAndRearangeCards(ThreeIndexes threeIndexes)
        {
            Storyboard storyBoard = new Storyboard();
            NameScope.SetNameScope(gridMain, new NameScope());


            for (int i = 0; i < 3; i++)
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
                doubleAnimation.FillBehavior = FillBehavior.Stop;
                int iLocationIndex = threeIndexes.LocationIndex[i];
                gridMain.RegisterName("Canvas" + iLocationIndex.ToString(), GetCanvas(iLocationIndex));
                Storyboard.SetTargetName(doubleAnimation, "Canvas"+iLocationIndex.ToString());
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Control.OpacityProperty));
                storyBoard.Children.Add(doubleAnimation); 
            }
            
            storyBoard.Completed += delegate
            {
                RemoveCard(threeIndexes.LocationIndex[0]);
                RemoveCard(threeIndexes.LocationIndex[1]);
                RemoveCard(threeIndexes.LocationIndex[2]);
                RearangeCards();
            };
            storyBoard.Begin(gridMain);
        }
        private Storyboard BuildRemoveStoryBoard(int iLocationIndex)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(iAnimationRemoveCardTime));
            Canvas currentCanvas = GetCanvas(iLocationIndex);
            currentCanvas.Opacity = 1;
            currentCanvas.Visibility = Visibility.Visible;
            doubleAnimation.FillBehavior = FillBehavior.Stop;
            Storyboard storyBoard = new Storyboard();
            storyBoard.Children.Add(doubleAnimation);

            Storyboard.SetTarget(storyBoard, currentCanvas);
            Storyboard.SetTargetProperty(storyBoard, new PropertyPath(Control.OpacityProperty));

            return storyBoard;
        }
        private Storyboard BuildMoveStoryBoard(int iLocationIndexFrom, int iLocationIndexTo)
        {
            Storyboard storyBoard = new Storyboard();
            return storyBoard;
        }
        // currently not in use...
        private void AnimatePutCard2(int iLocationIndex)
        {

            Canvas targetCanves = GetCanvas(iLocationIndex);
            Point pointCanvasCorner = targetCanves.PointToScreen(new Point(0, 0));

            // Create the animation path.
            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure();

            PolyBezierSegment pBezierSegment = new PolyBezierSegment();

                pFigure.StartPoint = new Point(-pointCanvasCorner.X + this.ActualWidth / 2, -pointCanvasCorner.Y);
                pBezierSegment.Points.Add(pFigure.StartPoint);
                pBezierSegment.Points.Add(new Point(0, 0));


            // Create a MatrixTransform. This transform will be used to move the button.
            MatrixTransform matrixTransform = new MatrixTransform();
            targetCanves.RenderTransform = matrixTransform;

            pFigure.Segments.Add(pBezierSegment);
            animationPath.Figures.Add(pFigure);

            // Freeze the PathGeometry for performance benefits.
            animationPath.Freeze();

            // Create a MatrixAnimationUsingPath to move the
            // button along the path by animating its MatrixTransform.
            MatrixAnimationUsingPath matrixAnimation =
                new MatrixAnimationUsingPath();
            matrixAnimation.PathGeometry = animationPath;
            matrixAnimation.Duration = TimeSpan.FromSeconds(1);

            // Set the animation's DoesRotateWithTangent property
            // to true so that rotates the card in addition to moving it.
            matrixAnimation.DoesRotateWithTangent = true;

            matrixAnimation.Completed += delegate
            {
                // the cards get reversed - there for need animation to flip it...
                DoubleAnimation da = new DoubleAnimation(180, 0, TimeSpan.FromSeconds(0.34));
                RotateTransform rt = new RotateTransform(0, targetCanves.ActualWidth / 2, targetCanves.ActualHeight / 2);//targetCanves.ActualHeight);
                targetCanves.RenderTransform = rt;
                rt.BeginAnimation(RotateTransform.AngleProperty, da);
            };

            matrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);
        }
        private void AnimatePutCard4(int iLocationIndex)
        {
            if (iLocationIndex % 3 == 0)
            {
                AnimatePutCardLeft(iLocationIndex);
            }
            else
            {
                AnimatePutCardMiddleOrRight(iLocationIndex);
            }
        }
        private void AnimatePutCardLeft(int iLocationIndex)
        {

            Canvas targetCanves = GetCanvas(iLocationIndex);
            Point pointCanvasCorner = targetCanves.PointToScreen(new Point(0, 0));

            // Create the animation path.
            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure();

            PolyBezierSegment pBezierSegment = new PolyBezierSegment();
            pFigure.StartPoint = new Point(-pointCanvasCorner.X + this.ActualWidth / 2, -pointCanvasCorner.Y);
            pBezierSegment.Points.Add(pFigure.StartPoint);
            pBezierSegment.Points.Add(new Point(targetCanves.ActualWidth * 2, targetCanves.ActualHeight));
            pBezierSegment.Points.Add(new Point(targetCanves.ActualWidth, targetCanves.ActualHeight));

            // Create a MatrixTransform. This transform will be used to move the button.
            MatrixTransform matrixTransform = new MatrixTransform();
            targetCanves.RenderTransform = matrixTransform;

            pFigure.Segments.Add(pBezierSegment);
            animationPath.Figures.Add(pFigure);

            // Freeze the PathGeometry for performance benefits.
            animationPath.Freeze();

            // Create a MatrixAnimationUsingPath to move the
            // button along the path by animating its MatrixTransform.
            MatrixAnimationUsingPath matrixAnimation =
                new MatrixAnimationUsingPath();
            matrixAnimation.PathGeometry = animationPath;
            matrixAnimation.Duration = TimeSpan.FromSeconds(2 * iAnimationPutCardTime / 3);

            matrixAnimation.Completed += delegate
            {
                // the cards get reversed - there for need animation to flip it...
                DoubleAnimation doubleAnimation = new DoubleAnimation(180, 0, TimeSpan.FromSeconds(iAnimationPutCardTime / 3));
                RotateTransform rotareTransform = new RotateTransform(0, targetCanves.ActualWidth / 2, targetCanves.ActualHeight / 2);//targetCanves.ActualHeight);
                targetCanves.RenderTransform = rotareTransform;
                rotareTransform.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
            };

            // Set the animation's DoesRotateWithTangent property
            // to true so that rotates the card in addition to moving it.
            matrixAnimation.DoesRotateWithTangent = true;
            matrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);
        }
        private void AnimatePutCardMiddleOrRight(int iLocationIndex)
        {

            Canvas targetCanves = GetCanvas(iLocationIndex);
            Point pointCanvasCorner = targetCanves.PointToScreen(new Point(0, 0));

            // Create the animation path.
            PathGeometry animationPath = new PathGeometry();
            PathFigure pFigure = new PathFigure();

            PolyBezierSegment pBezierSegment = new PolyBezierSegment();

            pFigure.StartPoint = new Point(-pointCanvasCorner.X + this.ActualWidth / 2, -pointCanvasCorner.Y);
            pBezierSegment.Points.Add(pFigure.StartPoint);
            pBezierSegment.Points.Add(new Point(-targetCanves.ActualWidth, 0));
            pBezierSegment.Points.Add(new Point(0, 0));


            // Create a MatrixTransform. This transform will be used to move the button.
            MatrixTransform matrixTransform = new MatrixTransform();
            targetCanves.RenderTransform = matrixTransform;

            pFigure.Segments.Add(pBezierSegment);
            animationPath.Figures.Add(pFigure);

            // Freeze the PathGeometry for performance benefits.
            animationPath.Freeze();

            // Create a MatrixAnimationUsingPath to move the
            // button along the path by animating its MatrixTransform.
            MatrixAnimationUsingPath matrixAnimation =
                new MatrixAnimationUsingPath();
            matrixAnimation.PathGeometry = animationPath;
            matrixAnimation.Duration = TimeSpan.FromSeconds(iAnimationPutCardTime);

            // Set the animation's DoesRotateWithTangent property
            // to true so that rotates the card in addition to moving it.
            matrixAnimation.DoesRotateWithTangent = true;
            matrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, matrixAnimation);
        }
        #endregion

    }

    public class ThreeIndexes
    {
        int[] arrLocationIndex;
        public int[] LocationIndex
        {
            get { return arrLocationIndex; }
            set { arrLocationIndex = value; }
        }
        public ThreeIndexes()
        {
            arrLocationIndex = new int[3];
            arrLocationIndex[0] = -1;
            arrLocationIndex[1] = -1;
            arrLocationIndex[2] = -1;
        }
        public ThreeIndexes(int iIndex1, int iIndex2, int iIndex3)
        {
            arrLocationIndex = new int[3];
            arrLocationIndex[0] = iIndex1;
            arrLocationIndex[1] = iIndex2;
            arrLocationIndex[2] = iIndex3;
        }
        public int GetValidIndexes()
        {
            int iRetVal = 0;
            for (int i = 0; i < 3; i++)
            {
                if (arrLocationIndex[i] >= 0)
                {
                    iRetVal++;
                }
            }
            return iRetVal;
        }
    }
}
