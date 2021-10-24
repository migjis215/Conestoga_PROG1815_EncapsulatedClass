/*
 * Program ID: Assignment4
 * 
 * Purpose: Creating a form to store and manage input values in a text file
 *          using an encapsulated class
 * 
 * Revision History: 
 *      Jisung Kim, 2021.04.12: Created
 */

using Assignment4.JKClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Assignment4
{
    public partial class StockMaintenance : Form
    {
        public StockMaintenance()
        {
            InitializeComponent();
        }

        string currentStockId = "";
        string temporaryRecord = "";
        bool isChanged = false;
        bool isCleared = false;

        // Load the stock list into the list box
        private void StockMaintenance_Load(object sender, EventArgs e)
        {
            lblMessages.Text = "";
            txtStockId.Text = "0";
            LoadListBox();
        }

        // Reload the list of list box from the file
        private void LoadListBox()
        {
            try
            {
                List<JKStock> stocks = JKStock.JKGetStocks();
                stocks = stocks.OrderBy(a => a.Name).ToList();

                lstStocks.DisplayMember = "Name";
                lstStocks.ValueMember = "StockId";
                lstStocks.DataSource = stocks;
            }
            catch (Exception ex)
            {
                lblMessages.ForeColor = Color.Red;
                lblMessages.Text = ex.Message;
            }
        }

        // When a stock is selected from the list, 
        // - load the stock's properties into the input areas
        private void lstStocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (lstStocks.SelectedIndex != -1)
                {
                    int selectedValue = int.Parse(lstStocks.SelectedValue.ToString());
                    JKStock stock = JKStock.JKGetByStockId(selectedValue);

                    txtStockId.Text = stock.StockId.ToString();
                    txtName.Text = stock.Name;
                    txtDescription.Text = stock.Description;
                    txtPrice.Text = stock.Price.ToString("n");
                    txtMinutes.Text = stock.Minutes.ToString();
                    chkIsProcedure.Checked = stock.IsProcedure;
                }
            }
            catch (Exception ex)
            {
                lblMessages.ForeColor = Color.Red;
                lblMessages.Text = ex.Message;
            }
        }
        
        // Change the stock ID to zero and clear all input values
        private void btnClearInputs_Click(object sender, EventArgs e)
        {
            lblMessages.Text = "";
            currentStockId = txtStockId.Text;

            temporaryRecord = $"{txtStockId.Text}\t{txtName.Text}\t" +
                              $"{txtDescription.Text}\t{txtPrice.Text}\t" +
                              $"{txtMinutes.Text}\t{chkIsProcedure.Checked}";

            txtStockId.Text = "0";
            txtName.Text = "";
            txtDescription.Text = "";
            txtPrice.Text = "";
            txtMinutes.Text = "";
            chkIsProcedure.Checked = false;

            lstStocks.SelectedIndex = -1;

            isChanged = true;
            isCleared = true;
        }

        // Create an object with input values as properties 
        // - to add or update a stock to the file
        private void btnSave_Click(object sender, EventArgs e)
        {
            lblMessages.ForeColor = Color.Red;
            lblMessages.Text = "";
            currentStockId = txtStockId.Text;

            JKStock stock = new JKStock(int.Parse(txtStockId.Text));

            stock.Name = txtName.Text;
            stock.Description = txtDescription.Text;
            
            if (double.TryParse(txtPrice.Text, out double price))
            {
                stock.Price = price;
            }
            else
            {
                lblMessages.Text += "Price is not numeric \n";
            }

            if (int.TryParse(txtMinutes.Text, out int minutes))
            {
                stock.Minutes = minutes;
            }
            else
            {
                lblMessages.Text += "Minutes is not an integer \n";
            }

            stock.IsProcedure = chkIsProcedure.Checked;

            if (lblMessages.Text != "")
            {
                FocusToInvalidFirstField();
                return;
            }

            lblMessages.ForeColor = Color.Blue;

            try
            {
                if (txtStockId.Text == "0")
                {
                    stock.JKAdd();
                    lblMessages.Text = "A new record has been added";
                }
                else
                {
                    stock.JKUpdate();
                    lblMessages.Text = "The record has been updated";
                }
            }
            catch (Exception ex)
            {
                lblMessages.ForeColor = Color.Red;
                lblMessages.Text = "Exception trying to save the stock: \n" + ex.Message;
                FocusToInvalidFirstField();
            }

            if (lblMessages.ForeColor == Color.Blue)
            {
                LoadListBox();
                lstStocks.SelectedValue = stock.StockId;
            }

            isCleared = false;
            isChanged = true;
        }

        private void FocusToInvalidFirstField()
        {
            string[] words = lblMessages.Text.Split(' ');
            
            foreach (var word in words)
            {
                if (word == "Name")
                {
                    txtName.SelectAll();
                    txtName.Focus();
                    break;
                }
                if (word == "Description")
                {
                    txtDescription.Focus();
                    break;
                }
                if (word == "Price")
                {
                    txtPrice.SelectAll();
                    txtPrice.Focus();
                    break;
                }
                if (word == "Minutes")
                {
                    txtMinutes.SelectAll();
                    txtMinutes.Focus();
                    break;
                }
            }
        }

        // Delete the selected stock 
        // - and display the next stock of the deleted one or the last one
        private void btnDelete_Click(object sender, EventArgs e)
        {
            lblMessages.ForeColor = Color.Red;
            lblMessages.Text = "";

            if (txtStockId.Text == "0")
            {
                if (lstStocks.Items.Count == 0)
                {
                    lblMessages.Text = "The file is empty";
                    return;
                }

                lblMessages.Text = "Please select the name of the stock " +
                                   "you want to delete from the file";
            }
            else
            {
                int selectedIndex = lstStocks.SelectedIndex;
                currentStockId = txtStockId.Text;

                try
                {
                    JKStock.JKDelete(txtStockId.Text);
                }
                catch (Exception ex)
                {
                    lblMessages.Text = ex.Message;
                }

                LoadListBox();

                if (lstStocks.Items.Count == 0)
                {
                    btnClearInputs_Click(sender, e);
                }
                else if (selectedIndex == lstStocks.Items.Count)
                {
                    lstStocks.SelectedIndex = selectedIndex - 1;
                }
                else
                {
                    lstStocks.SelectedIndex = selectedIndex;
                }

                lblMessages.ForeColor = Color.Blue;
                lblMessages.Text = "The stock has been deleted";
                isChanged = true;
                isCleared = false;
            }
        }

        // Return the values in the input areas to the values 
        // - before the user clear the input values or adds or updates the record
        private void btnCancel_Click(object sender, EventArgs e)
        {
            lblMessages.ForeColor = Color.Red;
            lblMessages.Text = "";
            
            if (!isChanged)
            {
                lblMessages.Text = "There is no action to cancel";
                return;
            }

            if (isCleared)
            {
                string[] inputs = temporaryRecord.Split('\t');

                lstStocks.SelectedValue = int.Parse(currentStockId);

                txtStockId.Text = inputs[0];
                txtName.Text = inputs[1];
                txtDescription.Text = inputs[2];
                txtPrice.Text = inputs[3];
                txtMinutes.Text = inputs[4];
                chkIsProcedure.Checked = bool.Parse(inputs[5]);
            }
            else
            {
                try
                {
                    JKStock.ConfirmFile();
                    File.Copy(JKStock.archivePath, JKStock.stockPath, overwrite: true);
                    LoadListBox();                    
                }
                catch (Exception ex)
                {
                    lblMessages.Text = ex.Message;
                }

                lstStocks.SelectedValue = int.Parse(currentStockId);
                if (lstStocks.SelectedValue == null)
                {
                    btnClearInputs_Click(sender, e);
                }
            }

            lblMessages.ForeColor = Color.Blue;
            lblMessages.Text = "The previous operation was canceled";
            isChanged = false;
            isCleared = false;
        }

        // Exit the program
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
