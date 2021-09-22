using Npgsql;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
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

namespace WinTermOverview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private int machinenumber;
        private string machinename;
        private string product;
        private string osinfo;
        private string installedversion;
        private bool terminaldebug;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            int vl = TextBoxVenLoc.Text.Length;
            if (vl != 7)
            {
                MessageBox.Show("Must be 7 digit number!");
                return;
            }
            GetTermMuiDataAsync();
        }

        private async Task GetTermMuiDataAsync()
        {
            List<String> machineUids = new List<String>();
            string sql;
            DatabaseObjectClass awsConn = new DatabaseObjectClass("postgres", "26.0.0.5", "retriever");
#if DEBUG
            awsConn.Connection.Host = "localhost";
            awsConn.Connection.Password = "Harvard%2525";
#endif

            sql = "SELECT * FROM newdevices WHERE venuelocation = " + TextBoxVenLoc.Text;
            DataSet dsAWS = awsConn.ReturnDataset(sql);

            if (dsAWS.Tables[0].Rows.Count == 0)
            {
                MessageBox.Show("This venuelocation doesn't exist.");
                return;
            }

            foreach (DataRow dr in dsAWS.Tables[0].Rows)
            {
                machineUids.Add(Convert.ToString(dr["machineuid"]));

            }
            //MessageBox.Show(String.Join("\n", machineUids));

            int r = 1;
            int c = 0;
            int elem = 0;
            foreach (string muId in machineUids)
            {
                await GetFromApi(muId);
                CreateDynamicStackPanel(r, c);


                if ((elem + 1) % 3 == 0)
                {
                    c = 0;
                    r++;
                }
                else
                {

                    c++;
                }
                elem++;

            }
        }

        public void CreateDynamicStackPanel(int r, int c)
        {
            StackPanel dynamicStackPanel = new StackPanel();
            dynamicStackPanel.Width = 200;
            dynamicStackPanel.Height = 200;
            dynamicStackPanel.Orientation = Orientation.Vertical;
            Label lbl1 = new Label();
            lbl1.Content = machinenumber;
            lbl1.Foreground = Brushes.White;
            dynamicStackPanel.Children.Add(lbl1);
            Label lbl2 = new Label();
            lbl2.Content = machinename;
            lbl2.Foreground = Brushes.LimeGreen;
            dynamicStackPanel.Children.Add(lbl2);
            Label lbl3 = new Label();
            lbl3.Content = product;
            lbl3.Foreground = Brushes.MediumBlue;
            dynamicStackPanel.Children.Add(lbl3);
            Label lbl4 = new Label();
            lbl4.Content = installedversion;
            lbl4.Foreground = Brushes.BlueViolet;
            dynamicStackPanel.Children.Add(lbl4);
            Label lbl5 = new Label();
            lbl5.Content = terminaldebug;
            lbl5.Foreground = Brushes.Orange;
            dynamicStackPanel.Children.Add(lbl5);
            Grid.SetRow(dynamicStackPanel, r);
            Grid.SetColumn(dynamicStackPanel, c);
            Main.Children.Add(dynamicStackPanel);

        }
        private async Task GetFromApi(string machineuid)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://retrieverdigital.com");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("machineuid", machineuid.ToString()),
            });

            try
            {
                HttpResponseMessage response = await client.PostAsync("/windowsterm/api/Terminal.php", content);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    JObject json = Newtonsoft.Json.Linq.JObject.Parse(data.ToString());

                    if (json != null && json.Count > 2)
                    {
                        
                        machinenumber = int.Parse(json["machinenumber"].ToString());
                        machinename = json["machinename"].ToString();
                        product = json["product"].ToString();
                        osinfo = json["osinfo"].ToString();
                        installedversion = json["installedversion"].ToString();
                        terminaldebug = bool.Parse(json["terminaldebug"].ToString() == "t" ? "true" : "false");
                    }
                }
                else
                {
                    //new rtlog(rtlog_type.error, "{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);

                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

    }
}
