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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

/*
* FILE : MainWindow.xaml.cs
* PROJECT : PROG2110 - Assignment #4 - Revenge of Wally's World
* PROGRAMMER : Anthony Bastos
* FIRST VERSION : 2016-12-07
* DESCRIPTION :
* The functions in this file are used to create the logic for a POS system for Wally's World. This file conatins
* all of the events that can happen within my application. This includes all button clicks. See MainWindow.xaml for
* the layout. Button events include log in, Add items, Complete order, log out, Searching and many more seen in more detail below.
*** NOTE : My reading from sql tables was taken from here: http://stackoverflow.com/questions/12408693/how-to-read-and-print-out-data-from-mysql-in-c-sharp
* I used this as a guidline for looking through tables in my database and applied it wherever necessary.
*/

namespace POSAsgn4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlCommand command = new MySqlCommand();//The command used for interacting with MySql
        bool firstTime = true;//Idication if this is the first time add was used.
        int amountOfItems = 0;//keeps track of the amount of items added.
        string[] OrderList = new string[10];//Keeps track of the orders made by the customer
        string OrderStatus = "PAID";//Determines the status of the current order
        int CustomerID;//Determines the Customers ID.
        POSSupportingClass VerifyOrder = new POSSupportingClass();//Used to make calls to the supporting class

        /*
        * METHOD : MainWindow
        *
        * DESCRIPTION : This method is the constructor for MainWindow. Here, I set up my connection to the 
        * database
        *
        * PARAMETERS : none
        *
        * RETURNS : none
        * 
        */
        public MainWindow()
        {
            InitializeComponent();
            MySqlConnection connection = null;
            string connectStr = "";
            connectStr = "server=127.0.0.1;uid=root;pwd=Conestoga1;database=abwally";//Change password later
            connection = new MySqlConnection(connectStr);
            connection.Open();
            command.Connection = connection;            
        }

        /*
        * METHOD : StaffMember_Checked
        *
        * DESCRIPTION : This method is used to hide/allow certain features when a textbox is checked.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void StaffMember_Checked(object sender, RoutedEventArgs e)
        {
            CustomerBox.IsChecked = false;
            VeiwOrders.IsEnabled = true;
            CheckOrderTab.IsEnabled = true;            
        }

        /*
        * METHOD : CustomerBox_Checked
        *
        * DESCRIPTION : This method is used to hide/allow certain features when a textbox is checked.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void CustomerBox_Checked(object sender, RoutedEventArgs e)
        {
            if(VeiwOrders != null)
            {
                StaffMember.IsChecked = false;
                VeiwOrders.IsEnabled = false;
                CheckOrderTab.IsEnabled = false;
            }            
        }

        /*
        * METHOD : VeiwInventory_Click
        *
        * DESCRIPTION : This method is used to display the current inventory levels for the database when clicked.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void VeiwInventory_Click(object sender, RoutedEventArgs e)
        {
            InventoryList.Items.Clear();
            command.CommandText = "SELECT Quantity, ProductName FROM Product;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            int i = 0;
            while(reader.Read())
            {
                InventoryList.Items.Insert(i, reader.GetString("ProductName") + " === " + reader.GetString("Quantity") );
                i++;
            }
            if(i == 0)
            {
                InventoryList.Items.Insert(i, "There is no inventory");
            }

            reader.Close();
        }

        /*
        * METHOD : checkOrdersBtn_Click
        *
        * DESCRIPTION : This method is used to find Orders with a given orderId in the CheckOrders Tab when clicked.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void checkOrdersBtn_Click(object sender, RoutedEventArgs e)
        {
            OrderListTxtBox.Text = "";
            command.CommandText = "SELECT * FROM `Order` WHERE OrderID = " + orderCheckTxtBox.Text + ";";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.Read() == true)
            {
                OrderListTxtBox.Text += "OrderID = " + reader.GetString("OrderID") + "\n";
                OrderListTxtBox.Text += "OrderStatus = " + reader.GetString("OrderStatus") + "\n";
                OrderListTxtBox.Text += "CustomerID = " + reader.GetString("CustomerID") + "\n";
                OrderListTxtBox.Text += "BranchID = " + reader.GetString("BranchID") + "\n";
                OrderListTxtBox.Text += "OrderDate = " + reader.GetString("OrderDate") + "\n";
                reader.Close();
            }
            else
            {
                OrderListTxtBox.Text = "There are no order with that ID";
                reader.Close();
            }
        }

        /*
        * METHOD : SubmitOrder_Click
        *
        * DESCRIPTION : This method is used to submit an order (More like add an item to a list) when clicked.
        * This method also checks the inventory levels, in order to update the order status.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void SubmitOrder_Click(object sender, RoutedEventArgs e)
        {           
           int status;                       
           InfoTxtBox.Text = "";

            if (locationCombo.Text != "")
            {
                if (itemSelectionBox.Text != "")
                {
                    status = VerifyOrder.VerifyInventory(command, itemSelectionBox.Text, Convert.ToInt32(quantityBox.Text));

                    if (status == 2)
                    {
                        InfoTxtBox.Text += "Not enough items in inventory\n Status will now be PENDING(PEND)\n\n";
                        OrderStatus = "PEND";
                    }
                    else
                    {
                        InfoTxtBox.Text += "There are enough items in inventory!\n\n";
                    }

                    if (amountOfItems >= 10)
                    {
                        InfoTxtBox.Text += "No more then 10 items per customer!";
                    }
                    else
                    {
                        InfoTxtBox.Text += "ADDED:\n" + itemSelectionBox.Text + " X " + Convert.ToInt32(quantityBox.Text);
                        OrderList[amountOfItems] = itemSelectionBox.Text + "." + Convert.ToInt32(quantityBox.Text);
                        amountOfItems++;
                    }
                    completeOrder.IsEnabled = true;
                }
            }
        }

        /*
        * METHOD : completeOrder_Click
        *
        * DESCRIPTION : This method is used to Complete an order for the user when clicked.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void completeOrder_Click(object sender, RoutedEventArgs e)
        {
            int BranchID = VerifyOrder.GetBranchID(command, locationCombo.Text);
            int OrderId;
            string SalesRecord;

            OrderId = VerifyOrder.CreateOrder(command, CustomerID, BranchID, OrderStatus);

            VerifyOrder.CreateOrderLines(command, OrderId, OrderList, amountOfItems, OrderStatus);

            SalesRecord = VerifyOrder.CreateSalesRecord(command, OrderId);

            MessageBox.Show(SalesRecord);
            InfoTxtBox.Text = SalesRecord;

            firstTime = true;
            completeOrder.IsEnabled = false;
            OrderList = new string[10];
            amountOfItems = 0;
            OrderStatus = "PAID";
        }

        /*
        * METHOD : RegisterBtn_Click
        *
        * DESCRIPTION : This method is used to register a new user to the database and allow this user
        * to use the system. If the user already exists, then they are not added and their information is
        * used instead. A new user can take control when the change user button is selected.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {            
            int status;

            if (firstTime)
            {
                status = VerifyOrder.VerifyCustomer(command, firstNameBox.Text, lastNameBox.Text, phoneNumberBox.Text);

                if (status == 0)
                {
                    InfoTxtBox.Text = "New Customer Found!\n\n";
                    command.CommandText = "INSERT INTO Customer(FirstName, LastName, PhoneNumber) values ('" + firstNameBox.Text + "','" + lastNameBox.Text + "','" + phoneNumberBox.Text + "');";
                    command.ExecuteNonQuery();
                    CustomerID = VerifyOrder.GetNewCustomerID(command) - 1;
                }
                else
                {
                    InfoTxtBox.Text = "Existing Customer Found!\n\n";
                    CustomerID = status;
                }
                //Below, contains anything i need to hide/allow on my menu
                FirstNameLbl.IsEnabled = false;
                lastNameLbl.IsEnabled = false;
                PhoneLbl.IsEnabled = false;

                QuantityLbl.IsEnabled = true;
                ItemLabel.IsEnabled = true;
                LocationLabel.IsEnabled = true;

                RequestCancelRefund.IsEnabled = true;
                locationCombo.IsEnabled = true;
                itemSelectionBox.IsEnabled = true;
                quantityBox.IsEnabled = true;
                SubmitOrder.IsEnabled = true;
                logOutBtn.IsEnabled = true;

                RegisterBtn.IsEnabled = false;
                firstNameBox.IsEnabled = false;
                lastNameBox.IsEnabled = false;
                phoneNumberBox.IsEnabled = false;
                firstTime = false;
                
            }
        }

        /*
        * METHOD : logOutBtn_Click
        *
        * DESCRIPTION : This method is used to change users and allow a new user to complete orders/Use the system.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void logOutBtn_Click(object sender, RoutedEventArgs e)
        {
            firstTime = true;
            completeOrder.IsEnabled = false;
            OrderList = new string[10];
            amountOfItems = 0;
            OrderStatus = "PAID";
            ListOfCustomerOrders.Items.Clear();

            InfoTxtBox.Text = "";
            firstTime = true;
            completeOrder.IsEnabled = false;
            OrderList = new string[10];
            FirstNameLbl.IsEnabled = true;
            lastNameLbl.IsEnabled = true;
            PhoneLbl.IsEnabled = true;

            QuantityLbl.IsEnabled = false;
            ItemLabel.IsEnabled = false;
            LocationLabel.IsEnabled = false;

            //Setup next step
            RequestCancelRefund.IsEnabled = false;
            locationCombo.IsEnabled = false;
            itemSelectionBox.IsEnabled = false;
            quantityBox.IsEnabled = false;
            SubmitOrder.IsEnabled = false;
            logOutBtn.IsEnabled = false;

            RegisterBtn.IsEnabled = true;
            firstNameBox.IsEnabled = true;
            lastNameBox.IsEnabled = true;
            phoneNumberBox.IsEnabled = true;
            firstTime = true;

            locationCombo.Text = "";
            itemSelectionBox.Text = "";
        }

        /*
        * METHOD : FindOrdersPEND_Click
        *
        * DESCRIPTION : This method is used to find any pending orders for the current user using the system.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void FindOrdersPEND_Click(object sender, RoutedEventArgs e)
        {
            ListOfCustomerOrders.Items.Clear();
            int tmpCustomerId;
            string Status;
            int i = 0;

            command.CommandText = "SELECT * FROM `Order`;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                tmpCustomerId = reader.GetInt32("CustomerId");
                Status = reader.GetString("OrderStatus");
                if (CustomerID == tmpCustomerId)
                {
                    if (Status == "PEND")
                    {
                        ListOfCustomerOrders.Items.Insert(i,"OrderId: " + reader.GetInt32("OrderId") + " Date: " + reader.GetString("OrderDate") + " BranchId: " + reader.GetInt32("BranchId") + " OrderStatus: " + reader.GetString("OrderStatus"));
                        i++;
                    }
                }
            }
            reader.Close();
            CancelSelectionBtn.IsEnabled = true;
            FindOrdersPaid.IsEnabled = false;
        }

        /*
        * METHOD : CancelSelectionBtn_Click
        *
        * DESCRIPTION : This method is used to cancel a selected pending order.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void CancelSelectionBtn_Click(object sender, RoutedEventArgs e)
        {
            string selectedItem = (string)ListOfCustomerOrders.SelectedItem;
            int OrderId;

            if (selectedItem != null)
            {
                char[] delimiter = { ' ' };

                string[] words = selectedItem.Split(delimiter);

                OrderId = Convert.ToInt32(words[1]);

                command.CommandText = "Update `Order` Set OrderStatus='CNCL' Where OrderId =" + OrderId + ";";
                command.ExecuteNonQuery();

                ListOfCustomerOrders.Items.Remove(selectedItem);                               
            }
            CancelSelectionBtn.IsEnabled = false;
            FindOrdersPaid.IsEnabled = true;
            ListOfCustomerOrders.Items.Clear();
            ListOfCustomerOrders.Items.Insert(0, "Order Cancellations Completed");
        }

        /*
        * METHOD : FindOrdersPaid_Click
        *
        * DESCRIPTION : This method is used to find any Paid orders for refunds.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void FindOrdersPaid_Click(object sender, RoutedEventArgs e)
        {
            ListOfCustomerOrders.Items.Clear();
            int tmpCustomerId;
            string Status;
            int i = 0;

            command.CommandText = "SELECT * FROM `Order`;";
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                tmpCustomerId = reader.GetInt32("CustomerId");
                Status = reader.GetString("OrderStatus");
                if (CustomerID == tmpCustomerId)
                {
                    if (Status == "PAID")
                    {
                        ListOfCustomerOrders.Items.Insert(i, "OrderId: " + reader.GetInt32("OrderId") + " Date: " + reader.GetString("OrderDate") + " BranchId: " + reader.GetInt32("BranchId") + " OrderStatus: " + reader.GetString("OrderStatus"));
                        i++;
                    }
                }
            }
            reader.Close();
            RefundSelectedBtn.IsEnabled = true;
            FindOrdersPEND.IsEnabled = false;
        }

        /*
        * METHOD : RefundSelectedBtn_Click
        *
        * DESCRIPTION : This method is used to refund a selected order.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void RefundSelectedBtn_Click(object sender, RoutedEventArgs e)
        {
            string selectedItem = (string)ListOfCustomerOrders.SelectedItem;
            int OrderId;

            if (selectedItem != null)
            {
                char[] delimiter = { ' ' };

                string[] words = selectedItem.Split(delimiter);

                OrderId = Convert.ToInt32(words[1]);

                command.CommandText = "Update `Order` Set OrderStatus='RFND' Where OrderId =" + OrderId + ";";
                command.ExecuteNonQuery();

                VerifyOrder.InitializeInventoryInfo(command, OrderId);

                ListOfCustomerOrders.Items.Remove(selectedItem);               
            }
            RefundSelectedBtn.IsEnabled = false;
            FindOrdersPEND.IsEnabled = true;
            ListOfCustomerOrders.Items.Clear();
            ListOfCustomerOrders.Items.Insert(0, "Order Refunding Completed");
        }

        /*
        * METHOD : GenerateSalesRecordStaffBtn_Click
        *
        * DESCRIPTION : This method is used to generate a sales record for a given sales record.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void GenerateSalesRecordStaffBtn_Click(object sender, RoutedEventArgs e)
        {
            string SalesRecord = VerifyOrder.CreateSalesRecord(command, Convert.ToInt32(orderCheckTxtBox.Text));
            OrderListTxtBox.Text = SalesRecord;
        }

        /*
        * METHOD : FindUserBtn_Click
        *
        * DESCRIPTION : This method is used to find any users with the given last name and phone number.
        *
        * PARAMETERS : object sender: the calling object
        * RoutedEventArgs e : The event arguments
        *
        * RETURNS : void  
        * 
        */
        private void FindUserBtn_Click(object sender, RoutedEventArgs e)
        {
            OrderListTxtBox.Text = "";
            string[] ListOfUsers = new string[1000];
            int i = 0;
            if(lastNameSearchBox.Text != "")
            {
                if(PhoneNumberSearchBox.Text != "")
                {
                    ListOfUsers = VerifyOrder.FindCustomers(command, lastNameSearchBox.Text, PhoneNumberSearchBox.Text);
                    while (i < ListOfUsers.Length)
                    {
                        if(ListOfUsers[i] == null)
                        {
                            break;
                        }
                        OrderListTxtBox.Text += VerifyOrder.FormatCustomerInfo(ListOfUsers[i]);
                        i++;
                    }
                }
            }
            
        }
    }
}
