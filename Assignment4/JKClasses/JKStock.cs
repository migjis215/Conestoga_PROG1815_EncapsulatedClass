using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assignment4.JKClasses
{
    /// <summary>
    /// Class to contain the properties and methods required to instantiate and manage objects
    /// Stock Maintenance Form
    /// J.Kim April 12 2021
    /// </summary>
    public class JKStock
    {
        #region constructors

        public JKStock(int stockId)
        {
            StockId = stockId;
        }
        public JKStock() { }

        #endregion

        #region properties

        public int StockId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Minutes { get; set; }
        public bool IsProcedure { get; set; }

        #endregion

        #region variables

        public static string stockPath = "Stock.txt";
        public static string archivePath = "Archive.txt";
                
        private static StreamReader reader;
        private static StreamWriter writer;
        private static string record = "";

        #endregion

        #region Get methods: JKGetStocks, JKGetByStockId, JKGetByDescription

        /// <summary>
        /// Fetch all stocks on file
        /// </summary>
        /// <returns>List&lt;JKStock&gt;</returns>
        public static List<JKStock> JKGetStocks()
        {
            ConfirmFile();
            List<JKStock> stocks = new List<JKStock>();            

            try
            {
                using (reader = new StreamReader(stockPath))
                {
                    while (!reader.EndOfStream)
                    {
                        record = ExtractRecord();

                        stocks.Add(JKParse(record));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception getting stocks on file: " + ex.Message);
            }

            return stocks.OrderBy(a => a.StockId).ToList();
        }

        /// <summary>
        /// Fetch the stock object for the given stock ID
        /// </summary>
        /// <param name="stockId"></param>
        /// <returns>JKStock object or null</returns>
        public static JKStock JKGetByStockId(int stockId)
        {
            ConfirmFile();

            try
            {
                using (reader = new StreamReader(stockPath))
                {
                    while (!reader.EndOfStream)
                    {
                        record = ExtractRecord();

                        if (record.StartsWith(stockId.ToString() + "\t"))
                        {
                            return JKParse(record);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception getting a stock by ID: " + ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Fetch all stocks for the given keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns>List&lt;JKStock&gt;</returns>
        public static List<JKStock> JKGetByDescription(string keyword)
        {
            ConfirmFile();
            List<JKStock> stocks = new List<JKStock>();

            try
            {
                using (reader = new StreamReader(stockPath))
                {
                    while (!reader.EndOfStream)
                    {
                        record = ExtractRecord();

                        string[] fields = record.Split('\t');

                        if ((fields[1] + fields[2]).ToLower().Contains(keyword.ToLower()))
                        {
                            stocks.Add(JKParse(record));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception getting a stock " +
                                    "by the given string phrase: " + ex.Message);
            }
            
            return stocks.OrderBy(a => a.StockId).ToList();
        }

        #endregion

        #region JKAdd, JKUpdate & JKDelete methods

        /// <summary>
        /// Assign a unique stock ID and add a new stock to the file, if it passes the JKEdit
        /// </summary>
        public void JKAdd()
        {
            JKEdit();

            List<JKStock> stocks = JKGetStocks();

            if (stocks.Count == 0)
            {
                StockId = 1;
            }
            else
            {
                StockId = stocks[stocks.Count - 1].StockId + 1;
            }

            ConfirmFile();

            try
            {
                record = ToString();

                using (writer = new StreamWriter(stockPath, append: true))
                {
                    writer.WriteLine(record);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception trying to add a new stock: " + 
                                     ex.Message);
            }
        }

        /// <summary>
        /// Update the existing stock, if it passes the JKEdit
        /// </summary>
        public void JKUpdate()
        {
            JKEdit();
            ConfirmFile();

            try
            {
                string updatedRecord = ToString();

                using (reader = new StreamReader(archivePath))
                {
                    using (writer = new StreamWriter(stockPath, append: false))
                    {
                        while (!reader.EndOfStream)
                        {
                            record = ExtractRecord();

                            if (record.StartsWith(StockId.ToString() + "\t"))
                            {
                                writer.WriteLine(updatedRecord);
                            }
                            else
                            {
                                writer.WriteLine(record);
                            }
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                throw new Exception("Exception trying to update the stock: " + 
                                     ex.Message);
            }
        }

        /// <summary>
        /// Delete the stock for the given stock ID
        /// </summary>
        /// <param name="stockId"></param>
        public static void JKDelete(string stockId)
        {
            ConfirmFile();

            try
            {
                File.Copy(stockPath, archivePath, overwrite: true);
                using (reader = new StreamReader(archivePath))
                {
                    using (writer = new StreamWriter(stockPath, append: false))
                    {
                        while (!reader.EndOfStream)
                        {
                            record = ExtractRecord();

                            if (!record.StartsWith(stockId + "\t"))
                            {
                                writer.WriteLine(record);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception trying to delete the stock: " + 
                                     ex.Message);
            }
        }

        #endregion

        #region utility methods: ToString, Parse, ConfirmFile, Edit, ExtractRecord

        /// <summary>
        /// Change all properties of an object to a single string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return $"{StockId}\t{Name}\t{Description}\t" +
                   $"{Price}\t{Minutes}\t{IsProcedure}";
        }

        /// <summary>
        /// Convert a record from the file to a JKStock object
        /// </summary>
        /// <param name="input"></param>
        /// <returns>JKStock object</returns>
        public static JKStock JKParse(string input)
        {
            string[] fields = input.Split('\t');

            JKStock stock = new JKStock()
            {
                StockId = int.Parse(fields[0]),
                Name = fields[1],
                Description = fields[2],
                Price = double.Parse(fields[3]),
                Minutes = int.Parse(fields[4]),
                IsProcedure = bool.Parse(fields[5])
            };

            return stock;
        }

        /// <summary>
        /// If the file does not exist, create a new file or recover it from archive
        /// </summary>
        public static void ConfirmFile()
        {
            try
            {
                if (!File.Exists(stockPath))
                {
                    if (File.Exists(archivePath))
                    {
                        File.Move(archivePath, stockPath);
                    }
                    else
                    {
                        File.Create(stockPath).Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception confirming the file: " + ex.Message);
            }
        }

        /// <summary>
        /// Edit the record before adding or updating it to the file
        /// </summary>
        private void JKEdit()
        {
            List<JKStock> stocks = JKGetByDescription(Name);

            bool duplicateName = false;

            foreach (var stock in stocks)
            {
                if (StockId != stock.StockId &&
                    Name.ToLower() == stock.Name.ToLower())
                {
                    duplicateName = true;
                    break;
                }
            }

            string errors = "";

            Name = (Name + "").Trim();
            if (Name == "")
            {
                errors += "--Please enter the stock's Name \n";
            }
            else if (duplicateName)
            {
                errors += "--The Name entered is already on file \n";
            }

            Description = (Description + "").Trim();
            if (Description == "")
            {
                errors += "--Please enter a Description of the stock \n";
            }

            if (Price < 0)
            {
                errors += "--Price cannot be less than zero \n";
            }

            if (!IsProcedure && Minutes != 0)
            {
                errors += "--Minutes must be zero if the stock is not a procedure \n";
            }

            if (IsProcedure && Minutes <= 0)
            {
                errors += "--Minutes must be greater than zero " +
                          "if the stock is a procedure \n";
            }

            File.Copy(stockPath, archivePath, overwrite: true);

            if (errors != "")
            {
                throw new Exception(errors);
            }
        }

        // Extract record exactly as it is entered in the file
        private static string ExtractRecord()
        {
            record = "";
            while (!(record.EndsWith("True") || record.EndsWith("False")))
            {
                record += reader.ReadLine();
                if (!(record.EndsWith("True") || record.EndsWith("False")))
                {
                    record += "\n";
                }
            }

            return record;
        }

        #endregion
    }
}
