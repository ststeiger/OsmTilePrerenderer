
using OsmSharp;
using OsmSharp.Streams;
using OsmSharp.Geo; // for ToFeatureSource 

// using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

using System.Linq;



namespace OsmTilePrerenderer
{


    class Program
    {



        // https://github.com/AliFlux/VectorTileRenderer
        static void Main(string[] args)
        {
            ReadGeometryStream();
            System.Console.WriteLine("Hello World!");


            var viewport = new Mapsui.Viewport
            {
                Center = new Mapsui.Geometries.Point(0, 0),
                Width = 600,
                Height = 400,
                Resolution = 63000
            };


            Mapsui.Map map = CreateMap();

            var mr = new Mapsui.Rendering.Skia.MapRenderer();
            
            using (System.IO.MemoryStream ms = mr.RenderToBitmapStream(viewport, map.Layers, map.BackColor))
            {
                byte[] bytes = ms.ToArray();
                System.IO.File.WriteAllBytes("GuessFileFormat.png", bytes);
            } // End Using System.IO.MemoryStream ms 


        } // End Sub Main 



        private static Mapsui.Layers.MemoryLayer CreatePointLayer()
        {
            var random = new System.Random();
            var features = new Mapsui.Providers.Features();
            for (var i = 0; i < 100; i++)
            {
                var feature = new Mapsui.Providers.Feature
                {
                    Geometry = new Mapsui.Geometries.Point(random.Next(100000, 5000000), random.Next(100000, 5000000))
                };
                features.Add(feature);
            } // Next i 


            var provider = new Mapsui.Providers.MemoryProvider(features);

            return new Mapsui.Layers.MemoryLayer { DataSource = provider };
        }



        // D:\Stefan.Steiger\Documents\Visual Studio 2017\Projects\Mapsui\Tests\Mapsui.Rendering.Xaml.Tests\MapRendererTests.cs

        public static void foo()
        {
            OsmStreamSource source = null;

            var merger = new OsmSharp.Streams.Filters.OsmStreamFilterMerge();
            merger.RegisterSource(source);
        }


        public static Mapsui.Map CreateMap()
        {
            var map = new Mapsui.Map
            {
                BackColor = Mapsui.Styles.Color.Transparent,
                Home = n => n.NavigateTo(new Mapsui.Geometries.Point(0, 0), 63000)
            };

            // OsmStreamSource source = null;
            // OsmSharp.Geo.Streams.IFeatureStreamSource features = null;
            // System.Collections.Generic.List<IFeature> features = null; ;
            NetTopologySuite.Features.FeatureCollection features = new NetTopologySuite.Features.FeatureCollection();


            // new Mapsui.Providers.MemoryProvider()
            Mapsui.Providers.IProvider source = null; // new Mapsui.Providers.MemoryProvider(features);

            Mapsui.Layers.ILayer layer = new Mapsui.Layers.MemoryLayer
            {
                Style = null,
                DataSource = source,
                Name = "Line"
            };

            map.Layers.Add(layer);
            return map;
        }


        static void ReadXmlStream()
        {
            string fileName = @"D:\username\Documents\Visual Studio 2017\Projects\OsmTilePrerenderer\OsmTilePrerenderer\Data\monaco-latest.osm.pbf";

            using (System.IO.FileStream fileStream = System.IO.File.OpenRead(fileName))
            {
                var target = new XmlOsmStreamTarget(fileStream);

                // var filtered = target.FilterSpatial(polygon, true);
                // target.RegisterSource(filtered);

                target.Pull();
            }
            
        }


        static void ReadGeometryStream()
        {
            // let's show you what's going on.
            OsmSharp.Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                System.Console.WriteLine(string.Format("[{0}] {1} - {2}", origin, level, message));
            };

            // Download.ToFile("http://files.itinero.tech/data/OSM/planet/europe/luxembourg-latest.osm.pbf", "luxembourg-latest.osm.pbf").Wait();

            using (System.IO.FileStream fileStream = System.IO.File.OpenRead(@"D:\username\Documents\Visual Studio 2017\Projects\OsmTilePrerenderer\OsmTilePrerenderer\Data\monaco-latest.osm.pbf"))
            {
                // create source stream.
                OsmStreamSource source = new PBFOsmStreamSource(fileStream);

                // show progress.
                OsmStreamSource progress = source.ShowProgress();

                // filter all powerlines and keep all nodes.
                System.Collections.Generic.IEnumerable<OsmGeo> filtered = 
                    from osmGeo in progress
                    where osmGeo.Type == OsmSharp.OsmGeoType.Node ||
                            (osmGeo.Type == OsmSharp.OsmGeoType.Way && osmGeo.Tags != null && osmGeo.Tags.Contains("power", "line"))
                    select osmGeo;

                // convert to a feature stream.
                // WARNING: nodes that are partof powerlines will be kept in-memory.
                //          it's important to filter only the objects you need **before** 
                //          you convert to a feature stream otherwise all objects will 
                //          be kept in-memory.

                OsmSharp.Geo.Streams.IFeatureStreamSource features = filtered.ToFeatureSource();

                // filter out only linestrings.
                System.Collections.Generic.IEnumerable<NetTopologySuite.Features.IFeature> lineStrings = from feature in features
                                  where feature.Geometry is LineString
                                  select feature;

                // build feature collection.
                NetTopologySuite.Features.FeatureCollection featureCollection = new NetTopologySuite.Features.FeatureCollection();
                foreach (NetTopologySuite.Features.IFeature feature in lineStrings)
                {
                    featureCollection.Add(feature);
                }


                // convert to geojson.
                string json = ToJson(featureCollection);



                // var st = new Mapsui.Providers.MemoryProvider(json);
                System.IO.File.WriteAllText("output.geojson", json);
            }
        }
        

        private static string ToJson(NetTopologySuite.Features.FeatureCollection featureCollection)
        {
            Newtonsoft.Json.JsonSerializer jsonSerializer = NetTopologySuite.IO.GeoJsonSerializer.Create();
            System.IO.TextWriter jsonStream = new System.IO.StringWriter();
            jsonSerializer.Serialize(jsonStream, featureCollection);
            string json = jsonStream.ToInvariantString();
            return json;
        }


    }


}
