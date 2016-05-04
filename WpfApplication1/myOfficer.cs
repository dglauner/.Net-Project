using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E33DGLAUNER
{
    //Officer Collection object
    public class myOfficer
    {
        private List<myItem> myStatusList = new List<myItem>();
        private string myStatus;

        public myOfficer()
        {
            //Build Stutus Code list
            myStatusList.Add(new myItem() { Name = "Break", Value = "1" });
            myStatusList.Add(new myItem() { Name = "Patrolling", Value = "2" });
            myStatusList.Add(new myItem() { Name = "Engaged", Value = "3" });
            myStatusList.Add(new myItem() { Name = "Under Fire", Value = "4" });
            myStatusList.Add(new myItem() { Name = "Off Duty", Value = "5" });
        }

        public string Id { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double distance { get; set; }
        public Microsoft.Maps.MapControl.WPF.Location loc
        {
            get
            {
                return new Microsoft.Maps.MapControl.WPF.Location(this.latitude, this.longitude);
            }
        }
        public string addr { get; set; }
        public string status
        {
            get
            {
                return myStatusList.Find(item => item.Value == myStatus).Name;
            }
            set
            {
                myStatus = value;
            }
        }
    }

}
