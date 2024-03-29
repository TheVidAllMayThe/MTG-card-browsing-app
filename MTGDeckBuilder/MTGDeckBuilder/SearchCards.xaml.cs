﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace MTGDeckBuilder
{

    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>


    public partial class SearchCards : Page
    {
Border[] borders;
        Label[] titles;
        Label[][] contentsOfBorder;
        Image[] images;
        SqlCommand currentCommand;
        DataTable table;
        BitmapImage[] bis;
        private string abilitiesStartingText;
        private int currentPage;
        public int Deck_id;
        public string Deck_name;
        private Canvas[] canvas;
        private SqlConnection conn;
        public SearchCards(int Deck_id = -1, string Deck_name = "")
        {
            InitializeComponent();
            string cs = ConfigurationManager.ConnectionStrings["magicConnect"].ConnectionString;
            if (this.conn == null) this.conn = new SqlConnection(@cs);
            this.conn.Open();
            this.Deck_id = Deck_id;
            this.Deck_name = Deck_name;

            canvas = new Canvas[6];
            if (Deck_id > 0)
            {
                text_block_context.Text = "Searching cards for deck '";
                text_block_deck.Text = Deck_name;
                text_block_results.Text = "'";
            }
            abilitiesStartingText = abilities_box.Text;
            borders = new Border[6];
            titles = new Label[6];
            images = new Image[6];
            contentsOfBorder = new Label[6][];
           
            for (int k = 0; k<6; k++)
            {
                Grid grid = new Grid();

                Grid buttongrid = new Grid();

                for (int w = 0; w < 2; w++)
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                for (int w = 0; w < 7; w++)
                    grid.RowDefinitions.Add(new RowDefinition());

                ColumnDefinition col = new ColumnDefinition();

                col.Width = new GridLength(1, GridUnitType.Auto);

                grid.ColumnDefinitions.Add(col);

                for (int w = 0; w < 2; w++)
                    buttongrid.ColumnDefinitions.Add(new ColumnDefinition());
                for (int w = 0; w < 7; w++)
                    buttongrid.RowDefinitions.Add(new RowDefinition());

                col = new ColumnDefinition();

                col.Width = new GridLength(1, GridUnitType.Auto);
                grid.ColumnDefinitions.Add(col);


                borders[k] = new Border();
                borders[k].BorderThickness = new Thickness(1.5);

                Viewbox viewBox = new Viewbox();

                images[k] = new Image();
                images[k].Margin = new Thickness(10, 10, 10, 10);

                BitmapImage image = new BitmapImage(new Uri("/magic_the_gathering.png", UriKind.Relative));
                images[k].Source = image;

                viewBox.Child = images[k];

                grid.Children.Add(viewBox);
                Grid.SetRow(viewBox, 0);
                Grid.SetColumn(viewBox, 0);
                Grid.SetRowSpan(viewBox, 7);

                titles[k] = new Label();
                titles[k].Style = Application.Current.Resources["Card Title Style"] as Style;
                titles[k].Content = "Cena";

                viewBox = new Viewbox();
                viewBox.Child = titles[k];
                viewBox.HorizontalAlignment = HorizontalAlignment.Left;

                grid.Children.Add(viewBox);
                Grid.SetRow(viewBox, 0);
                Grid.SetColumn(viewBox, 1);
                Grid.SetColumnSpan(viewBox, 2);

                Label[] tmp = new Label[6];

                

                for (int w=0 ; w<6 ; w++)
                {
                    Label tmpp = new Label();
                    tmpp.Style = Application.Current.Resources["Card Property Style"] as Style;
                    tmpp.Content = "Test";
                    tmp[w] = tmpp;

                    viewBox = new Viewbox();
                    viewBox.Child = tmpp;
                    viewBox.HorizontalAlignment = HorizontalAlignment.Left;
                    grid.Children.Add(viewBox);
                    Grid.SetRow(viewBox, w + 1);
                    Grid.SetColumn(viewBox, 1);
                    Grid.SetColumnSpan(viewBox, 2);

                }

                Line l1 = new Line();
                l1.X1 = 4.5;
                l1.Y1 = 25;
                l1.X2 = 45.5;
                l1.Y2 = 25;
                l1.StrokeThickness = 4;
                l1.Stroke = Brushes.White;

                Line l2 = new Line();
                l2.X1 = 25;
                l2.Y1 = 4.5;
                l2.X2 = 25;
                l2.Y2 = 45.5;
                l2.StrokeThickness = 4;
                l2.Stroke = Brushes.White;
                
                canvas[k] = new Canvas();
                canvas[k].Background = Brushes.Transparent; //To detect clicks anywhere on canvas
                canvas[k].Width = 50;
                canvas[k].Height = 50;
                canvas[k].Children.Add(l1);
                canvas[k].Children.Add(l2);
                canvas[k].Name = "AddButton" + k;
                canvas[k].MouseUp += new MouseButtonEventHandler(this.addButton_Click);
                canvas[k].MouseEnter += new MouseEventHandler(this.addButton_MouseEnter);
                canvas[k].MouseLeave += new MouseEventHandler(this.addButton_MouseLeave);

                viewBox = new Viewbox();
                viewBox.Child = canvas[k];
                viewBox.Margin = new Thickness(0, 0, 15, 15);
                viewBox.HorizontalAlignment = HorizontalAlignment.Right;

                buttongrid.Children.Add(viewBox);
                Grid.SetRow(viewBox, 6);
                Grid.SetColumn(viewBox, 2);
                Grid.SetZIndex(viewBox, 3);

                borders[k].Child = grid;
                mainGrid.Children.Add(borders[k]);
                Grid.SetRow(borders[k], k / 3);
                Grid.SetColumn(borders[k], k % 3);

                mainGrid.Children.Add(buttongrid);
                Grid.SetRow(buttongrid, k / 3);
                Grid.SetColumn(buttongrid, k % 3);

                Grid.SetZIndex(buttongrid, 5);


                contentsOfBorder[k] = tmp;

            }
            if (currentCommand == null)
            {
                currentCommand = new SqlCommand("SELECT * FROM udf_search_cards (@name, @type, @green, @blue, @white, @red, @black, @ability, @edition, @MinPower, @MaxPower, @MinTough, @MaxTough, @MinCMC, @MaxCMC, @Rarity)", conn);
                currentCommand.CommandType = CommandType.Text;
                currentCommand.Parameters.Add("@name", SqlDbType.VarChar, -1);
                currentCommand.Parameters.Add("@type", SqlDbType.VarChar, 255);
                currentCommand.Parameters.Add("@green", SqlDbType.Bit);
                currentCommand.Parameters.Add("@blue", SqlDbType.Bit);
                currentCommand.Parameters.Add("@white", SqlDbType.Bit);
                currentCommand.Parameters.Add("@red", SqlDbType.Bit);
                currentCommand.Parameters.Add("@black", SqlDbType.Bit);
                currentCommand.Parameters.Add("@ability", SqlDbType.VarChar, 255);
                currentCommand.Parameters.Add("@edition", SqlDbType.VarChar, 255);
                currentCommand.Parameters.Add("@MinPower", SqlDbType.Int);
                currentCommand.Parameters.Add("@MaxPower", SqlDbType.Int);
                currentCommand.Parameters.Add("@MinTough", SqlDbType.Int);
                currentCommand.Parameters.Add("@MaxTough", SqlDbType.Int);
                currentCommand.Parameters.Add("@MinCMC", SqlDbType.Int);
                currentCommand.Parameters.Add("@MaxCMC", SqlDbType.Int);
                currentCommand.Parameters.Add("@Rarity", SqlDbType.VarChar, 255);
                Search(null, null);
            }
        }

        private void setCards()
        {   
            using(currentCommand)
            {

                table = new DataTable("cards");
                SqlDataAdapter adapt = new SqlDataAdapter(currentCommand);
                adapt.Fill(table);
                text_block_context.Text = Deck_id > 0 ? "Searching cards for '" : "Search";
                text_block_deck.Text = Deck_name;
                text_block_results.Text = Deck_id > 0 ? "' results = " + table.Rows.Count : " results = " + table.Rows.Count;

                bis = new BitmapImage[table.Rows.Count];
                setPage(1);
            }
        }

        private void setPage(int page)
        {
            int maxPageInt = table.Rows.Count / 6 + (table.Rows.Count % 6 == 0 ? 0 : 1);

            currentPage = page;

            maxPage.Content = "/" + maxPageInt;

            pageTextBox.Text = "" + page;

            if (page < maxPageInt)
                nextPage.IsEnabled = true;
            else
                nextPage.IsEnabled = false;

            if (page > 1)
                previousPage.IsEnabled = true;
            else
                previousPage.IsEnabled = false;

            if(maxPageInt == 0)
            {
                MessageBox.Show("Your search has no results!", "No results", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (page == maxPageInt)
            {
                BitmapImage image = new BitmapImage(new Uri("/magic_the_gathering.png", UriKind.Relative));
                for (int i = 0; i < 6; i++)
                {
                    images[i].Source = null;
                    titles[i].Content = "";
                    canvas[i].Visibility = Visibility.Hidden;
                    for (int k = 0; k < 6; k++)
                    {
                        contentsOfBorder[i][k].Content = "";
                    }
                }
            }

            if (table.Rows.Count > 0)
            {
                for (int i = page * 6 - 6; i < (page == maxPageInt ? table.Rows.Count : page * 6); i++)
                {
                    canvas[i%6].Visibility = Visibility.Visible;
                    borders[i % 6] = new Border();
                    borders[i % 6].BorderThickness = new Thickness(1.5);

                    if (table.Rows[i]["multiverseID"] != null)
                    {
                        if (bis[i] == null)
                        {
                            string fullFilePath = @"http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=" + table.Rows[i]["multiverseID"] + @"&type=card";
                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            bi.UriSource = new Uri(fullFilePath, UriKind.RelativeOrAbsolute);
                            bi.EndInit();
                            bis[i] = bi;
                            images[i % 6].Source = bis[i];
                        }

                        else
                            images[i % 6].Source = bis[i];

                    }

                    titles[i % 6].Content = table.Rows[i]["cardName"];

                    contentsOfBorder[i % 6][0].Content = table.Rows[i]["editionName"];

                    contentsOfBorder[i % 6][1].Content = table.Rows[i]["rarity"];

                    using (SqlCommand cmd = new SqlCommand("usp_TypeOfCardSelect", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // set up the parameters
                        cmd.Parameters.Add("@card", SqlDbType.Int);
                        cmd.Parameters.Add("@type", SqlDbType.Int);

                        // set parameter values
                        cmd.Parameters["@card"].Value = table.Rows[i]["id"];
                        cmd.Parameters["@type"].Value = DBNull.Value;

                        DataTable types = new DataTable("types");
                        SqlDataAdapter adapt = new SqlDataAdapter(cmd);

                        adapt.Fill(types);

                        if (types.Rows.Count == 0)
                            contentsOfBorder[i % 6][2].Content = "---";
                        else
                        {
                            contentsOfBorder[i % 6][2].Content = "";
                            foreach (DataRow row in types.Rows)
                            {
                                contentsOfBorder[i % 6][2].Content = contentsOfBorder[i % 6][2].Content.ToString() + row["type"] + ", ";
                            }
                            contentsOfBorder[i % 6][2].Content = contentsOfBorder[i % 6][2].Content.ToString().Substring(0, contentsOfBorder[i % 6][2].Content.ToString().Length - 2);
                        }
                    }
                    using (SqlCommand cmd = new SqlCommand("usp_SubtypeOfCardSelect", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // set up the parameters
                        cmd.Parameters.Add("@card", SqlDbType.Int);
                        cmd.Parameters.Add("@subtype", SqlDbType.Int);

                        // set parameter values
                        cmd.Parameters["@card"].Value = table.Rows[i]["id"];
                        cmd.Parameters["@subtype"].Value = DBNull.Value;

                        DataTable types = new DataTable("types");

                        DataTable subtypes = new DataTable("types");
                        SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                        adapt.Fill(subtypes);

                        if (types.Rows.Count == 0)
                            contentsOfBorder[i % 6][3].Content = "---";
                        else
                        {
                            contentsOfBorder[i % 6][3].Content = "";
                            foreach (DataRow row in subtypes.Rows)
                            {
                                contentsOfBorder[i % 6][3].Content = contentsOfBorder[i % 6][3].Content.ToString() + row["subtype"] + ", ";
                            }

                            if (contentsOfBorder[i % 6][3].Content.ToString().Trim().Equals(""))
                                contentsOfBorder[i % 6][3].Content = "---";
                            else
                                contentsOfBorder[i % 6][3].Content = contentsOfBorder[i % 6][3].Content.ToString().Substring(0, contentsOfBorder[i % 6][3].Content.ToString().Length - 2);
                        }
                    }

                    if (contentsOfBorder[i % 6][2].ToString().Contains("Creature"))
                    {
                        using (SqlCommand cmd = new SqlCommand("usp_CreatureSelect", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            // set up the parameters
                            cmd.Parameters.Add("@card", SqlDbType.Int);

                            // set parameter values
                            cmd.Parameters["@card"].Value = table.Rows[i]["id"];

                            SqlDataReader dr = cmd.ExecuteReader();
                            dr.Read();
                            contentsOfBorder[i % 6][4].Content = "Power: " + (dr["power"] == null ? "---" : dr["power"]);
                            contentsOfBorder[i % 6][5].Content = "Toughness: " + dr["toughness"];
                            dr.Close();
                        }
                    }
                    else
                    {
                        contentsOfBorder[i % 6][4].Content = "";
                        contentsOfBorder[i % 6][5].Content = "";
                    }
                }
            }
        }



        private void searchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text.Equals("Name"))
            {
                searchBox.Foreground = Brushes.Black;
                searchBox.Text = "";
            }
        }

        private void abilityBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text.Equals("Search"))
            {
                searchBox.Foreground = Brushes.Black;
                searchBox.Text = "";
            }
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            typeComboBox.SelectionChanged -= this.Search;
            GCheckBox.Checked -= this.Search;
            UCheckBox.Checked -= this.Search;
            RCheckBox.Checked -= this.Search;
            WCheckBox.Checked -= this.Search;
            BCheckBox.Checked -= this.Search;
            rarity_combo_box.SelectionChanged -= this.Search;

            searchBox.Text = "Name";
            typeComboBox.SelectedIndex = 0;
            GCheckBox.IsChecked = false;
            UCheckBox.IsChecked = false;
            RCheckBox.IsChecked = false;
            WCheckBox.IsChecked = false;
            BCheckBox.IsChecked = false;
            abilities_box.Text = abilitiesStartingText;
            edition_box.Text = "Edition";
            rarity_combo_box.SelectedIndex = 0;
            min_power_box.Text = "";
            max_power_box.Text = "";
            min_toughness_box.Text = "";
            max_toughness_box.Text = "";
            min_cmc_box.Text = "";
            max_cmc_box.Text = "";

            typeComboBox.SelectionChanged += this.Search;
            GCheckBox.Checked += this.Search;
            UCheckBox.Checked += this.Search;
            RCheckBox.Checked += this.Search;
            WCheckBox.Checked += this.Search;
            BCheckBox.Checked += this.Search;
            rarity_combo_box.SelectionChanged += this.Search;

        }
        private void AdvancedSearchToggle(object sender, RoutedEventArgs e)
        {
            PointCollection pc = new PointCollection();
            Point p1;
            Point p2;
            Point p3;
            if (more_options_row.Height == new GridLength(0))
            {
                more_options_row.Height = GridLength.Auto;
                p1 = new Point(25, 4.5);
                p2 = new Point(4.5, 45.5);
                p3 = new Point(45.5, 45.5);
            }

            else
            {
                more_options_row.Height = new GridLength(0);
                p1 = new System.Windows.Point(25, 45.5);
                p2 = new System.Windows.Point(4.5, 4.5);
                p3 = new System.Windows.Point(45.5, 4.5);
            }
            pc.Add(p1);
            pc.Add(p2);
            pc.Add(p3);
            triangle.Points = pc;

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (abilities_box.Text.Equals(abilitiesStartingText))
            {

                abilities_box.Foreground = Brushes.Black;
                abilities_box.Text = "";
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (abilities_box.Text.Trim().Equals(""))
            {
                abilities_box.Foreground = Brushes.Gray;
                abilities_box.Text = abilitiesStartingText;
            }
        }

        private void searchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text.Trim().Equals(""))
            {
                searchBox.Foreground = Brushes.Gray;
                searchBox.Text = "Name";
            }
        }

        private void edition_box_GotFocus(object sender, RoutedEventArgs e)
        {
            if (edition_box.Text.Equals("Edition"))
            {

                edition_box.Foreground = Brushes.Black;
                edition_box.Text = "";
            }
        }

        private void edition_box_LostFocus(object sender, RoutedEventArgs e)
        {
            if (edition_box.Text.Trim().Equals(""))
            {
                edition_box.Foreground = Brushes.Gray;
                edition_box.Text = "Edition";
            }
        }

        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            
            Regex regex = new Regex("[^0-9]+");
            
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Border)sender).Opacity = 0.2;
        }


        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Border)sender).Opacity = 0;
        }

        private void nextPageClick(object sender, RoutedEventArgs e)
        {
            setPage(int.Parse(pageTextBox.Text)+1);
        }

        private void previousPageClick(object sender, RoutedEventArgs e)
        {
            setPage(int.Parse(pageTextBox.Text) - 1);
        }

        private void pageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.Parse(((TextBox)sender).Text) == 0)
                {
                    setPage(1);
                    e.Handled = true;
                    return;
                }

                if (int.Parse(((TextBox)sender).Text) > int.Parse(maxPage.Content.ToString().Substring(1)))
                {
                    MessageBoxResult result = MessageBox.Show("You have selected an invalid page", "Wrong Page", MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.Handled = true;
                    return;
                }

                setPage(int.Parse(pageTextBox.Text));
                e.Handled = true;
            }
        }

        private void Search(object sender, RoutedEventArgs e)
        {   
            // set parameter values
            if (searchBox.Text.Equals("Name") || searchBox.Text.Equals("")) currentCommand.Parameters["@name"].Value = DBNull.Value;
            else currentCommand.Parameters["@name"].Value = searchBox.Text;

            if (typeComboBox.Text.Equals("Any") || typeComboBox.Text.Equals("")) currentCommand.Parameters["@type"].Value = DBNull.Value;
            else currentCommand.Parameters["@type"].Value = typeComboBox.Text;

            if (!GCheckBox.IsChecked.Value) currentCommand.Parameters["@green"].Value = DBNull.Value;
            else currentCommand.Parameters["@green"].Value = 1;

            if (!UCheckBox.IsChecked.Value) currentCommand.Parameters["@blue"].Value = DBNull.Value;
            else currentCommand.Parameters["@blue"].Value = 1;

            if (!WCheckBox.IsChecked.Value) currentCommand.Parameters["@white"].Value = DBNull.Value;
            else currentCommand.Parameters["@white"].Value = 1;

            if (!RCheckBox.IsChecked.Value) currentCommand.Parameters["@red"].Value = DBNull.Value;
            else currentCommand.Parameters["@red"].Value = 1;

            if (!BCheckBox.IsChecked.Value) currentCommand.Parameters["@black"].Value = DBNull.Value;
            else currentCommand.Parameters["@black"].Value = 1;

            if (abilities_box.Text.Equals(abilitiesStartingText) || abilities_box.Text.Equals("")) currentCommand.Parameters["@ability"].Value = DBNull.Value;
            else currentCommand.Parameters["@ability"].Value = abilities_box.Text;

            if (edition_box.Text.Equals("Edition") || edition_box.Text.Equals("")) currentCommand.Parameters["@edition"].Value = DBNull.Value;
            else currentCommand.Parameters["@edition"].Value = edition_box.Text;
            
            if (min_power_box.Text.Equals("")) currentCommand.Parameters["@MinPower"].Value = DBNull.Value;
            else currentCommand.Parameters["@MinPower"].Value = int.Parse(min_power_box.Text);
            
            if (max_power_box.Text.Equals("")) currentCommand.Parameters["@MaxPower"].Value = DBNull.Value;
            else currentCommand.Parameters["@MaxPower"].Value = int.Parse(max_power_box.Text);
            
            if (min_toughness_box.Text.Equals("")) currentCommand.Parameters["@MinTough"].Value = DBNull.Value;
            else currentCommand.Parameters["@MinTough"].Value = int.Parse(min_toughness_box.Text);

            if (max_toughness_box.Text.Equals("")) currentCommand.Parameters["@MaxTough"].Value = DBNull.Value;
            else currentCommand.Parameters["@MaxTough"].Value = int.Parse(max_toughness_box.Text);

            if (min_cmc_box.Text.Equals("")) currentCommand.Parameters["@MinCMC"].Value = DBNull.Value;
            else currentCommand.Parameters["@MinCMC"].Value = int.Parse(min_cmc_box.Text);

            if (max_cmc_box.Text.Equals("")) currentCommand.Parameters["@MaxCMC"].Value = DBNull.Value;
            else currentCommand.Parameters["@MaxCMC"].Value = int.Parse(max_cmc_box.Text);


            if (rarity_combo_box.Text.Equals("") || rarity_combo_box.Text.Equals("Any")) currentCommand.Parameters["@Rarity"].Value = DBNull.Value;
            else currentCommand.Parameters["@Rarity"].Value = rarity_combo_box.Text;

            BitmapImage image = new BitmapImage(new Uri("/magic_the_gathering.png", UriKind.Relative));
            for (int i = 0; i < 6; i++)
            {
                images[i].Source = null;
                titles[i].Content = "";
                for (int k = 0; k < 6; k++)
                {
                    contentsOfBorder[i][k].Content = "";
                }
                canvas[i].Visibility = Visibility.Hidden;
            }

            
            setCards();

        }

        private void border_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            int borderPressed = int.Parse(((Border)sender).Name.Substring(6));

            //try
            //{
                if (Window.GetWindow(this) != null) //Avoid double click null pointer exceptions
                {
                    Card c = new Card((int)table.Rows[(currentPage - 1) * 6 + borderPressed]["id"]);
                    ((MainWindow)Window.GetWindow(this)).MainFrame.Navigate(c);
                }
            //}
            //catch (IndexOutOfRangeException IOORE) { }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            int buttonPressed = int.Parse(((Canvas)sender).Name.Substring(9));
            int rowNumber = (currentPage - 1) * 6 + buttonPressed;
            addCardDialog add;
            try
            {
                bool isBasicLand = table.Rows[rowNumber]["rarity"].ToString().Equals("Basic Land");
                if(Deck_id != -1)
                {
                    add = new addCardDialog((BitmapImage)images[rowNumber%6].Source, Deck_name, Deck_id, isBasicLand);
                }
                else
                {
                    add = new addCardDialog((BitmapImage)images[rowNumber % 6].Source, isBasicLand);
                }
            }catch(InvalidOperationException io)
            {
                if (Deck_id != -1)
                {
                    add = new addCardDialog((BitmapImage)images[rowNumber].Source, false);
                }
                else
                {
                    add = new addCardDialog((BitmapImage)images[rowNumber % 6].Source, false);
                }
            }
            add.ShowDialog();
            String card_name = table.Rows[rowNumber]["cardName"].ToString();
            if(add.DialogResult == true)
                App.Add_Card((int)table.Rows[rowNumber]["id"], add.DeckName, add.DeckID, add.Amount, add.SideBoard, card_name);
        }

        private void text_block_deck_MouseDown(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) != null) //Avoid double click null pointer exceptions
            {
                Deck d = new Deck(Deck_id);
                ((MainWindow)Window.GetWindow(this)).MainFrame.Navigate(d);
            }
        }

        private void addButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Canvas)sender).Opacity = 0.8;
        }

        private void addButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Canvas)sender).Opacity = 1;
        }

        private void text_block_deck_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Opacity = 0.6;
        }
        private void text_block_deck_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Opacity = 1;
        }

        private void search_KeyboardKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Search(sender, e);
        }
        private void changePage_KeyboardKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && more_options_row.Height == new GridLength(0)) AdvancedSearchToggle(sender, e);
            else if (e.Key == Key.Left)
            {
                if (int.Parse(pageTextBox.Text) > 1)
                    previousPageClick(sender, e);
            }
            else if (e.Key == Key.Right)
            {
                if (int.Parse(pageTextBox.Text)<= int.Parse(maxPage.Content.ToString().Substring(1)))
                    nextPageClick(sender, e);
            }
        }

        private void typeComboBox_DropDownClosed(object sender, EventArgs e)
        {
            Search(sender, null);
        }

        private void rarity_combo_box_DropDownClosed(object sender, EventArgs e)
        {
            Search(sender, null);
        }
    }
}
