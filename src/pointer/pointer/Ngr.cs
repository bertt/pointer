using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace pointer
{
    public class Ngr
    {
        public static List<Feature> GetPanden(string envelope)
        {
            var client = new HttpClient();

            var url = $"http://geodata.nationaalgeoregister.nl/bag/wfs?REQUEST=GetFeature&SERVICE=WFS&VERSION=2.0.0&TYPENAME=bag:pand&SRSNAME=EPSG:4326&cql_filter=(bbox(geometrie,{envelope},%27EPSG:4326%27))&outputformat=application/json";

            var response = client.GetAsync(url).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;

            var featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(responseString);
            return featureCollection.Features;

        }

    }
}
