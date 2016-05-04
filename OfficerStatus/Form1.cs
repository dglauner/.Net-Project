using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OfficerStatus
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadMe();
        }

        private void LoadMe()
        {
            //Set dummy Lat/Long
            this.txtLat.Text = "100";
            this.txtLon.Text = "200";

            //Build Officer list
            var myDataSource = new List<myItem>();
            myDataSource.Add(new myItem() { Name = "Ted", Value = "Ted" });
            myDataSource.Add(new myItem() { Name = "Al", Value = "Al" });
            myDataSource.Add(new myItem() { Name = "Dan", Value = "Dan" });

            //Setup data binding
            this.cmbOfficer.DataSource = myDataSource;
            this.cmbOfficer.DisplayMember = "Name";
            this.cmbOfficer.ValueMember = "Value";

            // make it readonly
            this.cmbOfficer.DropDownStyle = ComboBoxStyle.DropDownList;

            //Build Stutus Code list
            var myDataSource2 = new List<myItem>();
            myDataSource2.Add(new myItem() { Name = "Break", Value = "1" });
            myDataSource2.Add(new myItem() { Name = "Patrolling", Value = "2" });
            myDataSource2.Add(new myItem() { Name = "Engaged", Value = "3" });
            myDataSource2.Add(new myItem() { Name = "Under Fire", Value = "4" });
            myDataSource2.Add(new myItem() { Name = "Off Duty", Value = "5" });

            //Setup data binding
            this.cmbStatus.DataSource = myDataSource2;
            this.cmbStatus.DisplayMember = "Name";
            this.cmbStatus.ValueMember = "Value";

            // make it readonly
            this.cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            ServiceReference1.officerInfo myInfo = new ServiceReference1.officerInfo();

            myInfo.officerId = cmbOfficer.SelectedValue.ToString();
            myInfo.latitude = Convert.ToDouble(txtLat.Text);
            myInfo.longitude = Convert.ToDouble(txtLon.Text);
            myInfo.Status = Convert.ToInt16(cmbStatus.SelectedValue);
            
            ServiceReference1.Service1Client myservice = new ServiceReference1.Service1Client();

            myInfo.Publickey = myservice.GetPublicKey();

            bool response = myservice.SetStatus(myInfo);
            lblResponse.Text = response.ToString();
            timer1.Interval = 2000;
            timer1.Enabled = true;
            timer1.Start();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblResponse.Text = "...waiting";
            timer1.Enabled = false;
        }
    }

    public class myItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
