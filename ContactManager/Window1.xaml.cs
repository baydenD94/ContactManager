using System;
using System.Data.SQLite;
using System.Windows;

namespace ContactManager
{
    public partial class Window1 : Window
    {
        private int contactIndex;
        private MainWindow mainWindow;

        public Window1(MainWindow mainWindow, int contactIndex)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.contactIndex = contactIndex;
            LoadContactDetails();
        }

        private void update_Click(object sender, RoutedEventArgs e)
        {
            int contactId = mainWindow.GetContactId(contactIndex);
            string fName = viewFN.Text;
            string lName = viewLN.Text;
            string phone = viewPN.Text;
            string email = viewE.Text;

            UpdateContact(contactId, fName, lName, phone, email);
        }

        private void LoadContactDetails()
        {
            int contactId = mainWindow.GetContactId(contactIndex);

            using (var connection = MainWindow.GetFileDatabaseConnection())
            {
                string sqlStatement = @"SELECT *
                                        FROM Contacts
                                        WHERE ID = @id";

                using (var cmd = new SQLiteCommand(sqlStatement, connection))
                {
                    cmd.Parameters.AddWithValue("@id", contactId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            viewFN.Text = (reader["fName"].ToString());
                            viewLN.Text = (reader["lName"].ToString());
                            viewPN.Text = (reader["phone"].ToString());
                            viewE.Text = (reader["email"].ToString());
                        }
                    }
                }
            }
        }

        private void UpdateContact(int contactId, string fName, string lName, string phone, string email)
        {
            using (var connection = MainWindow.GetFileDatabaseConnection())
            {
                string sqlStatement = @"
                    UPDATE Contacts 
                    SET fName = @fName, lName = @lName, phone = @phone, email = @email
                    WHERE ID = @contactId";

                using (var cmd = new SQLiteCommand(sqlStatement, connection))
                {
                    cmd.Parameters.AddWithValue("@fName", fName);
                    cmd.Parameters.AddWithValue("@lName", lName);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@contactId", contactId);

                    cmd.ExecuteNonQuery();
                }
            }

            // Notify the main window to reload contacts and update the UI
            mainWindow.LoadContacts();

            // Close the current window after updating
            Close();
        }
    }
}
