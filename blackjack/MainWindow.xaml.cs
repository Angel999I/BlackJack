using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BlackJack
{

    public partial class MainWindow : Window
    {        
        //Hands and Cards
        private List<Card> cards = new List<Card>();
        private List<Image> dealer_hand = new List<Image>();
        private List<Image> player_hand = new List<Image>();

        //Buttons and labels for the game itself
        private Button hit = new Button();
        private Button stand = new Button();
        private Button dual = new Button();

        private Label player_score_label = new Label();
        private Label dealer_score_label = new Label();

        private Label player_wins_label = new Label();
        private Label dealer_wins_label = new Label();

        //Timer for delay between dealer turns
        private DispatcherTimer delay = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        //Random to pick a card with
        private Random rnd = new Random();

        //Project directory for resources
        private string __projectdir__ = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));

        //Card back image 
        private string card_back_source;
        
        //Sound player for sound FX
        private System.Media.SoundPlayer soundplay = new System.Media.SoundPlayer();

        //Grid fade animation as a storyboard
        private Storyboard gridFade;

        //Is the card flipped for the first two cards at the menu
        private bool card_ace_is_flipped = false;
        private bool card_king_is_flipped = false;

        //A bool to determine if the player got a blackjack
        private bool instant_win = false;

        //A bool to determine if the player busted
        private bool bust = false;

        //Player and dealer score
        private int dealer_score = 0;
        private int player_score = 0;

        private int dealer_wins = 0;
        private int player_wins = 0;

        //An int for the current win score (Double or not)
        private int current_win_score = 1;

        //Card variables for the dealer's second card
        private int second_card_score;
        private string second_card_source;

        public MainWindow()
        {
            InitializeComponent();

            //Add to the timer our result procedure to his tick event for a delay
            delay.Tick += Result_feeder;

            //Set the timer tick for a 1.5 Seconds
            delay.Interval = new TimeSpan(0, 0, 0, 1, 500);

            //Save the card back source to a string
            card_back_source = cardAce.Source.ToString();

            //Define the grid fade animation as a storyboard programmatically
            gridFade = (Storyboard)FindResource("GridFade");

            //Add the procedure to the completed event 
            gridFade.Completed += GridFade_Completed;
        }

        public void Game_start()
        {
            /*
             * ****************************************************************** *
             *                              GAME_START
             * 
             * EVENT?: FALSE
             * PURPOSE: When the game starts for the first time, set up the buttons
             * and labels for the player, display them on the canvas and signal the
             * dealer to draw and start the game
             * ****************************************************************** *
             */
            const int btns_and_score_height = 40, btns_and_score_width = 80, btns_and_score_fontsize = 22; //Const for the buttons and label scores height and width

            //Set the hit button properties
            hit.Height = btns_and_score_height;
            hit.Width = btns_and_score_width;
            hit.Content = "Hit";
            hit.Background = null;
            hit.BorderBrush = Brushes.Brown;
            hit.BorderThickness = new Thickness(3, 3, 3, 3);
            hit.FontSize = btns_and_score_fontsize;
            hit.Click += hit_click;

            //Set the stand button properties
            stand.Height = btns_and_score_height;
            stand.Width = btns_and_score_width;
            stand.Content = "Stand";
            stand.Background = null;
            stand.BorderBrush = Brushes.Brown;
            stand.BorderThickness = new Thickness(3, 3, 3, 3);
            stand.FontSize = btns_and_score_fontsize;
            stand.Click += stand_click;

            //Set the dual button properties
            dual.Height = btns_and_score_height;
            dual.Width = btns_and_score_width;
            dual.Content = "Double";
            dual.Background = null;
            dual.BorderBrush = Brushes.Brown;
            dual.BorderThickness = new Thickness(3, 3, 3, 3);
            dual.FontSize = btns_and_score_fontsize;
            dual.Click += dual_Click;

            //Set the player score label properties
            player_score_label.Height = btns_and_score_height;
            player_score_label.Width = btns_and_score_width;
            player_score_label.Content = 0;
            player_score_label.Foreground = Brushes.Gray;
            player_score_label.Background = null;
            player_score_label.BorderBrush = Brushes.White;
            player_score_label.FontSize = btns_and_score_fontsize;

            //Set the dealer score label properties
            dealer_score_label.Height = btns_and_score_height;
            dealer_score_label.Width = btns_and_score_width;
            dealer_score_label.Content = 0;
            dealer_score_label.Foreground = Brushes.Gray;
            dealer_score_label.Background = null;
            dealer_score_label.BorderBrush = Brushes.White;
            dealer_score_label.FontSize = btns_and_score_fontsize;

            //Set the player wins label properties
            player_wins_label.Height = 60;
            player_wins_label.Width = 100;
            player_wins_label.Content = 0;
            player_wins_label.Foreground = Brushes.White;
            player_wins_label.Background = null;
            player_wins_label.BorderBrush = Brushes.White;
            player_wins_label.FontSize = 26;

            //Set the dealer win label properties
            dealer_wins_label.Height = 60;
            dealer_wins_label.Width = 100;
            dealer_wins_label.Content = 0;
            dealer_wins_label.Foreground = Brushes.White;
            dealer_wins_label.Background = null;
            dealer_wins_label.BorderBrush = Brushes.White;
            dealer_wins_label.FontSize = 26;

            //Add all the button's and label's to their desired place
            canvas.Children.Add(hit);
            Canvas.SetTop(hit, 580);
            Canvas.SetLeft(hit, 700);

            canvas.Children.Add(stand);
            Canvas.SetTop(stand, 630);
            Canvas.SetLeft(stand, 700);

            canvas.Children.Add(dual);
            Canvas.SetTop(dual, 680);
            Canvas.SetLeft(dual, 700);

            canvas.Children.Add(player_score_label);
            Canvas.SetTop(player_score_label, 375);
            Canvas.SetLeft(player_score_label, 590);

            canvas.Children.Add(dealer_score_label);
            Canvas.SetTop(dealer_score_label, 345);
            Canvas.SetLeft(dealer_score_label, 590);

            canvas.Children.Add(player_wins_label);
            Canvas.SetTop(player_wins_label, 720);
            Canvas.SetLeft(player_wins_label, 0);

            canvas.Children.Add(dealer_wins_label);
            Canvas.SetTop(dealer_wins_label, 0);
            Canvas.SetLeft(dealer_wins_label, 0);

            Dealer_draw();  //Signal the dealer to draw
        }

        private void Result_feeder(object sender, object e)
        {
            /*
             * ****************************************************************** *
             *                          RESULT_FEEDER
             * 
             * EVENT?: TRUE
             * PURPOSE: When the "delay" timer ticked => Stop the timer, check 
             * who won, update the win scores and reset all the variables for
             * the next round
             * ****************************************************************** *
             */
            //Stop the timer
            delay.Stop();

            /*
             * ****************************************************************** *
             *    Check all of the conditions you may win in a blackjack round
             *          And up the score and update the labels as needed
             *                               ♠️🎲♠️
             * ****************************************************************** *
             */
            if (instant_win == true)
            {
                player_wins += current_win_score;

                player_wins_label.Content = player_wins;
            }

            else if (player_score > 21 && dealer_score > 21)
            {
                player_wins += current_win_score;
                dealer_wins += current_win_score;

                player_wins_label.Content = player_wins;
                dealer_wins_label.Content = dealer_wins;
            }

            else if (player_score == dealer_score)
            {
                player_wins+= current_win_score;
                dealer_wins+= current_win_score;

                player_wins_label.Content = player_wins;
                dealer_wins_label.Content = dealer_wins;
            }

            else if (player_score > 21 && dealer_score <= 21)
            {
                dealer_wins+= current_win_score;

                dealer_wins_label.Content = dealer_wins;
            }

            else if (dealer_score > 21 && player_score <= 21)
            {
                player_wins+= current_win_score;

                player_wins_label.Content = player_wins;
            }

            else if (player_score > dealer_score)
            {
                player_wins+= current_win_score;

                player_wins_label.Content = player_wins;
            }

            else
            {
                dealer_wins+= current_win_score;

                dealer_wins_label.Content = dealer_wins;
            }

            /*
             * ****************************************************************** *
             *        Reset all the crucial variables for the next round                             
             * ****************************************************************** *
             */
            cards.Clear();

            instant_win = false;

            current_win_score = 1;

            bust = false;

            dealer_score = 0;
            player_score = 0;

            player_score_label.Content = player_score;
            dealer_score_label.Content = player_score;

            hit.IsEnabled = true;
            stand.IsEnabled = true;
            dual.IsEnabled = true;

            while (dealer_hand.Count > 0)
            {
                canvas.Children.Remove(dealer_hand[0]);
                dealer_hand.RemoveAt(0);
            }

            while (player_hand.Count > 0)
            {
                canvas.Children.Remove(player_hand[0]);
                player_hand.RemoveAt(0);
            }

            build_card_deck();  //Build the card deck again

            Dealer_draw();  //Start the game by making the dealer draw
        }

        public void Dealer_draw()
        {
            /*
             * ****************************************************************** *
             *                            DEALER_DRAW
             * 
             * EVENT?: FALSE
             * PURPOSE: Make the dealer draw for the first time, draw two cards
             * the first one revealed and the second is not, add the first card
             * score and display both cards
             * ****************************************************************** *
             */
            int left = 515;  //An integer for the left position of the cards
            bool first_draw = true;  //Indicator for the first draw of the dealer
                      
            for (int i = 0; i < 2; i++)
            {
                int card_index = rnd.Next(cards.Count);  //Get a random index for our card list

                Image card = new Image();  //Create a new card as image

                //If the dealer drew his first card, flip him, if not dont flip him
                if (first_draw)
                {
                    card.Source = new BitmapImage(new Uri(cards[card_index].Source, UriKind.Relative));  //Set the card image to his card face

                    //Add the card score and update the label
                    dealer_score += cards[card_index].Score;
                    dealer_score_label.Content = dealer_score;

                    //If the card is an ace, tag him
                    if (cards[card_index].Score == 11)
                        card.Tag = "ace";
                }
                else
                {
                    card.Source = new BitmapImage(new Uri(card_back_source));  //Set the card image to the card back image

                    //Save this second card variables for the next dealer draw
                    second_card_source = cards[card_index].Source;  
                    second_card_score = cards[card_index].Score;
                }

                cards.RemoveAt(card_index);  //Remove the card from the deck

                //Set the image properties
                card.Height = 154;
                card.Width = 102;
                card.Stretch = Stretch.Fill;
                card.VerticalAlignment = VerticalAlignment.Top;
                card.HorizontalAlignment = HorizontalAlignment.Left;
                dealer_hand.Add(card);

                //Add the card image to the canvas
                canvas.Children.Add(dealer_hand[i]);
                Canvas.SetTop(dealer_hand[i], 30);
                Canvas.SetLeft(dealer_hand[i], left);

                left += 75;  //Set the left position to the next card
                first_draw = false;  //Indicate that now this is the dealer second draw
            }

            Player_draw();
        }

        public void Player_draw()
        {
            /*
             * ****************************************************************** *
             *                             PLAYER_DRAW
             * 
             * EVENT?: FALSE
             * PURPOSE: Make the player draw a card, create an image for the card
             * display the card on the canvas, add his score, remove him from the
             * deck and set the left position for the next card
             * ****************************************************************** *
             */
            int left = 515, i, score_temp = 0;  //Define a left for the card left position and a temp score for the first two cards

            for (i = 0; i < 2; i++)
            {
                
                int card_index = rnd.Next(cards.Count);  //Get a random index for our card list


                Image card = new Image();  //Create a new card as image

                
                card.Source = new BitmapImage(new Uri(card_back_source));  //Give the card a back image

                //Check if the card score is lower then 10 for string manipulation
                if (cards[card_index].Score < 10)
                    card.Tag = cards[card_index].Source + "0" + cards[card_index].Score.ToString();   
                else
                    card.Tag = cards[card_index].Source + cards[card_index].Score.ToString();

                score_temp += cards[card_index].Score;

                
                cards.RemoveAt(card_index);  //Remove the card from the list (From the deck)

                
                card.MouseLeftButtonUp += card_click;  //Add the card a click event

                //Set the image properties
                card.Height = 154;
                card.Width = 102;
                card.Stretch = Stretch.Fill;
                card.VerticalAlignment = VerticalAlignment.Top;
                card.HorizontalAlignment = HorizontalAlignment.Left;
                player_hand.Add(card);

                //Add the card to the canvas
                canvas.Children.Add(player_hand[i]);
                Canvas.SetTop(player_hand[i], 580);
                Canvas.SetLeft(player_hand[i], left);
                left += 75;  //Set the next card left position
            }

            //If the player got a blackjack instant_win is true
            if (score_temp == 21)
            {
                instant_win = true;
            }

        }

        public void Dealer_second_draw()
        {
            /*
             * ****************************************************************** *
             *                         DEALER_SECOND_DRAW
             * 
             * EVENT?: FALSE
             * PURPOSE: Make the dealer draw all of his card until he is above
             * 16 points, add all the cards score and display them on the canvas
             * ****************************************************************** *
             */
            //Disable the player ui buttons for the dealer draw
            hit.IsEnabled = false;
            stand.IsEnabled = false;
            dual.IsEnabled = false;

            dealer_hand[1].Source = new BitmapImage(new Uri(second_card_source, UriKind.Relative));  //Set the second card his card image
            dealer_score += second_card_score;  //Up the score by the card
            dealer_score_label.Content = dealer_score;  //Update the label

            //If one of the cards is an ace, tag him 
            if (second_card_score == 11)
                dealer_hand[1].Tag = "ace";

            //If the dealer busted check if he had an ace to balance his score
            if (dealer_score > 21 && ace_check((int)hands.dealer))
            {
                dealer_score -= 10;

                dealer_score_label.Content = dealer_score;
            }

            //If the player didn't bust draw as usua;
            if (bust == false)
            {

                for (int i = 2; dealer_score <= 16; i++)
                {
                    //Move all the cards left
                    for (int j = 0; j < dealer_hand.Count; j++)
                    {
                        Canvas.SetLeft(dealer_hand[j], Canvas.GetLeft(dealer_hand[j]) - 75);
                    }

                    int card_index = rnd.Next(cards.Count);  //Get a random index for our card list

                    Image card = new Image();  //Create a new card as image

                    card.Source = new BitmapImage(new Uri(cards[card_index].Source, UriKind.Relative));  //Give the card their front image

                    //Add the score and display it
                    dealer_score += cards[card_index].Score;
                    dealer_score_label.Content = dealer_score;

                    //If the card the dealer draw is an ace, tag him
                    if (cards[card_index].Score == 11)
                        card.Tag = "ace";

                    cards.RemoveAt(card_index);  //Remove the card from the deck

                    //Set the card properties
                    card.Height = 154;
                    card.Width = 102;
                    card.Stretch = Stretch.Fill;
                    card.VerticalAlignment = VerticalAlignment.Top;
                    card.HorizontalAlignment = HorizontalAlignment.Left;
                    dealer_hand.Add(card);

                    //Add the card to the canvas
                    canvas.Children.Add(dealer_hand[i]);
                    Canvas.SetTop(dealer_hand[i], 30);
                    Canvas.SetLeft(dealer_hand[i], 590);

                    //If the dealer busted, check if he had an ance to balance his score
                    if (dealer_score > 21 && ace_check((int)hands.dealer))
                    {
                        dealer_score -= 10;

                        dealer_score_label.Content = dealer_score;
                    }
                   
                }

            }

            delay.Start();


        }

        private void hit_click(object sender, RoutedEventArgs e)
        {
            /*
             * ****************************************************************** *
             *                                HIT_CLICK
             * 
             * EVENT?: TRUE
             * PURPOSE: When the hit buttons is clicked => Draw a card and add him
             * to the player hand list
             * ****************************************************************** *
             */
           

            //Check if the player flipped all the cards
            for (int i = 0; i < player_hand.Count; i++)
            {
                if (player_hand[i].Source.ToString() == card_back_source)
                    return;
            }

            //Disable the double button by blackjack rules
            dual.IsEnabled = false;

            //Move all the cards left
            for (int i = 0; i < player_hand.Count; i++)
            {
                Canvas.SetLeft(player_hand[i], Canvas.GetLeft(player_hand[i]) - 75);
            }

            
            int card_index = rnd.Next(cards.Count);  //Get a random index for our card list

            
            Image card = new Image();  //Create a new card as image

            
            card.Source = new BitmapImage(new Uri(card_back_source));  //Give the card a back image

            //Check if the card score is lower then 10 for string manipulation
            if (cards[card_index].Score < 10)
                card.Tag = cards[card_index].Source + "0" + cards[card_index].Score.ToString();
            else
                card.Tag = cards[card_index].Source + cards[card_index].Score.ToString();

            
            cards.RemoveAt(card_index);  //Remove the card from the list (From the deck)

            
            card.MouseLeftButtonUp += card_click;  //Add the card a click event

            //Set the image properties
            card.Height = 154;
            card.Width = 102;
            card.Stretch = Stretch.Fill;
            card.VerticalAlignment = VerticalAlignment.Top;
            card.HorizontalAlignment = HorizontalAlignment.Left;

            
            player_hand.Add(card);  //Add the card to the player hand list

            //Add the card to the canvas
            canvas.Children.Add(player_hand[player_hand.Count - 1]);
            Canvas.SetTop(player_hand[player_hand.Count - 1], 580);
            Canvas.SetLeft(player_hand[player_hand.Count - 1], 590);
        }

        private void stand_click(object sender, RoutedEventArgs e)
        {
            //Check if the player flipped all the cards
            for (int i = 0; i < player_hand.Count; i++)
            {
                if (player_hand[i].Source.ToString() == card_back_source)
                    return;
            }

            //Signal the dealer to make his second draw
            Dealer_second_draw();
        }

        private void dual_Click(object sender, RoutedEventArgs e)
        {
            /*
             * ****************************************************************** *
             *                                DUAL_CLICK
             * 
             * EVENT?: TRUE
             * PURPOSE: When the dual button is clicked => Double the win score,
             * draw one card and disable the buttons
             * ****************************************************************** *
             */
            //Check if the player flipped all the cards
            for (int i = 0; i < player_hand.Count; i++)
            {
                if (player_hand[i].Source.ToString() == card_back_source)
                    return;
            }

            hit_click(sender, e);  //Draw one card 

            current_win_score = 2;  //Set the score stake to 2

            //Disable the player ui buttons
            hit.IsEnabled = false;
            stand.IsEnabled = false;
            dual.IsEnabled = false;
        }
        
        private void card_click(object sender, RoutedEventArgs e)
        {
            /*
             * ****************************************************************** *
             *                                CARD_CLICK
             * 
             * EVENT?: TRUE
             * PURPOSE: If one of the player's cards is clicked => Flip them, 
             * calculate the score, and check if the player busted, doubled, or 
             * a blackjack
             * ****************************************************************** *
             */
            Image card = e.Source as Image;  //Create the card image so we can check its source

            //Check if the card is already flipped
            if (card.Source.ToString() == card_back_source)
            {
                //Play the card flip sound
                soundplay.SoundLocation = __projectdir__ + @"\Assests\SoundFX\Card Flip.wav";
                soundplay.Play();

                /*
                 * ********************************************************************** *
                 *   The image tag is includes the front card source and the card score
                 *     By manipulation the string we can use these variables as needed
                 * ********************************************************************** *
                 * 
                 * 
                 * Change the card source from his back to his front and add his score 
                 */
                card.Source = new BitmapImage(new Uri(card.Tag.ToString().Remove(card.Tag.ToString().Length - 2), UriKind.Relative));
                player_score += int.Parse(card.Tag.ToString().Substring(card.Tag.ToString().Length - 2));
                player_score_label.Content = player_score;

                //If the card is an ace, tag him
                if (int.Parse(card.Tag.ToString().Substring(card.Tag.ToString().Length - 2)) == 11)
                {
                    card.Tag = "ace";
                }
            }

            //If the player got a blackjack, skip all the draws and announce the player as the winner
            if (instant_win == true && player_score == 21)
            {
                //Disable the player ui buttons
                hit.IsEnabled = false;
                stand.IsEnabled = false;
                dual.IsEnabled = false;

                //Reveal the dealer second card and add his score to his score label
                dealer_hand[1].Source = new BitmapImage(new Uri(second_card_source, UriKind.Relative));
                dealer_score += second_card_score;
                dealer_score_label.Content = dealer_score;

                delay.Start();  //♠️♠️♠️ Announce our luckey winner ♠️♠️♠️
            }

            //If the player busted check if he had an ace to balance his score
            else if (player_score > 21 && ace_check((int)hands.player))
            {
                player_score -= 10;

                player_score_label.Content = player_score;
            }

            //If the player busted or doubled signal the dealer to draw
            if (player_score > 21)
            {
                bust = true;
                Dealer_second_draw();
            }

            else if (current_win_score == 2)
            {
                Dealer_second_draw();
            }

            

        }

        public bool ace_check(int person)
        {
            /*
             * ****************************************************************** *
             *                                ACE_CHECK
             * 
             * EVENT?: FALSE
             * PURPOSE: Check's if the any of the player's hands have an ace
             * IN: An int to differentiate between the player and the dealer
             * OUT: TRUE/FALSE For the hand ace situation
             * ****************************************************************** *
             */
            if (person == (int)hands.player)
            {
                for (int i = 0; i < player_hand.Count; i++)
                {
                    if (player_hand[i].Tag.ToString() == "ace")
                    {
                            player_hand[i].Tag = "true";
                            return true;
                    }
                    
                }
            }

            else
            {
                for (int i = 0; i < dealer_hand.Count; i++)
                {
                    if (dealer_hand[i].Tag != null)
                    {
                            dealer_hand[i].Tag = null;
                            return true;
                    }
                 
                }
            }

            return false;
        }

        private void GridFade_Completed(object sender, EventArgs e)
        {
            /*
             * ****************************************************************** *
             *                         GRIDFADE_COMPLETED
             * 
             * EVENT?: TRUE
             * PURPOSE: When the grid fade animation is completed => build our 
             * deck of cards and start the game
             * ****************************************************************** *
             */

            //Build the card deck
            build_card_deck();

            //Start the game
            Game_start();
            
        }

        private void cardAce_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            /*
             * ****************************************************************** *
             *                     CARDACE_MOUSELEFTBUTTONDOWN
             * 
             * EVENT?: TRUE
             * PURPOSE: when the cardAce is clicked => Show the club ace png
             * and the play the card flip sound
             * ****************************************************************** *
             */
            if (card_ace_is_flipped == false)
            {
                soundplay.SoundLocation = __projectdir__ + @"\Assests\SoundFX\Card Flip.wav";
                soundplay.Play();
            }
            cardAce.Source = new BitmapImage(new Uri("/Assests/Cards/Ace Club Black.png", UriKind.Relative));
            card_ace_is_flipped = true;                                              
        }

        private void cardKing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            /*
             * ****************************************************************** *
             *                    CARDKING_MOUSELEFTBUTTONDOWN
             * 
             * EVENT?: TRUE
             * PURPOSE: When the cardKing is clicked => Show the diamond king png
             * and play the card flip sound
             * ****************************************************************** *
             */
            if (card_king_is_flipped == false)
            {
                soundplay.SoundLocation = __projectdir__ + @"\Assests\SoundFX\Card Flip.wav";
                soundplay.Play();
            }
            cardKing.Source = new BitmapImage(new Uri("/Assests/Cards/King Diamond Red.png", UriKind.Relative));
            card_king_is_flipped = true;
        }

        private void menuLabels_MouseEnter(object sender, MouseEventArgs e)
        {
            /*
             * ****************************************************************** *
             *                      MENULABLES_MOUSEENTER
             * 
             * EVENT?: TRUE
             * PURPOSE: When the mouse is entering any label on the menu => Change
             * his foreground to yellow and black the menu selection sound
             * ****************************************************************** *
             */
            soundplay.SoundLocation = __projectdir__ + @"\Assests\SoundFX\Menu Selection.wav";
            soundplay.Play();
            Label menulabel = (Label)sender;
            menulabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFDB800"));      
        }

        private void menuLabels_MouseLeave(object sender, MouseEventArgs e)
        {
            /*
             * ****************************************************************** *
             *                       MENULABELS_MOUSELEAVE
             * 
             * EVENT?: TRUE
             * PURPOSE: When the mouse is leaving any label on the menu => Change 
             * his foreground back to white
             * ****************************************************************** *
             */
            Label menulabel = (Label)sender;
            menulabel.Foreground = Brushes.White;
        }

        private void start_label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*
             * ****************************************************************** *
             *                   START_LABEL_MOUSELEFTBUTTONUP
             * 
             * EVENT?: TRUE
             * PURPOSE: When the start label is clicked => Start the grid fade  
             * animation
             * ****************************************************************** *
             */
            gridFade.Begin();        
        }

        private void label_exit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*
             * ****************************************************************** *
             *                   LABEL_EXIT_MOUSELEFTBUTTONUP
             * 
             * EVENT?: TRUE
             * PURPOSE: When the exit label is clicked => Exit the game                           
             * ****************************************************************** *
             */
            Environment.Exit(0);
        }

        private void label_about_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*
             * ****************************************************************** *
             *                   LABEL_ABOUT_MOUSELEFTBUTTONUP
             * 
             * EVENT?: TRUE
             * PURPOSE: When the about label is clicked => show the the game 
             * about tab
             * ****************************************************************** *
             */
        }

        public enum hands
        {
            player = 1,
            dealer = 2
        }



        public void build_card_deck()
        {
            /*
             * ****************************************************************** *
             *                          BUILD_CARD_DECK
             * 
             * EVENT?: FALSE
             * PURPOSE: Build the card deck by adding our cards list all the 
             * cards with their appropriate source and score
             * ****************************************************************** *
             */

            // Club Family:
            cards.Add(new Card("/Assests/Cards/Ace Club Black.png", 11));
            cards.Add(new Card("/Assests/Cards/King Club Black.png", 10));
            cards.Add(new Card("/Assests/Cards/Queen Club Black.png", 10));
            cards.Add(new Card("/Assests/Cards/Jack Club Black.png", 10));
            cards.Add(new Card("/Assests/Cards/10 Club Black.png", 10));
            cards.Add(new Card("/Assests/Cards/9 Club Black.png", 9));
            cards.Add(new Card("/Assests/Cards/8 Club Black.png", 8));
            cards.Add(new Card("/Assests/Cards/7 Club Black.png", 7));
            cards.Add(new Card("/Assests/Cards/6 Club Black.png", 6));
            cards.Add(new Card("/Assests/Cards/5 Club Black.png", 5));
            cards.Add(new Card("/Assests/Cards/4 Club Black.png", 4));
            cards.Add(new Card("/Assests/Cards/3 Club Black.png", 3));
            cards.Add(new Card("/Assests/Cards/2 Club Black.png", 2));

            // Spade Family:
            cards.Add(new Card("/Assests/Cards/Ace Spade Black.png", 11));
            cards.Add(new Card("/Assests/Cards/King Spade Black.png", 10));
            cards.Add(new Card("/Assests/Cards/Queen Spade Black.png", 10));
            cards.Add(new Card("/Assests/Cards/Jack Spade Black.png", 10));
            cards.Add(new Card("/Assests/Cards/10 Spade Black.png", 10));
            cards.Add(new Card("/Assests/Cards/9 Spade Black.png", 9));
            cards.Add(new Card("/Assests/Cards/8 Spade Black.png", 8));
            cards.Add(new Card("/Assests/Cards/7 Spade Black.png", 7));
            cards.Add(new Card("/Assests/Cards/6 Spade Black.png", 6));
            cards.Add(new Card("/Assests/Cards/5 Spade Black.png", 5));
            cards.Add(new Card("/Assests/Cards/4 Spade Black.png", 4));
            cards.Add(new Card("/Assests/Cards/3 Spade Black.png", 3));
            cards.Add(new Card("/Assests/Cards/2 Spade Black.png", 2));

            // Diamond Family:
            cards.Add(new Card("/Assests/Cards/Ace Diamond Red.png", 11));
            cards.Add(new Card("/Assests/Cards/King Diamond Red.png", 10));
            cards.Add(new Card("/Assests/Cards/Queen Diamond Red.png", 10));
            cards.Add(new Card("/Assests/Cards/Jack Diamond Red.png", 10));
            cards.Add(new Card("/Assests/Cards/10 Diamond Red.png", 10));
            cards.Add(new Card("/Assests/Cards/9 Diamond Red.png", 9));
            cards.Add(new Card("/Assests/Cards/8 Diamond Red.png", 8));
            cards.Add(new Card("/Assests/Cards/7 Diamond Red.png", 7));
            cards.Add(new Card("/Assests/Cards/6 Diamond Red.png", 6));
            cards.Add(new Card("/Assests/Cards/5 Diamond Red.png", 5));
            cards.Add(new Card("/Assests/Cards/4 Diamond Red.png", 4));
            cards.Add(new Card("/Assests/Cards/3 Diamond Red.png", 3));
            cards.Add(new Card("/Assests/Cards/2 Diamond Red.png", 2));

            // Heart Family:
            cards.Add(new Card("/Assests/Cards/Ace Heart Red.png", 11));  
            cards.Add(new Card("/Assests/Cards/King Heart Red.png", 10));     
            cards.Add(new Card("/Assests/Cards/Queen Heart Red.png", 10));            
            cards.Add(new Card("/Assests/Cards/Jack Heart Red.png", 10));     
            cards.Add(new Card("/Assests/Cards/10 Heart Red.png", 10)); 
            cards.Add(new Card("/Assests/Cards/9 Heart Red.png", 9));  
            cards.Add(new Card("/Assests/Cards/8 Heart Red.png", 8));
            cards.Add(new Card("/Assests/Cards/7 Heart Red.png", 7));
            cards.Add(new Card("/Assests/Cards/6 Heart Red.png", 6));
            cards.Add(new Card("/Assests/Cards/5 Heart Red.png", 5));
            cards.Add(new Card("/Assests/Cards/4 Heart Red.png", 4));
            cards.Add(new Card("/Assests/Cards/3 Heart Red.png", 3));
            cards.Add(new Card("/Assests/Cards/2 Heart Red.png", 2));
        }
    }
}
