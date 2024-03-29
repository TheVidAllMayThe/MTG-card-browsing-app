﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MTGDeckBuilder
{
    /// <summary>
    /// Interaction logic for addCardDialog.xaml
    /// </summary>
    public partial class addCardDialog : Window
    {
        private bool isBasicLand;
        private int limit;
        private int amount;
        private int deckID;
        private string deckName;
        List<string> decknames;
        List<int> deckids;
        private bool sideBoard;

        public int Amount
        {
            get
            {
                return amount;
            }

            set
            {
                amount = value;
            }
        }
        public int DeckID
        {
            get
            {
                return deckID;
            }

            set
            {
                deckID = value;
            }
        }
        public bool SideBoard
        {
            get
            {
                return sideBoard;
            }

            set
            {
                sideBoard = value;
            }
        }

        public string DeckName
        {
            get
            {
                return deckName;
            }

            set
            {
                deckName = value;
            }
        }

        public addCardDialog(BitmapImage image, bool isBasicLand)
        {

            InitializeComponent();
            Amount = 0;
            this.isBasicLand = isBasicLand;


            if (isBasicLand)
            {
                limit = 99;
                numberOfCards.MaxLength = 2;
            }
            else
            {
                limit = 4;
                numberOfCards.MaxLength = 1;
            }

            CardImage.Source = image;

            string cs = ConfigurationManager.ConnectionStrings["magicConnect"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(@cs))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM udf_userDecks(@user)", conn))
            {
                cmd.CommandType = CommandType.Text;

                // set up the parameters
                cmd.Parameters.Add("@user", SqlDbType.VarChar, 255);

                // set parameter values
                cmd.Parameters["@user"].Value = App.User;

                // open connection and execute stored procedure
                conn.Open();
                SqlDataReader queryCommandReader = cmd.ExecuteReader();
                
                decknames = new List<string>();
                deckids = new List<int>();

                while (queryCommandReader.Read())
                {
                    decknames.Add(queryCommandReader["name"].ToString());
                    deckids.Add((int)queryCommandReader["id"]);
                }

                conn.Close();
                decksCombo.ItemsSource = decknames;
                btnDialogOk.IsEnabled = false;
            }
        }

        public addCardDialog(BitmapImage image, String deckname, int deckID, bool isBasicLand)
        {

            InitializeComponent();
            this.isBasicLand = isBasicLand;
            CardImage.Source = image;
            
            decksCombo.Text = deckname;
            decknames = new List<string>();
            deckids = new List<int>();
            decknames.Add(deckname);
            deckids.Add(deckID);
            decksCombo.ItemsSource = decknames;
            decksCombo.SelectedIndex = 0;
            decksCombo.IsEnabled = false;
            btnDialogOk.IsEnabled = true;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.Amount = int.Parse(numberOfCards.Text);
            this.DialogResult = true;
            this.deckID = deckids[decknames.FindIndex(a => a == decksCombo.Text)];
            this.deckName = decksCombo.Text;
            this.sideBoard = (bool)Yes.IsChecked;
            
        }

        private void lessCard_Click(object sender, RoutedEventArgs e)
        {
            try { 
                if ((int.Parse(numberOfCards.Text) - 1 != 0))
                    numberOfCards.Text = "" + (int.Parse(numberOfCards.Text) - 1);
            }catch(FormatException fo)
            {
                numberOfCards.Text = "" + 0;
            }
        }

        private void moreCard_Click(object sender, RoutedEventArgs e)
        {
            try { 
                if ((int.Parse(numberOfCards.Text) + 1 <= limit))
                    numberOfCards.Text = "" + (int.Parse(numberOfCards.Text) + 1);
            }catch(FormatException fo)
            {
                numberOfCards.Text = "" + 1;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = !isBasicLand?new Regex("[^1-4]"):new Regex("[^0-9]");


            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
        }

        private void decksCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDialogOk.IsEnabled = true;
        }
    }
}
