using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Newtonsoft.Json;
using Piceon.DatabaseAccess;

namespace Piceon.Services
{
    public static class AddressTaggingService
    {
        private static HttpClient client = new HttpClient();
        public static async Task TagImageAddressAsync(string id)
        {
            string path = await DatabaseAccessService.GetImagePathAsync(id);
            var image = await StorageFile.GetFileFromPathAsync(path);

            await TagAddressAsync(image);
            var tags = await GetTagsAsync(image);
            foreach( string tag in tags)
            {
                await DatabaseAccessService.InsertImageTagAsync(id, tag);
            }
        }
        public static async Task TagImageAddressAsync(int id)
        {
            await TagImageAddressAsync(id.ToString());
        }
        private static async Task TagAddressAsync(StorageFile image)
        {
            var latlon = await GetCoordinatesAsync(image);
            var address = await GetAddressAsync(latlon);
            var prop = await image.Properties.GetImagePropertiesAsync();

            if (address.City.Length > 0 && !prop.Keywords.Contains(address.City))
            {
                prop.Keywords.Add(address.City);
            }
            if (address.Region.Length > 0 && !prop.Keywords.Contains(address.Region))
            {
                prop.Keywords.Add(address.Region);
            }
            if (address.Subregion.Length > 0 && !prop.Keywords.Contains(address.Subregion))
            {
                prop.Keywords.Add(address.Subregion);
            }
            if (address.Neighborhood.Length > 0 && !prop.Keywords.Contains(address.Neighborhood))
            {
                prop.Keywords.Add(address.Neighborhood);
            }
            if (address.CountryCode.Length > 0 && !prop.Keywords.Contains(address.CountryCode))
            {
                prop.Keywords.Add(address.CountryCode);
            }
            if (address.Territory.Length > 0 && !prop.Keywords.Contains(address.Territory))
            {
                prop.Keywords.Add(address.Territory);
            }
            await prop.SavePropertiesAsync();
        }
        private static async Task<Tuple<double, double>> GetCoordinatesAsync(StorageFile image)
        {
            var prop = await image.Properties.GetImagePropertiesAsync();
            var lat = prop.Latitude.GetValueOrDefault();
            var lon = prop.Longitude.GetValueOrDefault();
            return new Tuple<double, double>(Math.Round(lat, 10), Math.Round(lon, 10));
        }
        private static async Task<Address> GetAddressAsync(Tuple<double, double> latlon)
        {
            var lat = latlon.Item1.ToString().Replace(',', '.');
            var lon = latlon.Item2.ToString().Replace(',', '.');
            string url = $"http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/reverseGeocode?location={lon},{lat}&langCode=pl&f=json";
            var uri = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));

            Root root = null;
            using (HttpResponseMessage response = await client.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    var rootJson = await response.Content.ReadAsStringAsync();
                    root = JsonConvert.DeserializeObject<Root>(rootJson);
                }
            }
            return root.address;
        }
        private static async Task<List<string>> GetTagsAsync(StorageFile image)
        {
            var prop = await image.Properties.GetImagePropertiesAsync();
            return prop.Keywords.ToList();
        }
    }
    public class Root
    {
        public Address address { get; set; }
    }
    public class Address
    {
        public string Neighborhood { get; set; }
        public string City { get; set; }
        public string Subregion { get; set; }
        public string Region { get; set; }
        public string CountryCode { get; set; }
        public string Territory { get; set; }
    }
}
