using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;

namespace pointer
{
    public static class LineCreator
    {
        public static LineString GetLine(double longitude, double latitude, double headingNorth, double distance)
        {
            var dist = distance / 111000;
            var c2 = new Coordinate(longitude + Math.Sin(headingNorth) * dist, latitude + Math.Cos(headingNorth) * dist);
            var c1 = new Coordinate(longitude, latitude);

            var line = new LineString(new Coordinate[] { c1, c2 });
            return line;
        }

    }
}
