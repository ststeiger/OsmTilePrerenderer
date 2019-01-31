
namespace OsmSharp.IO.Binary
{


    public class Class1
    {


        static void Test(string filename1, string filename2)
        {
            using (var sourceStream = System.IO.File.OpenRead(filename1))
            using (var targetStream = System.IO.File.Open(filename2, System.IO.FileMode.Create))
            {
                // Streams.OsmStreamSource sourceXML = new Streams.XmlOsmStreamSource(sourceStream);
                Streams.OsmStreamSource sourceBinary = new Streams.BinaryOsmStreamSource(sourceStream);


                Streams.XmlOsmStreamTarget targetXml = new Streams.XmlOsmStreamTarget(targetStream);
                targetXml.RegisterSource(sourceBinary);
                targetXml.Pull();
            }
        }

        public void BinaryOsmStreamSource_ShouldReadNode(System.IO.FileStream stream2)
        {
            using (System.IO.Stream stream = stream2)
            {
                Streams.OsmStreamSource sourceStream = new Streams.BinaryOsmStreamSource(stream);
                System.Collections.Generic.List<OsmGeo> nodes = System.Linq.Enumerable.ToList(sourceStream);
            }
        }


    }


}
