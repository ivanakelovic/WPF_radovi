using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//Potrebno uključiti radi korišteenja mysql-a
using MySql.Data.MySqlClient;
//Potrebno uključiti radi korištenja DataGrid kontrole
using System.Data;

namespace Radovi
{
   public partial class MainWindow : Window
    {
        MySqlConnection konekcija = null;

        public MainWindow()
        {
            InitializeComponent();
            //Konekcioni string se pamti kao promjenljiva u Properties.Settings
            konekcija = new MySqlConnection(Properties.Settings.Default.con_string);
            try 
            {
                konekcija.Open();
                //Poziva se funkcija u kojoj se učitavaju podaci u ComboBox cmbSesija
                ucitajSesije();
                //Poziva se funkcija za prikaz svih radova ili onih kojizadovoljavaju uslov pretrage
                ucitajRadove();
            }
            catch 
            {
                MessageBox.Show("Greška prilikom konekcije na bazu!");
            }
        }

        private void lblZatvori_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Kada se klikne na labelu lblZatvori zatvara se aplikacija
            Application.Current.Shutdown();
        }

        //Funkcija pomoću koje se "puni" ComboBox cmbSesija
        private void ucitajSesije()
        {
            //U upitu se selektuju samo RAZLIČITE vrijednosti iz kolone sesija sortirano po sesiji u rastucem redosledu
            string sql = "SELECT DISTINCT sesija FROM rad ORDER BY sesija ASC";
            MySqlCommand komanda = new MySqlCommand(sql, konekcija);
            MySqlDataReader reader = komanda.ExecuteReader();

            while (reader.Read())
            {
                //Za svaki pročitani red dodaje se stavka u cmbSesija
                cmbSesije.Items.Add(reader["sesija"]);
            }
            //Uvijek nakon zavrsetka rada sa reader-om potrebno ga je zatvoriti
            reader.Close();
        }
        private void ucitajRadove()
        {
            //Definisanje promjenljive pomoću koje će se prikazati broj radova
            int brojRadova = 0;
            //Kod prikaza radova u TextBoxu inicijalno se kreira zaglavlje
            txtRadovi.Text = "ID\tSIMPOZIJUM\tSESIJA\tRB\tPOSTER\tVIRTUAL\tNASLOV\n";
            //Upit pomoću koga se selektuju svi radovi. Dodato WHERE 1 radi dodavanja ostalih uslova pretrage (da se formira WHERE dio upita)
            string sql = "SELECT id, simpozijum, sesija, rb, poster, virtual, naslov FROM rad WHERE 1";

            //Ako je nešto upisano u polje godina proširuje se upit i sa ovim uslovom
            if (txtGodina.Text != "")
                sql += " AND simpozijum = '" + txtGodina.Text + "'";
            //Ovaj uslov se uzima u obzir samo ako u naslovu imaju barem 3 karaktera upisana. Koristi se LIKE jer se pretražuje po "dijelu" naslova
            if (txtNaslov.Text.Length >= 3)
                sql += " AND naslov LIKE '%" + txtNaslov.Text + "%'";
            //Ako je neka sesija selektovana dodaje sse i ovaj uslov
            if (cmbSesije.SelectedIndex > -1)
                sql += " AND sesija = '" + cmbSesije.SelectedItem + "'";
            //Ako je označeno ovo polje dodaje se i ovaj uslov
            if (chkObjavljen.IsChecked == true)
                sql += " AND objavljen = true";

            //Nakon svih uslova dodaje se uslov za sortiranje jer ovaj uslov ide uvijek na kraju upita
            sql += " ORDER BY sesija, rb ASC";
            MySqlCommand komanda = new MySqlCommand(sql, konekcija);
            MySqlDataReader reader = komanda.ExecuteReader();

            while (reader.Read())
            {
                //Podaci za svako polje su izvučeni u promjenljive radi preglednosti i jednostavnijeg korištenja
                string id = reader["id"].ToString();
                string simpozijum = reader["simpozijum"].ToString();
                string sesija = reader["sesija"].ToString();
                string rb = reader["rb"].ToString();
                string poster = bool.Parse(reader["poster"].ToString()) == true ? "DA" : "NE";
                string virt = bool.Parse(reader["virtual"].ToString()) == true ? "DA" : "NE";
                string naslov = reader["naslov"].ToString();
                //Podaci o svakom radu se dodaju u textbox
                txtRadovi.Text += id + "\t" + simpozijum + "\t\t" + sesija + "\t" + rb + "\t" + poster + "\t" + virt + "\t" + naslov + "\n";

                //Svaki rad se broji (inkrementira se vrijednost promjenljive brojRadova)
                brojRadova++;
            }
            //Uvijek nakon zavrsetka rada sa reader-om potrebno ga je zatvoriti
            reader.Close();
            //Broj radova se ispisuje na odgovarajućoj labeli
            lblBrojRadova.Content = "Broj prikazanih radova: " + brojRadova;

            //UČITAVANJE PODATAKA U DATAGRID
            //Upit se izvršava i podaci se smještaju u objekat tipa DataAdapter
            MySqlDataAdapter da = new MySqlDataAdapter(sql, konekcija);
            //Kreira se objekat tipa DataTable u koji će se smjestiti podaci iz DataAdapter-a
            DataTable tabela = new DataTable();
            //MySqlDataAdaper da puni podacima DataTable dt
            da.Fill(tabela);
            //U DataGridu se prilazuju podaci iz DataTable-a dt
            dgRadovi.ItemsSource = tabela.DefaultView;
        }

        private void btnPretraga_Click(object sender, RoutedEventArgs e)
        {
            //Kada se klikne na taster Pretraga poziva se funkcija ucitajRadove()
            ucitajRadove();
        }

        private void btnPonisti_Click(object sender, RoutedEventArgs e)
        {
            //Funkcija pomoću koje se poništavaju parametri pretrage
            txtGodina.Clear();
            txtNaslov.Clear();
            cmbSesije.SelectedIndex = -1;
            chkObjavljen.IsChecked = false;

            //Poziva se funkcija ucitajRadove(), da bi se osvježio prikaz nakon resetovanja svih polja (to jeste ucitali svi radovi)
            ucitajRadove();
        }
    }
}
