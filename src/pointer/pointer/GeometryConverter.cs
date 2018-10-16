using GeoAPI.Geometries;
using GeoJSON.Net.Geometry;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace pointer
{
    public static class GeometryConverter
    {
        public static IGeometry GetGeoApiGeometry(IGeometryObject geom)
        {
            var geomtype = geom.Type;
            var geomstring = JsonConvert.SerializeObject(geom);
            var reader = new GeoJsonReader();
            IGeometry pandGeom;
            if (geomtype == GeoJSON.Net.GeoJSONObjectType.MultiPolygon)
            {
                pandGeom = reader.Read<NetTopologySuite.Geometries.MultiPolygon>(geomstring);
            }
            else
            {
                pandGeom = reader.Read<NetTopologySuite.Geometries.Polygon>(geomstring);
            }
            return pandGeom;
        }

    }
}
