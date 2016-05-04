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
using CSCIE237LegacyComponent;
using System.Runtime.InteropServices;
using E33DGLAUNER;
using System.ComponentModel;
using VBA;
using Microsoft.Maps.MapControl.WPF;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using BingMapsRESTService.Common.JSON;

namespace E33DGLAUNER
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        //Databound wait label
        public MyWaitLabelViewModel labelData { get; set; }
        //Databound list for combobox
        public List<CodeObj> Codedata { get; set; }
        //Databound selected item for combobox
        public CodeObj mySelectedValue
        {
            get;
            set;
        }
        //Databound list for combobox
        public List<myItem> Statedata { get; set; }
        //Databound selected item for combobox
        public myItem mySelectedState
        {
            get;
            set;
        }
        //Storage for the Officers collection
        private List<myOfficer> officers = new List<myOfficer>();
        //Store the incident origin point
        Microsoft.Maps.MapControl.WPF.Location originLoc;

        [DllImport("Ole32.dll")]
        public static extern void CoFreeUnusedLibraries();

        Communicator myCommunicator;

        public MainWindow()
        {
            //Creat the wait label object
            labelData = new MyWaitLabelViewModel();
            labelData.MyWaitLabelOutput = "Waiting...";
            //Load Com Codes in Bound list
            loadCodes();
            //Load State Codes
            loadStates();
            //Create myCommunicator
            myCommunicator = new Communicator();
            myCommunicator.Callback += MyCommunicator_Callback;
            //Init the form
            InitializeComponent();
            //Init the map
            initMap();
        }

        private void initMap()
        {
            myMap.Center = new Microsoft.Maps.MapControl.WPF.Location(42.3770068, -71.1188541);
            myMap.ZoomLevel = 16;
        }

        public void Route(string start, string end, string key, Action<Response> callback)
        {
            Uri requestURI = new Uri(string.Format("http://dev.virtualearth.net/REST/V1/Routes/Driving?wp.0={0}&wp.1={1}&rpo=Points&key={2}", Uri.EscapeDataString(start), Uri.EscapeDataString(end), key));
            GetResponse(requestURI, callback);
        }

        private void AddPushPin(double lat, double lon, string text, Map myMap, bool isIncident, string ToolTipText)
        {
            Pushpin pin = new Pushpin();
            ControlTemplate template;
            if (!isIncident)
            {
                template = (ControlTemplate)this.FindResource("CutomPushpinTemplate");
            }
            else
            {
                template = (ControlTemplate)this.FindResource("IncidentPushpinTemplate");
            }
            pin.Location = new Microsoft.Maps.MapControl.WPF.Location(lat, lon);
            pin.ToolTip = ToolTipText;
            pin.Content = text;
            pin.Template = template;
            pin.MouseDown += pushpin_MouseDown;
            pin.Cursor = System.Windows.Input.Cursors.Hand;
            //Add the Pushpin
            myMap.Children.Add(pin);
        }

        private void pushpin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Pushpin p = (Pushpin)sender;
            if (p.Content.ToString() == "Incident")
            {
                MessageBox.Show(p.ToolTip.ToString());
            }
            else
            {
                MapDirectionsForm _form = new MapDirectionsForm(originLoc.ToString(), p.Location.ToString());
                _form.Show();
            }
        }

        public void GetResponse(Uri uri, Action<Response> callback)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Response));

                        if (callback != null)
                        {
                            callback(ser.ReadObject(stream) as Response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void GetRoute(string fromloc, string toloc, Map myMap)
        {
            string to = toloc;//"Seattle";
            string from = fromloc;//"New York";

            if (!string.IsNullOrWhiteSpace(from))
            {
                if (!string.IsNullOrWhiteSpace(to))
                {
                    GetKey((c) =>
                    {
                        Route(from, to, c, (r) =>
                        {
                            if (r != null &&
                                r.ResourceSets != null &&
                                r.ResourceSets.Length > 0 &&
                                r.ResourceSets[0].Resources != null &&
                                r.ResourceSets[0].Resources.Length > 0)
                            {
                                Route route = r.ResourceSets[0].Resources[0] as Route;

                                double[][] routePath = route.RoutePath.Line.Coordinates;
                                LocationCollection locs = new LocationCollection();

                                for (int i = 0; i < routePath.Length; i++)
                                {
                                    if (routePath[i].Length >= 2)
                                    {
                                        locs.Add(new Microsoft.Maps.MapControl.WPF.Location(routePath[i][0], routePath[i][1]));
                                    }
                                }

                                MapPolyline routeLine = new MapPolyline()
                                {
                                    Locations = locs,
                                    Stroke = new SolidColorBrush(Colors.Red),
                                    StrokeThickness = 5
                                };

                                myMap.Children.Add(routeLine);
                                myMap.SetView(locs, new Thickness(30), 0);
                            }
                            else
                            {
                                MessageBox.Show("No Results found.");
                            }
                        });
                    });
                }
                else
                {
                    MessageBox.Show("Invalid End location.");
                }
            }
            else
            {
                MessageBox.Show("Invalid Start location.");
            }
        }

        public void GetKey(Action<string> callback)
        {
            if (callback != null)
            {
                myMap.CredentialsProvider.GetCredentials((c) =>
                {
                    callback(c.ApplicationId);
                });
            }
        }

        public string GetMapKey(Map myMap)
        {
            string key = "";
            myMap.CredentialsProvider.GetCredentials((c) =>
            {
                key = c.ApplicationId;
            });
            return key;
        }

        public Microsoft.Maps.MapControl.WPF.Location GetLatLongFromAddress(string street, string city, string state)
        {
            Microsoft.Maps.MapControl.WPF.Location loc;
            // Get the Location for a street address
            if (!string.IsNullOrWhiteSpace(city) && !string.IsNullOrWhiteSpace(state))
            {
                string bingMapsUri = string.Format("http://dev.virtualearth.net/REST/v1/Locations?q={1} {2}, {3} &o=xml&key={0}", GetMapKey(myMap), street, city, state);
                XmlDocument bingMapsXmlDoc = new XmlDocument();
                bingMapsXmlDoc.Load(bingMapsUri);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(bingMapsXmlDoc.NameTable);
                nsmgr.AddNamespace("rest", "http://schemas.microsoft.com/search/local/ws/rest/v1");
                string sLong = bingMapsXmlDoc.DocumentElement.SelectSingleNode(@".//rest:Longitude", nsmgr).InnerText;
                string sLat = bingMapsXmlDoc.DocumentElement.SelectSingleNode(@".//rest:Latitude", nsmgr).InnerText;
                loc = new Microsoft.Maps.MapControl.WPF.Location(Convert.ToDouble(sLat), Convert.ToDouble(sLong));
                return loc;
            }
            else
            {
                MessageBox.Show("City and State are required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }


        }

        public string GetAddressFromLoc(Microsoft.Maps.MapControl.WPF.Location o)
        {
            string sAddr = "";
            // Get the Address for a Location
            try
            {
                string bingMapsUri = string.Format("http://dev.virtualearth.net/REST/v1/Locations/{1},{2}?o=xml&key={0}", GetMapKey(myMap), o.Latitude, o.Longitude);
                XmlDocument bingMapsXmlDoc = new XmlDocument();
                bingMapsXmlDoc.Load(bingMapsUri);
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(bingMapsXmlDoc.NameTable);
                nsmgr.AddNamespace("rest", "http://schemas.microsoft.com/search/local/ws/rest/v1");
                sAddr = bingMapsXmlDoc.DocumentElement.SelectSingleNode(@".//rest:Name", nsmgr).InnerText;
                return sAddr;
            }
            catch
            {
                return sAddr;
            }

        }


        private void MyCommunicator_Callback(Communicator.mydata data)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    Communicator.CallbackEventHandler d = new Communicator.CallbackEventHandler(MyCommunicator_Callback);
                    Dispatcher.BeginInvoke(d, data);
                }
                else
                {
                    //Update the information for the bound label
                    labelData.MyWaitLabelOutput = data.Addressee + " : " + data.Message;
                }
            }
            catch
            {
                //Application is closing, do nothing
            }
        }

        private void loadCodes()
        {
            //Make our binding list
            Codedata = new List<CodeObj>();

            //Load our legacy Com codes
            Class1 LegacyClass;

            LegacyClass = new Class1();
            VBA.Collection MyClasses = LegacyClass.GetIncidentCodes();

            Class2 LegacyCodes;

            //Add all codes to List
            for (int i = 1; i <= MyClasses.Count(); i++)
            {
                LegacyCodes = MyClasses.Item(i);
                Codedata.Add(new CodeObj(LegacyCodes));
                unloadComObj(LegacyCodes);
            }

            //Free Com Objects
            unloadComObj(MyClasses);
            unloadComObj(LegacyClass);
            CoFreeUnusedLibraries();
        }

        private void loadStates()
        {
            //Make our State binding list
            Statedata = new List<myItem>();

            Statedata.Add(new myItem() { Name = "Massachusetts", Value = "MA" });
            Statedata.Add(new myItem() { Name = "Connecticut", Value = "CT" });
            Statedata.Add(new myItem() { Name = "New Hampshire", Value = "NH" });
            Statedata.Add(new myItem() { Name = "Maine", Value = "ME" });
            Statedata.Add(new myItem() { Name = "Vermont", Value = "VT" });
        }

        private void unloadComObj(object m)
        {
            while (System.Runtime.InteropServices.Marshal.ReleaseComObject(m) > 0)
            {
                //Explicity release Com objects when done using them
            }
            m = null;
        }

        private void btnCall_Click(object sender, RoutedEventArgs e)
        {
            Boolean returnbool;

            try
            {
                returnbool = myCommunicator.SendAlert(txtOfficer.Text, "A Test String");
            }
            catch (SecureCommunicationException ex)
            {
                Exception test = ex;
                while (test.InnerException != null)
                {
                    test = test.InnerException;
                }
                //Update the information for the bound label
                labelData.MyWaitLabelOutput = test.Message;
            }
        }

        private void MainWindow1_Closed(object sender, EventArgs e)
        {
            //Properly dispose of myCommunicator
            myCommunicator.Dispose();
        }

        private void btnLocate_click(object sender, RoutedEventArgs e)
        {
            originLoc = GetLatLongFromAddress(this.txtStreet.Text, this.txtCity.Text, this.cmbState.SelectedValue.ToString());
            if (originLoc != null)
            {
                List<myOfficer> officers = loadOfficerFile(originLoc.Latitude, originLoc.Longitude);
                showOnMap(originLoc, officers, myMap);
            }
        }

        private void showOnMap(Microsoft.Maps.MapControl.WPF.Location origin, List<myOfficer> officers, Map myMap)
        {
            //Show Origin and Officer pushpins on the map
            LocationCollection locs = new LocationCollection();
            myMap.Children.Clear();
            AddPushPin(origin.Latitude, origin.Longitude, "Incident", myMap, true, "Incident: " + Environment.NewLine + GetAddressFromLoc(origin));
            locs.Add(new Microsoft.Maps.MapControl.WPF.Location(origin.Latitude, origin.Longitude));
            foreach (myOfficer o in officers)
            {
                AddPushPin(o.latitude, o.longitude, o.Id, myMap, false, GetToolTipText(o));
                locs.Add(o.loc);
            }
            myMap.SetView(locs, new Thickness(30), 0);
        }

        //Get the text for a tool tip that display's name, location address and status.
        private string GetToolTipText(myOfficer o)
        {
            return "Officer:" + o.Id + Environment.NewLine + o.addr + Environment.NewLine + o.status;
        }

        //Load the Officer XML file we created with OfficerStatus
        public List<myOfficer> loadOfficerFile(double originLat, double originLon)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(SecureWebService.Constants.FilePath);

            myOfficer tempOfficer;
            foreach (XmlElement el in doc.DocumentElement)
            {
                tempOfficer = new myOfficer();
                tempOfficer.latitude = Convert.ToDouble(el.Attributes["latitude"].Value.ToString());
                tempOfficer.longitude = Convert.ToDouble(el.Attributes["longitude"].Value.ToString());
                tempOfficer.Id = el.Attributes["id"].Value.ToString();
                tempOfficer.status = el.Attributes["status"].Value.ToString();
                tempOfficer.distance = mathStuff.distance(tempOfficer.latitude, tempOfficer.longitude, originLat, originLon, 'M');
                tempOfficer.addr = GetAddressFromLoc(tempOfficer.loc);

                //Only load 2 Officers
                if (officers.Count < 2)
                {
                    officers.Add(tempOfficer);
                }
                else
                {
                    //We want the closest 2 Officers
                    myOfficer temp;
                    myOfficer temp0 = officers[0];
                    myOfficer temp1 = officers[1];
                    officers.Clear();
                    if (temp0.distance > tempOfficer.distance)
                    {
                        temp = temp0;
                        temp0 = tempOfficer;
                        tempOfficer = temp;
                        if (temp1.distance > tempOfficer.distance)
                        {
                            temp = temp1;
                            temp1 = tempOfficer;
                            tempOfficer = temp;
                        }
                    }
                    else if (temp1.distance > tempOfficer.distance)
                    {
                        temp = temp1;
                        temp1 = tempOfficer;
                        tempOfficer = temp;
                    }
                    officers.Add(temp0);
                    officers.Add(temp1);
                }
            }
            return officers;
        }

        public class MyWaitLabelViewModel : INotifyPropertyChanged
        {
            //Databound object for MyWaitLabel
            public MyWaitLabelViewModel() { }

            private String _MyWaitLabelOutput;
            public String MyWaitLabelOutput
            {
                get
                {
                    return _MyWaitLabelOutput;
                }
                set
                {
                    _MyWaitLabelOutput = value;
                    //Notify the label on my form to update
                    this.OnPropertyChanged("MyWaitLabelOutput");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = this.PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

    }

}
