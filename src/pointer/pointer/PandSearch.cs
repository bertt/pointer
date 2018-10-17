using GeoJSON.Net.Feature;
using System;
using System.Collections.Generic;

namespace pointer
{
    public static class PandSearch
    {
        public static (Feature pand, double distance) GetNearestPand(List<Feature> panden, NetTopologySuite.Geometries.Point loc1, NetTopologySuite.Geometries.LineString line, double MinimumDistance)
        {
            Feature nearestPand = null;
            double distanceNearest = Double.MaxValue;

            foreach (var pand in panden)
            {
                var geom = pand.Geometry;
                var pandGeom = GeometryConverter.GetGeoApiGeometry(geom);
                var intersects = pandGeom.Intersects(line);

                if (intersects)
                {
                    var dist = loc1.Distance(pandGeom);

                    if (dist < distanceNearest && dist > MinimumDistance / 111000)
                    {
                        distanceNearest = dist;
                        nearestPand = pand;
                    }
                }
            }
            return (nearestPand, distanceNearest);
        }


    }
}
