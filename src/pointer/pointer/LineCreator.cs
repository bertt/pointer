using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;

namespace pointer
{
    public static class LineCreator
    {
        public static LineString GetLine(double longitude, double latitude, double headingNorth)
        {
            var c2 = new Coordinate(longitude + Math.Sin(headingNorth) * 0.001, latitude + Math.Cos(headingNorth) * 0.001);
            var c1 = new Coordinate(longitude, latitude);

            var line = new LineString(new Coordinate[] { c1, c2 });
            return line;
        }

    }
}
