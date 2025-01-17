using System.Text.Json;

namespace HjsonSharp.Tests;

public class JsoncTests {
    [Fact]
    public void ParseExampleTest() {
        // Example from https://thelinuxcode.com/what-is-jsonc-are-jsonc-json-c-different

        string Text = """
            {
              "name": "John Smith",
                // Date of Birth for user 
                "dob": "1987-11-23",

                /* Indicates if user has 

                completed registration */
                "registered": true,   


                /* User location coordinates.  

                Stored as longitude/latitude  

                for geospatial queries */
                "coordinates": {

                "longitude": "44.3421",
                "latitude": "22.1234"
                },
            }
            """;
        var AnonymousObject = new {
            name = "John Smith",
            // Date of Birth for user 
            dob = "1987-11-23",

            /* Indicates if user has 

            completed registration */
            registered = true,   


            /* User location coordinates.  

            Stored as longitude/latitude  

            for geospatial queries */
            coordinates = new {
                longitude = "44.3421", // TODO: Replace with number
                latitude = "22.1234" // TODO: Replace with number
            },
        };

        JsonElement Element = HjsonReader.ParseElement(Text, HjsonReaderOptions.Jsonc).Value;
        Assert.Equal(JsonSerializer.Serialize(AnonymousObject), JsonSerializer.Serialize(Element));
    }
}