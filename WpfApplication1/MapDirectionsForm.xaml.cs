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
using System.Windows.Shapes;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MapDirectionsForm : Window
    {
        public MapDirectionsForm(string fromloc, string toloc)
        {
            InitializeComponent();
            GetRoute(fromloc, toloc, myMap);
            this.SizeToContent = SizeToContent.Height;
        }

        private void GetRoute(string fromloc, string toloc, Map myMap)
        {
            string to = toloc;
            string from = fromloc;

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
                                //Make our Point collection for the Map route
                                for (int i = 0; i < routePath.Length; i++)
                                {
                                    if (routePath[i].Length >= 2)
                                    {
                                        locs.Add(new Microsoft.Maps.MapControl.WPF.Location(routePath[i][0], routePath[i][1]));
                                    }
                                }
                                //Make our Turn by turn Directions
                                for (int i = 0; i < route.RouteLegs.Length; i++)
                                {
                                    RouteLeg temp = route.RouteLegs[i];
                                    DrivingDirections.FontSize = 24;
                                    DrivingDirections.TextWrapping = TextWrapping.Wrap;
                                    for (int j = 0; j < temp.ItineraryItems.Length; j++)
                                    {
                                        ItineraryItem itinTemp = temp.ItineraryItems[j];
                                        DrivingDirections.Text += Convert.ToString(j + 1) + ": " + itinTemp.Instruction.Text + Environment.NewLine;
                                    }
                                }
                                //Add our route to the Map
                                MapPolyline routeLine = new MapPolyline()
                                {
                                    Locations = locs,
                                    Stroke = new SolidColorBrush(Colors.Red),
                                    StrokeThickness = 5
                                };

                                myMap.Children.Add(routeLine);
                                //Set our map View
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

        public void Route(string start, string end, string key, Action<Response> callback)
        {
            Uri requestURI = new Uri(string.Format("http://dev.virtualearth.net/REST/V1/Routes/Driving?wp.0={0}&wp.1={1}&rpo=Points&key={2}", Uri.EscapeDataString(start), Uri.EscapeDataString(end), key));
            GetResponse(requestURI, callback);
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
    }
}
