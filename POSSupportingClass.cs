using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
/*
* FILE : POSSupportClass.cs
* PROJECT : PROG2110 - Assignment #4 - Revenge of Wally's World
* PROGRAMMER : Anthony Bastos
* FIRST VERSION : 2016-12-07
* DESCRIPTION :
* The functions in this file are used to help with the logic in creating a POS for Wally's World.
* Example functions include verify customer info, inventory, searching for product names quantities, formatting
* text(Like the sales record), and mainly used for getting data from the MySql database.
*** NOTE : My reading from sql tables was taken from here: http://stackoverflow.com/questions/12408693/how-to-read-and-print-out-data-from-mysql-in-c-sharp
* I used this as a guidline for looking through tables in my database and applied it wherever necessary.
*/

namespace POSAsgn4
{
    /*
    * NAME : POSSupportingClass
    * PURPOSE : To provide supporting functionality for the POS system for Wally's World. There are many
    * differnt methods listed below(Described in furthur detail) but the main purpose for the methods in this
    * class are to handle and MySql querries needed/Reading MySql table information.
    */
    class POSSupportingClass
    {
        public POSSupportingClass()
        {
        }

        /*
        * METHOD : VerifyCustomer
        *
        * DESCRIPTION : This method is used to verify a customers existence. If a customer exists,
        * a status of 1 is returned. If a customer is not found, a status of 0 is returned.
        *
        * PARAMETERS : MySqlCommand command : command control for Sql
        * string FirstName : The firstname given by the user
        * string LastName : The lastname given by the user
        * string PhoneNumber : The phonenumber given by the user
        *
        * RETURNS : int : The status of the search/verification
        * 
        */
        public int VerifyCustomer(MySqlCommand command, string FirstName, string LastName, string PhoneNumber)
        {
            int status = 0;
            int i = 1;
            string tmpFirstName;
            string tmpLastName;
            string tmpPhoneNumber;
            int CustomerID;

            command.CommandText = "SELECT * FROM Customer;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            while(reader.Read())
            {
                tmpFirstName = reader.GetString("FirstName");
                tmpLastName = reader.GetString("LastName");
                tmpPhoneNumber = reader.GetString("Phonenumber");
                CustomerID = reader.GetInt32("CustomerID");

                if(tmpFirstName == FirstName)
                {
                    if(tmpLastName == LastName)
                    {
                        if (tmpPhoneNumber == PhoneNumber)
                        {
                            if (i == CustomerID)
                            {
                                status = i;
                                break;//Customer Found
                            }
                        }                    
                    }
                }
                i++;
            }
            reader.Close();

            return status;
        }

        /*
        * METHOD : FindCustomers
        *
        * DESCRIPTION : This method is used to find any users with a given last name and phone number.
        * An array of these names is returned. When a user is found, the information about the user is
        * formatted into one string to be later formatted by the method FormatCustomerInfo
        *
        * PARAMETERS : MySqlCommand command : command control for Sql
        * string LastName : The lastname given by the user
        * string PhoneNumber : The phonenumber given by the user
        *
        * RETURNS : string[] : The array of names with the given lastname/phonenumber
        * 
        */
        public string[] FindCustomers(MySqlCommand command, string LastName, string PhoneNumber)
        {
            string[] ListOfUsers = new string[1000];
            string tmpFirstName;
            string tmpLastName;
            string tmpPhoneNumber;
            int CustomerID;
            int i = 0;

            command.CommandText = "SELECT * FROM Customer;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                tmpFirstName = reader.GetString("FirstName");
                tmpLastName = reader.GetString("LastName");
                tmpPhoneNumber = reader.GetString("Phonenumber");
                CustomerID = reader.GetInt32("CustomerID");

                if (tmpLastName == LastName)
                {
                    if (tmpPhoneNumber == PhoneNumber)
                    {
                        ListOfUsers[i] = CustomerID + "." + tmpFirstName + "." + tmpLastName + "." + tmpPhoneNumber;
                        i++;
                    }                    
                }
            }
            reader.Close();

            return ListOfUsers;
        }

        /*
         * METHOD : FormatCustomerInfo
         *
         * DESCRIPTION : This method is used to format the customer info found when searching through FindCustomers
         * (Above). This is done so that it looks pretty.
         *
         * PARAMETERS : string info : The string that will be formatted
         *
         * RETURNS : string : The formatted string for displaying
         * 
         */
        public string FormatCustomerInfo(string info)
        {
            string formattedInfo = "";

            char[] delimiter = { '.' };

            string[] words = info.Split(delimiter);

            formattedInfo += "CustomerId = " + words[0] + "\n";
            formattedInfo += "FirstName = " + words[1] + "\n";
            formattedInfo += "LastName = " + words[2] + "\n";
            formattedInfo += "PhoneNumber = " + words[3] + "\n\n";

            return formattedInfo;
        }


        /*
        * METHOD : GetNewCustomerID
        *
        * DESCRIPTION : This method is used to find a new user's CustomerId. This is only called
        * if a new user has been registered. It loops through all customer Ids, and takes the value of the last
        * customer added(since it is new, it will be at the end).
        *
        * PARAMETERS : MySqlCommand command : command control for Sql
        *
        * RETURNS : int : The customers Id
        * 
        */
        public int GetNewCustomerID(MySqlCommand command)
        {
            int CustID;
            int i = 1;

            command.CommandText = "SELECT CustomerId FROM Customer;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                i++;
            }
            reader.Close();
            CustID = i;
            return CustID;
        }


        /*
         * METHOD : VerifyInventory
         *
         * DESCRIPTION : This method is used to verify the inventory levels in the database. This
         * is used to determine if an order should become pending. 2 is returned if there is not enough
         * of the product in inventory.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * string ProductName : The name of the product being ordered.
         * int Quantity : The amount of the product being ordered.
         *
         * RETURNS : int : The status of the search
         * 
         */
        public int VerifyInventory(MySqlCommand command, string ProductName, int Quantity)
        {
            int status = 0;
            int currentQuantity = 0;
            string currentProduct;
            command.CommandText = "SELECT Quantity, ProductName FROM Product;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                currentProduct = reader.GetString("ProductName");
                currentQuantity = reader.GetInt32("Quantity");
                if(ProductName == currentProduct)
                {
                    if(currentQuantity < Quantity)
                    {
                        status = 2;//Not enough inventory
                        break;
                    }
                    else
                    {
                        break;
                    }
                }              
            }
            reader.Close();
            return status;
        }

        /*
         * METHOD : UpdateInventory
         *
         * DESCRIPTION : This method is used to update inventory levels once an order is complete.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * int newQuantity : The amount of the product ordered.
         * string currentProduct : The name of the product being ordered.
         *
         * RETURNS : void
         * 
         */
        public void UpdateInventory(MySqlCommand command, int newQuantity, string currentProduct)
        {
            
            int productId = GetProductName(command, currentProduct);
            int oldQuantity = GetQunatity(command, productId);
            int quantity = oldQuantity - newQuantity;

            command.CommandText = "Update Product Set Quantity="+ quantity +" Where ProductId ="+productId+ ";";
            command.ExecuteNonQuery();
        }

        /*
         * METHOD : GetQunatity
         *
         * DESCRIPTION : This method is used to get the current quantity for a given productId.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * int ProductId : The productId used for searching.
         *
         * RETURNS : int : The quantity found from ProductId
         * 
         */
        public int GetQunatity(MySqlCommand command, int ProductId)
        {
            int Quantity = 0;
            command.CommandText = "SELECT Quantity FROM Product;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            int i = 1;
            while (reader.Read())
            {
                if(i == ProductId)
                {
                    Quantity = reader.GetInt32("Quantity");
                    break;
                }
                i++;
            }
            reader.Close();
            return Quantity;
        }

        /*
         * METHOD : GetBranchID
         *
         * DESCRIPTION : This method is used to get the branchId for a BranchName.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * string BranchName : The BranchName used for searching.
         *
         * RETURNS : int : The branchId found from BranchName
         * 
         */
        public int GetBranchID(MySqlCommand command, string BranchName)
        {
            int BranchID = 1;
            string currentBranchName;
            command.CommandText = "SELECT * FROM Branch;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                currentBranchName = reader.GetString("BranchName");

                if(currentBranchName == BranchName)
                {
                    break;
                }
                BranchID++;
            }
            reader.Close();
            return BranchID;
        }

        /*
         * METHOD : CreateOrder
         *
         * DESCRIPTION : This method is used create an order(add to order table). This is called when a new order is needed
         * to be created. It will return the orderId of the newly created order.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * int CustomerId : The CustomerID for the Order.
         * int BranchId : The BranchId for the Order.
         * int OrderStatus : The OrderStatus for the Order.
         *
         * RETURNS : int : The OrderId for the new order.
         * 
         */
        public int CreateOrder(MySqlCommand command, int CustomerId, int BranchId, string OrderStatus)
        {
            int OrderId = 0;
            string OrderDate = GetDate();
            command.CommandText = "Insert Into `Order`(CustomerId, BranchId, OrderDate, OrderStatus) values (" + CustomerId +"," + BranchId + ",'" + OrderDate + "','" + OrderStatus + "');";
            command.ExecuteNonQuery();

            command.CommandText = "Select OrderId from `Order`";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();
            int i = 0;
            while(reader.Read())
            {
                i++;
            }
            reader.Close();
            OrderId = i;

            return OrderId;
        }

        /*
         * METHOD : CreateOrder
         *
         * DESCRIPTION : This method is used to add information to the orderline. This is called when a new order is 
         * created. This method loops for every product the customer ordered in one order.(Ye) Updates database.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * int OrderID : The OrderID for the Orderline.
         * string[] listOfOrders : The list of products ordered and their quantities.
         * int amountOfOrders : The Amount of products ordered.
         * string orderStatus : Used to check if inventory needs an update.
         *
         * RETURNS : int : The OrderId for the new order.
         * 
         */
        public void CreateOrderLines(MySqlCommand command,int OrderID, string[] listOfOrders, int amountOfOrders, string orderStatus)
        {
            int i = 0;
            char[] delimiter = { '.' };
            int ProductId;

            while (i < amountOfOrders)
            {
                string[] words = listOfOrders[i].Split(delimiter);
                ProductId = GetProductName(command, words[0]);
                command.CommandText = "Insert Into OrderLine(OrderId,ProductId,Quantity) values (" + OrderID + "," + ProductId + "," + Convert.ToInt32(words[1]) + ");";
                command.ExecuteNonQuery();
                if(orderStatus == "PAID")
                {
                    UpdateInventory(command, Convert.ToInt32(words[1]), words[0]);
                }               
                i++;
            }            
        }

        /*
         * METHOD : InitializeInventoryInfo
         *
         * DESCRIPTION : This method is used verify and format everything the user has ordered. This function
         * is used to update the inventory. 
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * int OrderID : The OrderID for the Order being updated.
         *
         * RETURNS : void
         * 
         */
        public void InitializeInventoryInfo(MySqlCommand command, int OrderId)
        {
            int[] tmpProduct = new int[11];
            int[] tmpQuantity = new int[11];
            int i = 0;
            //Finds the amount of orders with the same orderId
            command.CommandText = "Select * FROM OrderLine WHERE OrderId= " + OrderId +";";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                tmpProduct[i] = reader.GetInt32("ProductId");
                tmpQuantity[i] = reader.GetInt32("Quantity");
                i++;
            }
            reader.Close();
            UpdateInventoryLevel(command, tmpProduct, tmpQuantity, i, OrderId);
                                
        }

        /*
         * METHOD : UpdateInventoryLevel
         *
         * DESCRIPTION : This method is used to add information to update the inventory. This is whenever
         * a refund request is made.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * int[] ProductId : Array of ProductIds for update.
         * int[] Quantity : Array of Quantitys for update.
         * int amountOfOrders : The Amount of products ordered.
         * int OrderId : The order to determine which orderlines should be updated.
         *
         * RETURNS : void
         * 
         */
        public void UpdateInventoryLevel(MySqlCommand command, int[] ProductId, int[] Quantity, int amountOfItems, int OrderId)
        {
            int i = 0;
            while (i < amountOfItems)
            {
                int oldQuantity = GetQunatity(command, ProductId[i]);
                int quantity = oldQuantity + Quantity[i];

                command.CommandText = "Update Product Set Quantity=" + quantity + " Where ProductId =" + ProductId[i] + ";";
                command.ExecuteNonQuery();
                command.CommandText = "Update OrderLine Set Quantity=0  Where OrderId =" + OrderId + ";";
                command.ExecuteNonQuery();
                i++;
            }
        }

        /*
         * METHOD : GetProductName
         *
         * DESCRIPTION : This method is used to get the ProductId for a ProductName.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * string ProductName : The ProductName used for searching.
         *
         * RETURNS : int : The ProductId found from ProductName
         * 
         */
        public int GetProductName(MySqlCommand command, string ProductName)
        {
            int ProductID = 0;
            string currentProduct;
            int currentProductId;
            command.CommandText = "SELECT ProductId, ProductName FROM Product;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                currentProduct = reader.GetString("ProductName");
                currentProductId = reader.GetInt32("ProductId");
                if (ProductName == currentProduct)
                {
                    ProductID = currentProductId;
                    break;
                }
            }
            reader.Close();
            return ProductID;
        }

        /*
        * METHOD : CreateSalesRecord
        *
        * DESCRIPTION : This method is used to create the formatted sales record.
        * This method first gets everything needed from the order table, then the branch table,
        * Then the customer talbe, then the orderline/producttable. This is done through a series of methods,
        * commands, and readers.
        * 
        * PARAMETERS : MySqlCommand command : command control for Sql
        * int OrderId : The orderId for the sales record being created.
        *
        * RETURNS : string : the formatted sales record ready for display.
        */
        public string CreateSalesRecord(MySqlCommand command, int OrderId)
        {
            string BranchName;
            string FirstName;
            string LastName;
            string OrderStatus;
            int[] ProductIds = new int[11];
            int[] Quantitys = new int[11];
            string[] SalesList = new string[11];
            double TotalPrice = 0;
            double HSTPrice = 0;
            double FinalPrice = 0;
            int BranchId;
            int CustomerId;
            string OrderDate;

            string FormattedSalesRecord = "*************************************\n";

            FormattedSalesRecord += "Thank you for shopping at\n";

            command.CommandText = "SELECT * FROM `Order` WHERE OrderId = " + OrderId +";";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();
            reader.Read();
            BranchId = reader.GetInt32("BranchId");
            CustomerId = reader.GetInt32("CustomerId");
            OrderDate = reader.GetString("OrderDate");
            OrderStatus = reader.GetString("OrderStatus");
            reader.Close();

            BranchName = GetBranchName(command, BranchId);
            FirstName = GetFirstName(command, CustomerId);
            LastName = GetLastName(command, CustomerId);

            FormattedSalesRecord += "Wally's World " + BranchName + "\n";
            FormattedSalesRecord += "On " + OrderDate + ", " + FirstName + " " + LastName + "!\n";
            FormattedSalesRecord += "OrderId: " + OrderId + "\n";

            command.CommandText = "SELECT * FROM OrderLine WHERE OrderId = " + OrderId + ";";
            command.ExecuteNonQuery();
            reader = command.ExecuteReader();
            int amountOfItems = 0;
            while(reader.Read())
            {
                ProductIds[amountOfItems] = reader.GetInt32("ProductId");
                Quantitys[amountOfItems] = reader.GetInt32("Quantity");
                amountOfItems++;
            }
            reader.Close();
            SalesList = GenerateSalesList(command, ProductIds, Quantitys, amountOfItems);
            int i = 0;

            while(i<amountOfItems)
            {
                char[] delimiter = { '-' };

                string[] words = SalesList[i].Split(delimiter);

                FormattedSalesRecord += words[0] + " " + Quantitys[i] + " x $" + words[1] + " = " + words[2] + "\n";

                TotalPrice += Convert.ToDouble(words[2]);
                i++;
            }

            FormattedSalesRecord += "Subtotal = $" + TotalPrice + "\n";

            HSTPrice = TotalPrice * 0.13;
            HSTPrice = Math.Round(HSTPrice, 2) ;

            FormattedSalesRecord += "HST(13%) = $" + HSTPrice + "\n";

            FinalPrice = HSTPrice + TotalPrice;

            FormattedSalesRecord += "Sale Total = $" + FinalPrice;

            if(OrderStatus == "PEND")
            {
                FormattedSalesRecord += "\n***This order is pending***";
            }
            if (OrderStatus == "RFND")
            {
                FormattedSalesRecord += "\n***This order was refunded***";
            }
            if (OrderStatus == "CNCL")
            {
                FormattedSalesRecord += "\n***This order was canceled***";
            }

            return FormattedSalesRecord;
        }

        /*
        * METHOD : GenerateSalesList
        *
        * DESCRIPTION : This method is used generate a list of items to be added to the sales record.
        * This method also calculates the total cost for each amount of product (Quantity*price)
        * 
        * PARAMETERS : MySqlCommand command : command control for Sql
        * int[] ProductId : Array of productIds from the order.
        * int[] Quantity : Array of Quantitys from the order.
        * int amountOfItems : The amount of items ordered.
        *
        * RETURNS : string[] : A list of formatted strings for printing.
        */
        public string[] GenerateSalesList(MySqlCommand command, int[] ProductId, int[] Quantity, int amountOfItems)
        {
            string[] SalesList = new string[11];
            double saleTotalItem;
            int i = 0;

            while (i < amountOfItems)
            {
                command.CommandText = "SELECT * FROM Product WHERE ProductId = " + ProductId[i] + ";";
                command.ExecuteNonQuery();
                MySqlDataReader reader = command.ExecuteReader();
                reader.Read();
                SalesList[i] += reader.GetString("ProductName") + "-";
                saleTotalItem = reader.GetDouble("Price");
                SalesList[i] += saleTotalItem + "-";//Get the price of one
                saleTotalItem = saleTotalItem * Quantity[i];//Calculate the price of all with quantity
                SalesList[i] += saleTotalItem;
                reader.Close();
                i++;
            }
            return SalesList;
        }

        /*
         * METHOD : GetBranchName
         *
         * DESCRIPTION : This method is used to get the BranchName for a BranchId.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * int BranchId : The BranchId used for searching.
         *
         * RETURNS : int : The BranchName found from BranchId
         * 
         */
        public string GetBranchName(MySqlCommand command, int BranchId)
        {
            string BranchName ="";
            string tmpBranchName = "";
            int tmpBranchId;
            command.CommandText = "SELECT * FROM Branch;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                tmpBranchName = reader.GetString("BranchName");
                tmpBranchId = reader.GetInt32("BranchId");
                if(tmpBranchId == BranchId)
                {
                    BranchName = tmpBranchName;
                    break;
                }
            }
            reader.Close();
            return BranchName;
        }

        /*
         * METHOD : GetFirstName
         *
         * DESCRIPTION : This method is used to get the FirstName for a CustomerId.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * int CustomerId : The CustomerId used for searching.
         *
         * RETURNS : int : The FirstName found from CustomerId
         * 
         */
        public string GetFirstName(MySqlCommand command, int CustomerId)
        {
            string FirstName = "";
            string tmpFirstName = "";
            int tmpCustomerId;
            command.CommandText = "SELECT * FROM Customer;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                tmpFirstName = reader.GetString("FirstName");
                tmpCustomerId = reader.GetInt32("CustomerId");
                if (tmpCustomerId == CustomerId)
                {
                    FirstName = tmpFirstName;
                    break;
                }
            }
            reader.Close();
            return FirstName;
        }

        /*
         * METHOD : GetLastName
         *
         * DESCRIPTION : This method is used to get the LastName for a CustomerId.
         *
         * PARAMETERS : MySqlCommand command : command control for Sql
         * int CustomerId : The CustomerId used for searching.
         *
         * RETURNS : int : The LastName found from CustomerId
         * 
         */
        public string GetLastName(MySqlCommand command, int CustomerId)
        {
            string LastName = "";
            string tmpLastName = "";
            int tmpCustomerId;
            command.CommandText = "SELECT * FROM Customer;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                tmpLastName = reader.GetString("LastName");
                tmpCustomerId = reader.GetInt32("CustomerId");
                if (tmpCustomerId == CustomerId)
                {
                    LastName = tmpLastName;
                    break;
                }
            }
            reader.Close();
            return LastName;
        }

        /*
        * METHOD : GetDate
        *
        * DESCRIPTION : This method is used to get the date. This is not from the
        * database, this is for order creation
        *
        * PARAMETERS : none
        *
        * RETURNS : string : The generated date.
        * 
        */
        public string GetDate()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string month;
            string day;
            string year;
            char[] delimiter = { '-' };

            string[] words = date.Split(delimiter);
            year = words[0];
            day = words[2];

            switch (words[1])
            {
                case "01":
                    month = "Jan.";
                    break;
                case "02":
                    month = "Feb.";
                    break;
                case "03":
                    month = "March";
                    break;
                case "04":
                    month = "April";
                    break;
                case "05":
                    month = "May";
                    break;
                case "06":
                    month = "June";
                    break;
                case "07":
                    month = "July";
                    break;
                case "08":
                    month = "Aug.";
                    break;
                case "09":
                    month = "Sept.";
                    break;
                case "10":
                    month = "Oct.";
                    break;
                case "11":
                    month = "Nov.";
                    break;
                case "12":
                    month = "Dec.";
                    break;
                default:
                    month = "Feb.";
                    break;
            }

            date = month+ " " + day + ", " + year;
            return date;
        }
    }
}
