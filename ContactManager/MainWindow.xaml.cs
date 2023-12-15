using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
using System.Xml.Linq;
using WPF_contact_manager;

namespace ContactManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int currentContactIndex = -1;
        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadContacts();
        }
        private void InitializeDatabase()
        {
            using (var connection = GetFileDatabaseConnection())
            {
                string sqlStatement = @"
                    SELECT count(name) 
                    FROM sqlite_master
                    WHERE (
                        type = 'table' AND name = 'Contacts'
                    )";
                var cmdCheck = new SQLiteCommand(sqlStatement, connection);

                if ((long)cmdCheck.ExecuteScalar() == 0)
                {
                    using (var cmd = new SQLiteCommand(connection))
                    {
                        sqlStatement = @"
                            CREATE TABLE Contacts (
                                ID INTEGER PRIMARY KEY,
                                fName VARCHAR2(20),
                                lName VARCHAR2(20),
                                phone VARCHAR2(20),
                                email VARCHAR2(30)
                             )";

                        cmd.CommandText = sqlStatement;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static SQLiteConnection GetFileDatabaseConnection()
        {
            var connection = new SQLiteConnection("Data Source=rolodex.db");
            connection.Open();
            return connection;
        }
        private void AddContact(string fName, string lName, string phone, string email)
        {
            // Get the SQLite database connection.
            using (var connection = GetFileDatabaseConnection())
            {
                // Prepare the SQL statement to insert a new contact.
                string sqlStatement = @"
                    INSERT INTO Contacts (fName, lName, phone, email)
                    VALUES (@fName, @lName, @phone, @email)";

                // Prepare the command object with parameters.
                using (var cmd = new SQLiteCommand(sqlStatement, connection))
                {
                    cmd.Parameters.AddWithValue("@fName", fName);
                    cmd.Parameters.AddWithValue("@lName", lName);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@email", email);


                    // Execute the command to insert the new contact.
                    cmd.ExecuteNonQuery();
                }
            }

            // Reload contacts to update the ListBox.
            LoadContacts();
        }
        private void add_Click(object sender, RoutedEventArgs e)
        {

            string newfName = FN.Text;
            string newlName = LN.Text;
            string newPhone = PN.Text;
            string newEmail = E.Text;

            AddContact(newfName, newlName, newPhone, newEmail);

            FN.Text = "";
            LN.Text = "";
            PN.Text = "";
            E.Text = "";

        }
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (currentContactIndex > 0)
            {
                currentContactIndex--;
                lstContacts.SelectedIndex = currentContactIndex;
            }
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (currentContactIndex < lstContacts.Items.Count - 1)
            {
                currentContactIndex++;
                lstContacts.SelectedIndex = currentContactIndex;
            }
        }
        private void DeleteContact(int contactId)
        {
            using (var connection = GetFileDatabaseConnection())
            {
                string sqlStatement = "DELETE FROM Contacts WHERE ID = @contactId";
                using (var cmd = new SQLiteCommand(sqlStatement, connection))
                {
                    cmd.Parameters.AddWithValue("@contactId", contactId);
                    cmd.ExecuteNonQuery();
                }
            }

            // Reload contacts to update the ListBox.
            LoadContacts();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstContacts.SelectedIndex >= 0)
            {
                // Retrieve the contact ID from the selected index in the ListBox.
                int contactId = GetContactId(lstContacts.SelectedIndex);

                // Delete the contact from the UI and database.
                DeleteContact(contactId);

                // Update the selected index if necessary.
                if (currentContactIndex >= lstContacts.Items.Count)
                {
                    currentContactIndex = lstContacts.Items.Count - 1;
                }

                lstContacts.SelectedIndex = currentContactIndex;
            }
        }

        // Helper method to get the contact ID based on the selected index in the ListBox.
        public int GetContactId(int selectedIndex)
        {
            using (var connection = GetFileDatabaseConnection())
            {
                var command = new SQLiteCommand("SELECT ID FROM Contacts LIMIT 1 OFFSET @index", connection);
                command.Parameters.AddWithValue("@index", selectedIndex);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private void btnDetails_Click(object sender, RoutedEventArgs e)
        {
            Window5 w5 = new Window5();
            using (var connection = GetFileDatabaseConnection())
            {
                string sqlStatement = @"SELECT *
                                FROM Contacts
                                WHERE ID = @id";

                using (var cmd = new SQLiteCommand(sqlStatement, connection))
                {

                    cmd.Parameters.AddWithValue("@id", GetContactId(lstContacts.SelectedIndex));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            w5.viewFN.Text = (reader["fName"].ToString());
                            w5.viewLN.Text = (reader["lName"].ToString());
                            w5.viewPN.Text = (reader["phone"].ToString());
                            w5.viewE.Text = (reader["email"].ToString());

                        }
                    }
                }
            }

            w5.Show();
            LoadContacts();
        }
        private void edit_Click(object sender, RoutedEventArgs e)
        {
            Window1 w1 = new Window1(this, lstContacts.SelectedIndex);
            using (var connection = GetFileDatabaseConnection())
            {
                string sqlStatement = @"SELECT *
                                FROM Contacts
                                WHERE ID = @id";

                using (var cmd = new SQLiteCommand(sqlStatement, connection))
                {

                    cmd.Parameters.AddWithValue("@id", GetContactId(lstContacts.SelectedIndex));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            w1.viewFN.Text = (reader["fName"].ToString());
                            w1.viewLN.Text = (reader["lName"].ToString());
                            w1.viewPN.Text = (reader["phone"].ToString());
                            w1.viewE.Text = (reader["email"].ToString());

                        }
                    }
                }
            }

            w1.Show();
            LoadContacts();
        }



        public void LoadContacts()
        {

            lstContacts.Items.Clear(); // Clear existing items before reloading
            using (var connection = GetFileDatabaseConnection())
            {
                var command = new SQLiteCommand("SELECT fName FROM Contacts", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lstContacts.Items.Add(reader["fName"].ToString());

                    }
                }
            }

            if (lstContacts.Items.Count > 0)
            {
                currentContactIndex = 0;
                lstContacts.SelectedIndex = currentContactIndex;
            }
        }
        private void export_Click(object sender, RoutedEventArgs e)
        {
            // Prompt user for the file save location
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Save Database",
                DefaultExt = "txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string filePath = "C:\\Users\\bayde\\source\\repos\\export.txt";

                // Export database to the selected file
                ExportDatabaseToText(filePath);

                MessageBox.Show("Database exported successfully.", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportDatabaseToText(string filePath)
        {
            using (var connection = GetFileDatabaseConnection())
            {
                // Retrieve all data from the 'Contacts' table
                var command = new SQLiteCommand("SELECT * FROM Contacts", connection);

                using (var reader = command.ExecuteReader())
                {
                    // Open a StreamWriter to write to the file
                    using (var writer = new System.IO.StreamWriter(filePath))
                    {
                        // Write header with column names
                        writer.WriteLine("ID,First Name,Last Name,Phone,Email");

                        // Write data to the file
                        while (reader.Read())
                        {
                            writer.WriteLine($"{reader["ID"]},{reader["fName"]},{reader["lName"]},{reader["phone"]},{reader["email"]}");
                        }
                    }
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
